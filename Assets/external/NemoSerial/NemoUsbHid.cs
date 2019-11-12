//#define  TestSerial
 
using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NemoUsbHid : MonoBehaviour 
{

    public class Package
    {
        public byte[] data;

    }

    public delegate void RecivePackage(Package pack);
    public RecivePackage HandleRecivePackage;

    /// <summary>
    /// 打包处理函数
    /// </summary>
    /// <param name="byteReaded"></param>
    /// <returns></returns>
    public delegate List<Package> Handle_SetupPackage(byte[] byteReaded);
    public Handle_SetupPackage HandleSetupPackage;

    public delegate void GenericEvent();
    public GenericEvent EvtOpened;
    public GenericEvent EvtClosed;
    public int PID;
    public int VID;

    private SerialPort mSerialPort;


    //private Thread mSerialThread;   // 串口线程

    //private Thread mThreadSend;   
    private Thread mThreadRecive;  
    //private bool mThreadLoopSend = true;
    private bool mThreadLoopRecive = true;


    private byte[] mReadBuffer; // 读取buf 
    

    private System.Object mSendPack_ThreadLock;  // 发包锁
    private System.Object mRecivePack_ThreadLock;  // 收包锁

    private static uint READ_BUFFER_SIZE = 65;


    private List<Package> mPackToSendMT;//主线程(Main Thread)数据发出队列
    private List<Package> mPackToSendST;//串口线程(SerialPort Thread)数据发出队列

    private List<Package> mPackToReciveMT;//主线程(Main Thread)数据接收队列
    private List<Package> mPackToReciveST;//串口线程(SerialPort Thread)数据接收队列



    private IntPtr mIOHandler = IntPtr.Zero;
    //private int mInputReportLen;
    private int mOutputReportLen;

    //Win32Usb.Overlapped ol = new Win32Usb.Overlapped();
    Win32Usb.Overlapped mReadOL;
    Win32Usb.Overlapped mWriteOL;
    /// <summary>
    /// Unity关闭时调用
    /// </summary>
    void OnDestroy()
    {
        Close();
    }

    /// <summary>
    /// Unity调用
    /// </summary>
    void Update()
    {
        MainThreadUpdate();
    }

    void Start()
    {
         
        WndMsgHook.Evt_DeviceArrived += Handle_Device_Arrival;
        WndMsgHook.Evt_DeviceRemoved += Handle_Device_RemoveComplete;
    

        InitVals();
        Open();
    }
    void InitVals()
    {
        if (mSendPack_ThreadLock == null)
            mSendPack_ThreadLock = new System.Object();
        if (mRecivePack_ThreadLock == null)
            mRecivePack_ThreadLock = new System.Object();

        if (mPackToSendMT == null)
            mPackToSendMT = new List<Package>();
        if (mPackToSendST == null)
            mPackToSendST = new List<Package>();

        if (mPackToReciveMT == null)
            mPackToReciveMT = new List<Package>();
        if (mPackToReciveST == null)
            mPackToReciveST = new List<Package>();
    }
    /// <summary>
    /// 打开 portNo串口号
    /// </summary>
    /// <param name="portNo">串口号</param>
    /// <returns>是否打开成功</returns>
    public bool Open()
    {
 
        if ( mIOHandler != IntPtr.Zero)
            return true;
        InitVals();
        try
        {

            string devicePath = FindDevicePath(VID, PID);
 
            if (devicePath == null || devicePath == "")
            {
                return false;
            }


            mIOHandler = Win32Usb.CreateFile(devicePath, Win32Usb.GENERIC_READ | Win32Usb.GENERIC_WRITE
                        , Win32Usb.FILE_SHARE_READ | Win32Usb.FILE_SHARE_WRITE, IntPtr.Zero, Win32Usb.OPEN_EXISTING, Win32Usb.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
             
            if (mIOHandler != Win32Usb.InvalidHandleValue)	// if the open worked...
            {
                IntPtr lpData;
                if (Win32Usb.HidD_GetPreparsedData(mIOHandler, out lpData))	// get windows to read the device data into an internal buffer
                {
                    try
                    {
                        Win32Usb.HidCaps oCaps;
                        Win32Usb.HidP_GetCaps(lpData, out oCaps);	// extract the device capabilities from the internal buffer
                        //mInputReportLen = oCaps.InputReportByteLength;	// get the input...
                        mOutputReportLen = oCaps.OutputReportByteLength;	// ... and output report lengths
                        //Debug.Log(string.Format("mInputReportLen = {0} mOutputReportLen = {1}",mInputReportLen,mOutputReportLen));
                        //m_oFile = new FileStream(m_hHandle, FileAccess.Read | FileAccess.Write, true, m_nInputReportLength, true);
                        //m_oFile = new FileStream(new SafeFileHandle(m_hHandle, false), FileAccess.Read | FileAccess.Write, m_nInputReportLength, true);

                        //BeginAsyncRead();	// kick off the first asynchronous read                              
                    }
                    catch (Exception)
                    {
                        //throw HIDDeviceException.GenerateWithWinError("Failed to get the detailed data from the hid.");
                        Debug.LogWarning("[HidUsb]得不到hid报文长度.");
                        return false;
                    }
                    finally
                    {
                        Win32Usb.HidD_FreePreparsedData(ref lpData);	// before we quit the funtion, we must free the internal buffer reserved in GetPreparsedData
                    }
                }
                else	// GetPreparsedData failed? Chuck an exception
                {
                    Debug.LogWarning("[HidUsb]获取不到hid属性.");
                    return false;
                }
            }
            else	// File open failed? Chuck an exception
            {
                mIOHandler = IntPtr.Zero;
                Debug.LogWarning("[HidUsb]打不开hid设备.");
                return false;
            }
 
            
            /*mThreadLoopSend = */mThreadLoopRecive = true;

            mReadBuffer = new byte[READ_BUFFER_SIZE];

            //mThreadSend = new Thread(Thread_Send);
            //mThreadSend.Start();

            mThreadRecive = new Thread(Thread_Recive);
            mThreadRecive.Start();


            if (EvtOpened != null)
                EvtOpened();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Open Serialport Error." + e);
            return false;
        }
    }
 
    // 关闭
    public void Close()
    {
         
            if (mIOHandler != IntPtr.Zero)
            { 

                //if (Win32Usb.WaitForSingleObjectEx(mIOHandler, 0xffffff, true) == 0)
                {

                    Win32Usb.CancelIo(mIOHandler);
                    Win32Usb.CloseHandle(mIOHandler);
                    mIOHandler = IntPtr.Zero;
                   
                    //Debug.Log("Marshal.GetLastWin32Error() = " + Marshal.GetLastWin32Error());
                   /* mThreadLoopSend =*/ mThreadLoopRecive = false;

                    //if (mThreadSend != null)
                    //    mThreadSend.Join();

                    if (mThreadRecive != null)
                        mThreadRecive.Join();

                    if (EvtClosed!= null)
                        EvtClosed();
                }
            }

   
            //if(!)//该函数会阻塞2秒,
            //{
            //    Debug.LogError("Close SerialThread Erro!");
            //}
   
    
    }
 

    
    //private int debugMainThreadLoopNum = 0;
    /// <summary>
    /// 主线程Update(一般由外部线程调用)
    /// <param name = "delta">上一帧使用时间</param>
    /// </summary>
   
    public void MainThreadUpdate(/*float delta*/)
    {

 

        if (Monitor.TryEnter(mRecivePack_ThreadLock))
        {
            List<Package> tempList = mPackToReciveMT;
            mPackToReciveMT = mPackToReciveST;
            mPackToReciveST = tempList;
            Monitor.Exit(mRecivePack_ThreadLock);
        }


        foreach (Package p in mPackToReciveMT)
        {
            HandleRecivePackage(p);
        }
        mPackToReciveMT.Clear();
    }

 

    private void Thread_Recive()
    {
        uint readCount = 0;
 
        Win32Usb.ReadFile(mIOHandler, mReadBuffer, READ_BUFFER_SIZE, ref readCount,ref mReadOL);
        while (mThreadLoopRecive && mIOHandler != IntPtr.Zero)
        {
            try
            { 
                //Debug.Log("d1," + mReadBuffer.Length+"  mReadBufCount="+mReadBufCount+" READ_BUFFER_SIZE="+READ_BUFFER_SIZE);

                //接收包
                if (/*Win32Usb.WaitForSingleObjectEx(mIOHandler,0,true) ==0&&*/
                    Win32Usb.GetOverlappedResult(mIOHandler, ref mReadOL, ref readCount, false))//有信号
                {
                    if (HandleSetupPackage != null)
                    {

                        byte[] paramDatas = new byte[readCount];
                        Array.Copy(mReadBuffer, paramDatas, readCount);
                        List<Package> packages = HandleSetupPackage(paramDatas);
                        //Debug.Log(ByteArrayToString(paramDatas));
                        if (packages.Count != 0)
                        {
                            Monitor.Enter(mRecivePack_ThreadLock);
                            foreach (Package p in packages)
                                mPackToReciveST.Add(p);
                            Monitor.Exit(mRecivePack_ThreadLock);
                        }
                    }


                    mReadOL.Clear();
                    Win32Usb.ReadFile(mIOHandler, mReadBuffer, READ_BUFFER_SIZE, ref readCount, ref mReadOL);

                }


                // 发送包
                mPackToSendST.Clear();
                List<Package> tempList = mPackToSendST;

                if (Monitor.TryEnter(mSendPack_ThreadLock))
                {
                    mPackToSendST = mPackToSendMT;
                    mPackToSendMT = tempList;
                    Monitor.Exit(mSendPack_ThreadLock);
                }

                uint numWrite = 0;
                foreach (Package dtw in mPackToSendST)
                {
                    mWriteOL.Clear();
                    //mWriteOL.Event = Win32Usb.CreateEvent(IntPtr.Zero, true, false, IntPtr.Zero);
                    byte[] writeData = new byte[mOutputReportLen];//可能会有内存泄露
                    Array.Copy(dtw.data, 0, writeData, 1, dtw.data.Length);
                    
                    //if (!Win32Usb.WriteFile(mIOHandler, dtw.data, (uint)dtw.data.Length, ref numWrite, ref mWriteOL))
                    Win32Usb.WriteFile(mIOHandler, writeData, (uint)writeData.Length, ref numWrite, ref mWriteOL);
                    
                    
                    //if (Win32Usb.GetOverlappedResult(mIOHandler, ref mWriteOL, ref numWrite, true))
                    //{
                    //    //Debug.Log(ByteArrayToString(dtw.data) + "  numWrite = " + numWrite);
                    //}
                    //Win32Usb.WaitForSingleObjectEx(mWriteOL.Event, 0xffffffff, true);
                    //Debug.Log("11");
                } 

            }
            catch (System.TimeoutException)
            {
            }
            catch (System.Exception ex)
            {
                Debug.LogError("SerialThreadUpdate Exception:" + ex.Message);
            }

        }
    }
 
    public static string ByteArrayToString(byte[] byteArray)
    {
        string str = "";
        foreach (byte b in byteArray)
        {
            str += string.Format("{0:x2} ", b);
        }
        return str;
    }



    void Handle_Device_Arrival()
    {
         //Debug.Log("Msg_Device_Arrival");
         if (FindDevicePath(VID, PID) != "")
         {
             Open();
         }
    }

    void Handle_Device_RemoveComplete()
    {
        //Debug.Log("Msg_Device_RemoveComplete");
        if (FindDevicePath(VID, PID) == "")
        {
            Close();
        }
    }

    ////////////////////////////////////////////内部函数/////////////////////////////////////////////////////////////////////////////


    // 发送数据
    public void Send(byte[] data)
    {
        Package dtw = new Package();
        dtw.data = data;
        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(dtw);
        Monitor.Exit(mSendPack_ThreadLock);

    }

  

    private static string GetDevicePath(IntPtr hInfoSet, ref Win32Usb.DeviceInterfaceData oInterface)
    {
        uint nRequiredSize = 0;
        // Get the device interface details
        if (!Win32Usb.SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero))
        {
            Win32Usb.DeviceInterfaceDetailData oDetail = new Win32Usb.DeviceInterfaceDetailData();
            oDetail.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
            if (Win32Usb.SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, ref oDetail, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
            {
                return oDetail.DevicePath;
            }
        }
        return null;
    }

    public static string FindDevicePath(int nVid, int nPid)
    {
        //string strPath = string.Empty;
        string strSearch = string.Format("vid_{0:x4}&pid_{1:x4}", nVid, nPid); // first, build the path search string
        Guid gHid = Win32Usb.HIDGuid;
        //HidD_GetHidGuid(out gHid);	// next, get the GUID from Windows that it uses to represent the HID USB interface
        IntPtr hInfoSet = Win32Usb.SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, (uint)(Win32Usb.DIGCF_DEVICEINTERFACE | Win32Usb.DIGCF_PRESENT));	// this gets a list of all HID devices currently connected to the computer (InfoSet)
        try
        {
            Win32Usb.DeviceInterfaceData oInterface = new Win32Usb.DeviceInterfaceData();	// build up a device interface data block
            oInterface.Size = Marshal.SizeOf(oInterface);
            // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
            // to get device details for each device connected
            int nIndex = 0;
            while (Win32Usb.SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))	// this gets the device interface information for a device at index 'nIndex' in the memory block
            {
                
                string strDevicePath = GetDevicePath(hInfoSet, ref oInterface);	// get the device path (see helper method 'GetDevicePath')
             
                if (strDevicePath.IndexOf(strSearch) >= 0)	// do a string search, if we find the VID/PID string then we found our device!
                {
                    return strDevicePath;
                }
                nIndex++;	// if we get here, we didn't find our device. So move on to the next one.
            }
        }
        catch (Exception ex)
        {

            Debug.LogWarning(string.Format("[警告]打开usbhid设备错误 :{0}WinError:{1:X8}", ex.ToString(),Marshal.GetLastWin32Error()));
            //Console.WriteLine(ex.ToString());
        }
        finally
        {
            // Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
            Win32Usb.SetupDiDestroyDeviceInfoList(hInfoSet);
        }
        return "";	// oops, didn't find our device
    }
}

