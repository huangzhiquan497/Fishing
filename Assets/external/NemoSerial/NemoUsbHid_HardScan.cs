//#define RECORE_INITSTATE //??????????,???????????????
#define MOBILE_EDITION
using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

public class NemoUsbHid_HardScan : MonoBehaviour, INemoControlIO
{

 
    //?????????
    enum OutputPackCmd
    {
        OutCoin,
        OutTicket,
        FlashLight,
        RequestMCUInfo,//??????¶À?????
        EditMCUInfo,//?????¶À?????
        ReadWriteRequest,//??ß’????
    }

    //?????????
    public enum InputPackCmd
    {
        Key,
        InsertCoin,
        OutCoin,
        OutTicket,
        LackCoin,
        LackTicket,
        MCUInfo,//??¶À?????
        CtrlBoardConnectState,//??????????????
        ResultEditMCUInfo,//?????¶À????????
        ResultReadWrite,//??ß’???????
    }

    /// <summary>
    /// ????????,??ß≥??LenInputDataPerCtrller???????,????ofsCtrller_xx???
    /// </summary>
    public enum InputCmd
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        BtnA = 4,
        BtnB = 5,
        BtnC = 6,
        BtnD = 7,
        InCoin = 8,
        BtnE = 9,
        OutTicket = 10,
        OutCoin = 11
    }

    public class Package
    {

    }
    class OutputPackage : Package
    {
        public OutputPackage(OutputPackCmd cmd, int ctrllerIdx)
        {
            Cmd = cmd;
            CtrllerIdx = ctrllerIdx;
        }
        public OutputPackCmd Cmd;
        public int CtrllerIdx;//??????????
    }

    /// <summary>
    /// ?????-??
    /// </summary>
    class OutputPack_Light:OutputPackage
    {
        
        public OutputPack_Light(OutputPackCmd cmd, int ctrllerIdx, int lightIdx, bool isOn):base(cmd,ctrllerIdx)
        {
            LightIdx = lightIdx;
            IsOn = isOn;
        }
        public int LightIdx;
        public bool IsOn;
    }
    /// <summary>
    /// ?????-????/?
    /// </summary>
    class OutputPack_OutBounty : OutputPackage
    {
        public OutputPack_OutBounty(OutputPackCmd cmd, int ctrllerIdx, bool enable):base(cmd,ctrllerIdx)
        {
            Enable = enable; 
        } 
        public bool Enable;
    }

    class OutputPack_EditMCUInfo : OutputPackage
    {
        public OutputPack_EditMCUInfo(OutputPackCmd cmd, int ctrllerIdx, int gameIdx, int mainVersion, int subVersion)
            : base(cmd, ctrllerIdx)
        {
            GameIdx = gameIdx;
            MainVersion = mainVersion;
            SubVersion = subVersion;
        } 
        public int GameIdx;
        public int MainVersion;
        public int SubVersion;
    }

    /// <summary>
    /// ??ß’?????
    /// </summary>
    class OutputPack_ReadWriteRequest : OutputPackage
    {
        public OutputPack_ReadWriteRequest(OutputPackCmd cmd, bool isWrite, uint address,byte dataLength,byte[] data)
            : base(cmd, 0)
        {
            IsWrite = isWrite;
            Address = address;
            DataLength = dataLength;
            Data = data;
        }
        public bool IsWrite;//0??1ß’
        public uint Address;//???
        public byte DataLength;//???????,??¶À:???
        public byte[] Data;//????????

    }

    /// <summary>
    /// ?????-
    /// </summary>
    public class InputPackage : Package
    {
        public InputPackCmd Cmd;
        public int CtrllerIdx;

        public InputPackage(InputPackCmd cmd, int ctrllerIdx)
        {
            Cmd = cmd;
            CtrllerIdx = ctrllerIdx;
        }


    }

    /// <summary>
    /// ?????-????
    /// </summary>
    public class InputPack_Key : InputPackage
    {
        public InputCmd Key_;
        public bool DownOrUp;

        public InputPack_Key(InputPackCmd cmd, int ctrllerIdx,InputCmd key,bool downOrUp):base(cmd,ctrllerIdx)
        {
            Key_ = key;
            DownOrUp = downOrUp;
        }

    }

    class InputPack_MCUInfo : InputPackage
    {
        public bool VerifySucess;//????????
        public int GameIdx;//??????
        public int VersionMain;
        public int VersionSub;
        public InputPack_MCUInfo(InputPackCmd cmd, int ctrllerIdx, bool verifySucess, int gIdx, int verMain, int verSub)
            : base(cmd, ctrllerIdx)
        {
            VerifySucess = verifySucess;
            GameIdx = gIdx;
            VersionMain = verMain;
            VersionSub = verSub;
        }
    }

    class InputPack_CtrlBoardState : InputPackage
    {
        public int CtrlBoardId;//?????ID
        public bool ConectOrDisconect;//???????
        public InputPack_CtrlBoardState(InputPackCmd cmd, int ctrlIdx,int ctrlBoardId, bool conectOrDisConect)
            :base(cmd,ctrlIdx)
        {
            CtrlBoardId = ctrlBoardId;
            ConectOrDisconect = conectOrDisConect;
        }
    }

    class InputPack_ResultEditMCUInfo : InputPackage
    { 
        public bool Result;//???????
        public InputPack_ResultEditMCUInfo(InputPackCmd cmd, int ctrlIdx, bool result)
            : base(cmd, ctrlIdx)
        { 
            Result = result;
        }
    }

    /// <summary>
    /// ??ß’?????
    /// </summary>
    class InputPack_ResultReadWrite : InputPackage
    {
        public bool IsWrite;//0??1ß’
        public uint Address;//???
        public byte DataLength;//???????,??¶À:???
        public byte ResultCode;//??ß’???
        public byte[] Data;//????????

        public InputPack_ResultReadWrite(InputPackCmd cmd, bool isWrite, uint address, byte dataLength, byte resultCode, byte[] data)
            : base(cmd, 0)
        {
            IsWrite = isWrite;
            Address = address;
            DataLength = dataLength;
            ResultCode = resultCode;
            Data = data;
        }
        
    }
    //????????,????????????,?????ßÿ???????????(??????)
    class OutBountyElapse
    {
        public OutBountyElapse(long time)
        {
            Time = time;
        }
        public long Time;
    }

    NemoCtrlIO_EventHardwareInfo mEvtHardwareInfo;
    NemoCtrlIO_EventKey mEvtKey;
    NemoCtrlIO_EventInsertCoin mEvtInsertCoin;
    NemoCtrlIO_EventController mEvtOutCoinReflect;
    NemoCtrlIO_EventController mEvtOutTicketReflect;
    NemoCtrlIO_EventController mEvtLackCoin;
    NemoCtrlIO_EventController mEvtLackTicket;
    NemoCtrlIO_EventCtrlBoardStateChanged mEvtCtrlBoardStateChanged;
    NemoCtrlIO_EventGeneral mEvtOpened;
    NemoCtrlIO_EventGeneral mEvtClosed;
    NemoCtrlIO_EventResultReadWrite mEvtResultReadWrite;

    public NemoCtrlIO_EventHardwareInfo EvtHardwareInfo { get { return mEvtHardwareInfo; } set { mEvtHardwareInfo = value; } }
    public NemoCtrlIO_EventKey EvtKey { get { return mEvtKey; } set { mEvtKey = value; } }
    public NemoCtrlIO_EventInsertCoin EvtInsertCoin { get { return mEvtInsertCoin; } set { mEvtInsertCoin = value; } }
    public NemoCtrlIO_EventController EvtOutCoinReflect { get { return mEvtOutCoinReflect; } set { mEvtOutCoinReflect = value; } }
    public NemoCtrlIO_EventController EvtOutTicketReflect { get { return mEvtOutTicketReflect; } set { mEvtOutTicketReflect = value; } }
    public NemoCtrlIO_EventController EvtLackCoin { get { return mEvtLackCoin; } set { mEvtLackCoin = value; } }
    public NemoCtrlIO_EventController EvtLackTicket { get { return mEvtLackTicket; } set { mEvtLackTicket = value; } }
    public NemoCtrlIO_EventCtrlBoardStateChanged EvtCtrlBoardStateChanged { get { return mEvtCtrlBoardStateChanged; } set { mEvtCtrlBoardStateChanged = value; } }
    public NemoCtrlIO_EventGeneral EvtOpened { get { return mEvtOpened; } set { mEvtOpened = value; } }
    public NemoCtrlIO_EventGeneral EvtClosed { get { return mEvtClosed; } set { mEvtClosed = value; } }
    public NemoCtrlIO_EventResultReadWrite EvtResultReadWrite { get { return mEvtResultReadWrite; } set { mEvtResultReadWrite = value; } }

    public delegate void Event_ResultEditMCU(bool result);
    public Event_ResultEditMCU EvtResultEditMCU;

    public delegate bool Func_IsInvokeKeyEvent(int ctrlIdx, InputCmd k, bool keyStateDownOrUp);
    public Func_IsInvokeKeyEvent FuncHSThread_AddKeyPress;
    public delegate void Event_HSThread_FrameStart();
    public Event_HSThread_FrameStart Evt_HSThread_FrameStart;


    public int PID;
    public int VID;
    public int NumControlBoard = 1;//????????,????,?????ßÿ???????

    
    static int[,] InputCmdTrans;//???????????????,??????????⁄Ö
    static int[] BackStageInputCmdTrans;//???ß≥??????????
        
    static readonly int OfsOutput_Light0 = 0;
    static readonly int OfsOutput_Light1 = 1;
    static readonly int OfsOutput_OutCoin = 2;
    static readonly int OfsOutput_OutTicket = 3;

    static readonly int OfsInput_ControlBoardState = 240;//bit,???????,???8??bit
    

    static readonly int OfsInput_BsKeyStartBit = 249;//bit,??????????¶À??,??????????bit,???????248 
    static readonly int OfsInput_BsKeyEndBit = 256;//bit,???????????¶À??

    static readonly int LenInputBuf = 33;//???
    static readonly int LenOuputBuf = 33;//???
    static readonly int LenInputDataPerCtrller = 12; //bit,?????????????????
    static readonly int LenOuputDataPerCtrller = 4; //bit,?????????????????

    static readonly int NumCtrller = 20;//??,????????? 

    static readonly uint Timeout_Read = 1150;//?????? 
    static readonly long Timeout_Outbounty = 50000000;//tick,5000*10000 1??????10000tick,??????DateTime.Now.Tick,??¶À?100

 

    Thread mThreadRecive; 
    bool mThreadLoopRecive = true;

    System.Object mSendPack_ThreadLock;  // ??????
    System.Object mRecivePack_ThreadLock;  // ?????

    List<OutputPackage> mPackToSendMT;//?????(Main Thread)???????????
    List<OutputPackage> mPackToSendST;//???????(SerialPort Thread)???????????

    List<InputPackage> mPackToReciveMT;//?????(Main Thread)??????????
    List<InputPackage> mPackToReciveST;//???????(SerialPort Thread)??????????

    IntPtr mIOHandler = IntPtr.Zero;
    //int mInputReportLen;
    //int mOutputReportLen;

    Win32Usb.Overlapped mReadOL;
    Win32Usb.Overlapped mWriteOL;
    
    BitArray mInputBuff;//??????????? 
    BitArray mOutputBuff;

    BitArray mInputBuffInit;//???????????
    int mCurChallengeNum;//?????????MCUINFO???ChallengeNum
    
    Dictionary<int, OutBountyElapse> mOuttingCoinTag;
    Dictionary<int, OutBountyElapse> mOuttingTicketTag;

    List<InputPackage> mInputPackageFromPlugin;//?????????????(???????????????????)

    System.Random mRndObj;
    // ????
    public bool IsOpen()
    {
        return mIOHandler != IntPtr.Zero;
    }

    /// <summary>
    /// Unity????????
    /// </summary>
    void OnDestroy()
    {
        Close();
    }

    /// <summary>
    /// Unity????
    /// </summary>
    void Update()
    {
 
        RecivePackages();
    }

    void Start()
    {
#if MOBILE_EDITION
        return;
#endif
        WndMsgHook.Evt_DeviceArrived += Handle_Device_Arrival;
        WndMsgHook.Evt_DeviceRemoved += Handle_Device_RemoveComplete;
 

        InitVals();
        Open();
    }
    void InitVals()
    {
#if MOBILE_EDITION
        return;
#endif

        if (mSendPack_ThreadLock == null)
            mSendPack_ThreadLock = new System.Object();
        if (mRecivePack_ThreadLock == null)
            mRecivePack_ThreadLock = new System.Object();

        if (mPackToSendMT == null)
            mPackToSendMT = new List<OutputPackage>();
        if (mPackToSendST == null)
            mPackToSendST = new List<OutputPackage>();

        if (mPackToReciveMT == null)
            mPackToReciveMT = new List<InputPackage>();
        if (mPackToReciveST == null)
            mPackToReciveST = new List<InputPackage>();
        if (mInputPackageFromPlugin == null)
            mInputPackageFromPlugin = new List<InputPackage>();
 
    }
    /// <summary>
    /// ?? portNo?????
    /// </summary>
    /// <param name="portNo">?????</param>
    /// <returns>??????</returns>
    public bool Open()
    {
#if MOBILE_EDITION
        return false;
#endif

        if (mIOHandler != IntPtr.Zero)
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
                        //mOutputReportLen = oCaps.OutputReportByteLength;	// ... and output report lengths
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("[HidUsb]?®∞???hid???????.");
                        return false;
                    }
                    finally
                    {
                        Win32Usb.HidD_FreePreparsedData(ref lpData);	// before we quit the funtion, we must free the internal buffer reserved in GetPreparsedData
                    }
                }
                else	// GetPreparsedData failed? Chuck an exception
                {
                    Debug.LogWarning("[HidUsb]???????hid????.");
                    return false;
                }
            }
            else	// File open failed? Chuck an exception
            {
                mIOHandler = IntPtr.Zero;
                Debug.LogWarning("[HidUsb]???hid?ıÙ.");
                return false;
            }


            mThreadLoopRecive = true;
 
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

    // ???
    public void Close()
    {
#if MOBILE_EDITION
        return;
#endif
        if (mIOHandler != IntPtr.Zero)
        {
            {

                Win32Usb.CancelIo(mIOHandler);
                Win32Usb.CloseHandle(mIOHandler);
                mIOHandler = IntPtr.Zero;

               
                mThreadLoopRecive = false;

                if (mThreadRecive != null)
                    mThreadRecive.Join();

                if (EvtClosed != null)
                    EvtClosed();
            }
        }
 

    }

     
    

    void Thread_Recive()
    {
#if MOBILE_EDITION
        return;
#endif
        try
        {
        InputCmdTrans = new int[LenInputDataPerCtrller, 2];

        for (int i = 0; i != LenInputDataPerCtrller; ++i)
            InputCmdTrans[i, 0] = i;

        InputCmdTrans[0,1] = (int)InputCmd.InCoin;
        InputCmdTrans[1,1] = (int)InputCmd.BtnE;
        InputCmdTrans[2,1] = (int)InputCmd.OutTicket;
        InputCmdTrans[3,1] = (int)InputCmd.OutCoin;
        InputCmdTrans[4,1] = (int)InputCmd.Up;
        InputCmdTrans[5,1] = (int)InputCmd.Down;
        InputCmdTrans[6,1] = (int)InputCmd.Left;
        InputCmdTrans[7,1] = (int)InputCmd.Right;
        InputCmdTrans[8,1] = (int)InputCmd.BtnA;
        InputCmdTrans[9,1] = (int)InputCmd.BtnB;
        InputCmdTrans[10,1] = (int)InputCmd.BtnC;
        InputCmdTrans[11,1] = (int)InputCmd.BtnD;
        //???,??,??,ß≥???,???,??
        BackStageInputCmdTrans = new int[12];
        BackStageInputCmdTrans[0] = (int)InputCmd.Up;
        BackStageInputCmdTrans[1] = (int)InputCmd.BtnB;
        BackStageInputCmdTrans[2] = (int)InputCmd.Left;
        BackStageInputCmdTrans[3] = (int)InputCmd.Right;
        BackStageInputCmdTrans[4] = (int)InputCmd.BtnC;
        BackStageInputCmdTrans[5] = (int)InputCmd.BtnA;
        BackStageInputCmdTrans[6] = (int)InputCmd.Down;
 
        if (mOutputBuff == null)
            mOutputBuff = new BitArray((LenOuputBuf - 1) * 8 );//????bitarray ????????.???.??????????????¶À
 

        BitArray tmpBA = null;//???InputBitArray
        byte[] tmpOutputBuff = new byte[LenOuputBuf];
        byte[] tmpInputBuff = new byte[LenInputBuf];
        byte[] tmpPreInputBuff = new byte[LenInputBuf]; 
        List<InputPackage> tmpRecivePack = new List<InputPackage>();


        if (mOuttingCoinTag == null)
            mOuttingCoinTag = new Dictionary<int, OutBountyElapse>();
        if (mOuttingTicketTag == null)
            mOuttingTicketTag = new Dictionary<int, OutBountyElapse>();
        List<int> tmpListI = new List<int>();
      
        int offsetRead = 8;//????????????8 bit
        uint numWrite = 0;
        uint readCount = 0;
        mRndObj = new System.Random();
        //????????
        mWriteOL.Clear();
        Array.Clear(tmpOutputBuff, 0, tmpOutputBuff.Length);
        
        Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);

        if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)//???
        {
            Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);

            //?????
            if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)
            {
                mInputBuff = new BitArray(tmpInputBuff);

#if RECORE_INITSTATE
                mInputBuffInit = new BitArray(tmpInputBuff);
#endif
                //????????????
                for (int i = OfsInput_ControlBoardState + offsetRead; i != OfsInput_ControlBoardState + offsetRead + NumControlBoard; ++i)
                {
                    tmpRecivePack.Add(new InputPack_CtrlBoardState(InputPackCmd.CtrlBoardConnectState
                        , 0, i - OfsInput_ControlBoardState - offsetRead, mInputBuff[i]));//???????
                }
                
                _FlushInputPackToMainThread(tmpRecivePack);
            }
            Thread.Sleep(100);
        }

#region ???????
        /*
        bool enableOutCoin = false;
        
        while (mThreadLoopRecive && mIOHandler != IntPtr.Zero)
        {
            mWriteOL.Clear();
            Array.Clear(tmpOutputBuff, 0, tmpOutputBuff.Length);


            mOutputBuff[0 * LenOuputDataPerCtrller + OfsOutput_OutCoin] = enableOutCoin;
            enableOutCoin = !enableOutCoin;
            mOutputBuff.CopyTo(tmpOutputBuff, 1);

            Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);

            if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)//???????
            {
                Thread.Sleep(1);
                
                Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);

                //?????
                if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)//???????
                {
                    mInputBuff = new BitArray(tmpInputBuff);

                    //Thread.Sleep(1);

                    //????????????
                    //for (int i = OfsInput_ControlBoardState + offsetRead; i != OfsInput_ControlBoardState + offsetRead + NumControlBoard; ++i)
                    //{
                    //    tmpRecivePack.Add(new InputPack_CtrlBoardState(InputPackCmd.CtrlBoardConnectState
                    //        , 0, i - OfsInput_ControlBoardState - offsetRead, mInputBuff[i]));//???????
                    //}

                    //_FlushInputPackToMainThread(tmpRecivePack);
                }
                

            }
        }
        */
#endregion


        while (mThreadLoopRecive && mIOHandler != IntPtr.Zero)
        {
            if (Evt_HSThread_FrameStart != null)
                Evt_HSThread_FrameStart();

            

            if (Monitor.TryEnter(mSendPack_ThreadLock))
            {
                List<OutputPackage> tempList = mPackToSendST;

                mPackToSendST = mPackToSendMT;
                mPackToSendMT = tempList;
                Monitor.Exit(mSendPack_ThreadLock);
            } 
            
            foreach (OutputPackage pack in mPackToSendST)
            {
                   
                switch (pack.Cmd)
                {
                    case OutputPackCmd.OutCoin:
                        mOutputBuff[pack.CtrllerIdx * LenOuputDataPerCtrller + OfsOutput_OutCoin] = ((OutputPack_OutBounty)pack).Enable;

                        if (!mOuttingCoinTag.ContainsKey(pack.CtrllerIdx))
                            mOuttingCoinTag.Add(pack.CtrllerIdx, new OutBountyElapse(DateTime.Now.Ticks));
                        else
                            Debug.LogWarning("??????????");
                        break;
                    case OutputPackCmd.OutTicket:
                        mOutputBuff[pack.CtrllerIdx * LenOuputDataPerCtrller + OfsOutput_OutTicket] = ((OutputPack_OutBounty)pack).Enable;
                        if (!mOuttingTicketTag.ContainsKey(pack.CtrllerIdx))
                            mOuttingTicketTag.Add(pack.CtrllerIdx, new OutBountyElapse(DateTime.Now.Ticks));
                        else
                            Debug.LogWarning("??????????");
                        break;
                    case OutputPackCmd.FlashLight:
                        { 
                            OutputPack_Light lightPack = (OutputPack_Light)pack; 
                            if (lightPack.LightIdx == 0)
                                mOutputBuff[pack.CtrllerIdx * LenOuputDataPerCtrller + OfsOutput_Light0] = lightPack.IsOn;
                            else
                                mOutputBuff[pack.CtrllerIdx * LenOuputDataPerCtrller + OfsOutput_Light1] = lightPack.IsOn;

                        }
                        break;
                    case OutputPackCmd.RequestMCUInfo:
                        {

                            tmpOutputBuff[1] = tmpOutputBuff[tmpOutputBuff.Length - 1] = 1;

                            System.Random randObj = new System.Random();
                            int challengeNum = randObj.Next(int.MinValue, int.MaxValue); //???????MCU?????????
                            System.Array.Copy(System.BitConverter.GetBytes(challengeNum), 0, tmpOutputBuff, 2, 4);

                            mWriteOL.Clear();
                            Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);
         
                            if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)//???
                            {
                                Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);
                                bool verfySucess = true;
                                if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read,true) == 0)
                                {
                                            
                                    if (tmpInputBuff[1] == tmpInputBuff[tmpInputBuff.Length - 1] && tmpInputBuff[1] == 1)
                                    {
                                        HMACMD5 cryptor = new HMACMD5(System.Text.Encoding.ASCII.GetBytes("yidingyaochang"));
                                        byte[] challengeAnswer = cryptor.ComputeHash(System.BitConverter.GetBytes(challengeNum));
                                        for (int i = 0; i != challengeAnswer.Length; ++i)
                                        {
                                            if (challengeAnswer[i] != tmpInputBuff[7 + i])
                                            { 
                                                verfySucess = false;
                                                break;
                                            }
                                        }
                                                
                                               
                                    }
                                    else//??????
                                    {
                                        verfySucess = false;
                                    }
                                }
                                else//??????
                                {
                                    verfySucess = false;
                                }

                                if (verfySucess)
                                {
                                    tmpRecivePack.Add(new InputPack_MCUInfo(InputPackCmd.MCUInfo, 0, true,
                                        tmpInputBuff[2],//??????
                                        System.BitConverter.ToUInt16(tmpInputBuff, 3),//???∑⁄??
                                        System.BitConverter.ToUInt16(tmpInputBuff, 5)//???∑⁄??
                                        ));
                                }
                                else
                                {
                                    tmpRecivePack.Add(new InputPack_MCUInfo(InputPackCmd.MCUInfo, 0, false, -1, 0, 0));
                                }
                                _FlushInputPackToMainThread(tmpRecivePack);
                            } 
                        }
                        break;
                    case OutputPackCmd.EditMCUInfo:
                        {
                            OutputPack_EditMCUInfo op = (OutputPack_EditMCUInfo)pack;

                            bool result = _UsbThread_ChangeMCUInfo(op.GameIdx, op.MainVersion, op.SubVersion);

                            tmpRecivePack.Add(new InputPack_ResultEditMCUInfo(InputPackCmd.ResultEditMCUInfo, 0, result));
                            _FlushInputPackToMainThread(tmpRecivePack);
                        }
                        break;

                    case OutputPackCmd.ReadWriteRequest://?ßÿ?ß’?????,?????ßÿ??????
                        {
                            OutputPack_ReadWriteRequest p = (OutputPack_ReadWriteRequest)pack;
                            //?????
                            tmpOutputBuff[1] = 0xf2;
                            //??ß’???
                            tmpOutputBuff[2] = (byte)(p.IsWrite ?  1 : 0);
                            //???
                            Array.Copy(System.BitConverter.GetBytes(p.Address), 0, tmpOutputBuff, 3, 4);
                            //???????
                            tmpOutputBuff[7] = p.DataLength;
                            //????
                            if(p.Data != null)
                                Array.Copy(p.Data, 0, tmpOutputBuff, 8, p.DataLength);
                            //????????
                            Byte[] tmpByte = new byte[5];
                            mRndObj.NextBytes(tmpByte);
                            Array.Copy(tmpByte, 0, tmpOutputBuff, 24, 5);
                            //ßµ????
                            Array.Clear(tmpByte,0,5);
                            for (int i = 0; i != 4; ++i)
                            {
                                for (int j = 0; j != 7; ++j)
                                {
                                    tmpByte[i] ^= tmpOutputBuff[j * 4 + i + 1];
                                }
                            }
                            Array.Copy(tmpByte, 0, tmpOutputBuff, 29, 4);
                            //Debug.Log(string.Format("adress:{0:d} len:{1:d}  ", p.Address, p.DataLength));
                            //Debug.Log("w");
                            //???????
                            mWriteOL.Clear();

                            Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);
                                

                            //??????
                            if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)//???
                            {
                                Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);
                                if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)
                                {
                                    //????????????
                                    if(tmpInputBuff[1] != 0xF2)
                                        goto BREAK_THIS_SWITCH;
                                    //ßµ??????????
                                    Byte[] tmpByte4 = new byte[4];
                                    for (int i = 0; i != 4; ++i)
                                    {
                                        for (int j = 0; j != 7; ++j)
                                        {
                                            tmpByte4[i] ^= tmpInputBuff[j * 4 + i + 1];
                                        }
                                            
                                    }
                                    for(int i = 0; i != 4; ++i)
                                    {
                                        if(tmpByte4[i] != tmpInputBuff[29+i])
                                            goto BREAK_THIS_SWITCH;
                                    }

                                    //Array.Copy(tmpInputBuff, 3, tmpByte4, 0, 4);
                                    uint adress = System.BitConverter.ToUInt32(tmpInputBuff,3);
                                    byte dataLen = tmpInputBuff[7];
                                    byte[] dataOut = null;
                                    if(tmpInputBuff[2] == 0)//??????
                                    {
                                        dataOut = new byte[dataLen];
                                        Array.Copy(tmpInputBuff,9,dataOut,0,dataLen);
                                    }
                                    //??????
                                    tmpRecivePack.Add(new InputPack_ResultReadWrite(InputPackCmd.ResultReadWrite
                                        , tmpInputBuff[2] == 0 ? false : true,//??ß’???
                                        adress,//???
                                        dataLen,//??????
                                        tmpInputBuff[8],//??ß’???
                                        dataOut//??
                                        ));
                                    //Debug.Log(string.Format("adress:{0:d}  datalen{1:d}  resultcode:{2:d}  ",adress,dataLen,tmpInputBuff[8]));
                                    _FlushInputPackToMainThread(tmpRecivePack);
                                    
                                }
                            }
                        }
                        BREAK_THIS_SWITCH: 
                        break;

                }

            }

            mPackToSendST.Clear();
            
            //????????
            mOutputBuff.CopyTo(tmpOutputBuff, 1);
            mWriteOL.Clear();
            Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);
            //Thread.Sleep(5);
            if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) != 0)//?????
            {
                
                goto TAG_INPUT_PROCESS;//?????,???????????
            }
            
            //?????(?????????)
            tmpListI.Clear();
            foreach (KeyValuePair<int, OutBountyElapse> kvp in mOuttingCoinTag)
            {
                if (DateTime.Now.Ticks - kvp.Value.Time > Timeout_Outbounty)//???????????ß“?
                    tmpListI.Add(kvp.Key);
            }

            foreach (int ctrlIdx in tmpListI)
            {
                tmpRecivePack.Add(new InputPackage(InputPackCmd.LackCoin, ctrlIdx));//?????
                mOutputBuff[ctrlIdx * LenOuputDataPerCtrller + OfsOutput_OutCoin] = false;//???????
                mOuttingCoinTag.Remove(ctrlIdx);
            }

            //??????
            tmpListI.Clear();
            foreach (KeyValuePair<int, OutBountyElapse> kvp in mOuttingTicketTag)
            {
                if (DateTime.Now.Ticks - kvp.Value.Time > Timeout_Outbounty)//???????????ß“?
                    tmpListI.Add(kvp.Key);
            }

            foreach (int ctrlIdx in tmpListI)
            {
                tmpRecivePack.Add(new InputPackage(InputPackCmd.LackTicket, ctrlIdx));
                mOutputBuff[ctrlIdx * LenOuputDataPerCtrller + OfsOutput_OutTicket] = false;//???????
                mOuttingTicketTag.Remove(ctrlIdx);
            }
 

        TAG_INPUT_PROCESS:
            //????????
            mReadOL.Clear(); 
            Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);
            //????? 
            if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)
            {
                //????ß“Å£
                #region ??????????????ß“Å£,??ß“Å£??????(?????????????,?????????,???ßπ??)

                int idxValid = 0;//???????????
                for(idxValid = 0; idxValid != tmpPreInputBuff.Length;++idxValid)
                {
                    if(tmpPreInputBuff[idxValid] != tmpInputBuff[idxValid])
                    {
                        break;
                    }
                }
                if (idxValid == tmpPreInputBuff.Length)//?????????????
                {
                    //goto TAG_BREAK_INPUT;
                }
                else
                {
                    tmpInputBuff.CopyTo(tmpPreInputBuff, 0);
                }
                #endregion

                tmpBA = new BitArray(tmpInputBuff);
                
                int validDataEnd = LenInputDataPerCtrller * NumCtrller + offsetRead;
                for (int i = offsetRead; i != validDataEnd; ++i)
                {
                    if (tmpBA[i] != mInputBuff[i])
                    {
                        int ctrlIdx = (i - offsetRead) / LenInputDataPerCtrller;
                        InputCmd dataIdx = (InputCmd)InputCmdTrans[((i - offsetRead) % LenInputDataPerCtrller), ctrlIdx % 2];
                        switch (dataIdx)
                        {
                            case InputCmd.Up:
                            case InputCmd.Down:
                            case InputCmd.Left:
                            case InputCmd.Right:
                            case InputCmd.BtnA:
                            case InputCmd.BtnB:
                            case InputCmd.BtnC:
                            case InputCmd.BtnD:
                            case InputCmd.BtnE:
                                {
                                    if (FuncHSThread_AddKeyPress == null)
                                    {
                                        InputPack_Key pack = new InputPack_Key(InputPackCmd.Key, ctrlIdx, dataIdx, tmpBA[i]);
                                        tmpRecivePack.Add(pack);
                                    }
                                    else
                                    {
                                        if (FuncHSThread_AddKeyPress(ctrlIdx, dataIdx , tmpBA[i]))
                                        {
                                            InputPack_Key pack = new InputPack_Key(InputPackCmd.Key, ctrlIdx, dataIdx, tmpBA[i]);
                                            tmpRecivePack.Add(pack);
                                        }
                                    }
                                    
                                    //InputPack_Key pack = new InputPack_Key(InputPackCmd.Key, ctrlIdx, dataIdx, tmpBA[i]);
                                    //tmpRecivePack.Add(pack);
                                }
                                break;
                            case InputCmd.InCoin:
                                {
                                    if (tmpBA[i])
                                    {
                                        InputPackage pack = new InputPackage(InputPackCmd.InsertCoin, ctrlIdx);
                                        tmpRecivePack.Add(pack);
                                    }
                                }
                                break;
                            case InputCmd.OutCoin:
                                {
#if RECORE_INITSTATE
                                    if (tmpBA[i] == mInputBuffInit[i])
#else
                                    if (tmpBA[i])
#endif
                                    {
                                        InputPackage pack = new InputPackage(InputPackCmd.OutCoin, ctrlIdx);
                                        tmpRecivePack.Add(pack);
                                        //???®Æ??? 
                                        mOutputBuff[pack.CtrllerIdx * LenOuputDataPerCtrller + OfsOutput_OutCoin] = false;
                                        mOuttingCoinTag.Remove(pack.CtrllerIdx);
                                    }
                                }
                                break;
                            case InputCmd.OutTicket:
                                {
#if RECORE_INITSTATE
                                    if (tmpBA[i] == mInputBuffInit[i])
#else
                                    if (tmpBA[i])
#endif
                                    {

                                        InputPackage pack = new InputPackage(InputPackCmd.OutTicket, ctrlIdx);
                                        tmpRecivePack.Add(pack);
                                        //???®Æ??
                                        mOutputBuff[pack.CtrllerIdx * LenOuputDataPerCtrller + OfsOutput_OutTicket] = false;
                                        mOuttingTicketTag.Remove(pack.CtrllerIdx);
                                    }
                                }
                                break;
                        }//switch


                    }//if (tmpBA[i] != mInputBuff[i])?ßÿ????????

                }//for (int i = 0; i != validData; ++i)?????ßπ???????,
                //???????
               
                for (int i = OfsInput_ControlBoardState + offsetRead; i != OfsInput_ControlBoardState + offsetRead + NumControlBoard; ++i)
                {
                    if (tmpBA[i] != mInputBuff[i])
                    {
                        tmpRecivePack.Add(new InputPack_CtrlBoardState(InputPackCmd.CtrlBoardConnectState
                        , 0, i - OfsInput_ControlBoardState - offsetRead, tmpBA[i]));//???????
                    }
                    
                }
 
                //Debug.Log("pBA[257]=" + tmpBA[257]);
                //????????ßÿ? //??,???,??,??,ß≥???,???,??
                for (int i = OfsInput_BsKeyStartBit + offsetRead; i != OfsInput_BsKeyEndBit + offsetRead; ++i)
                {
                    if (tmpBA[i] != mInputBuff[i])
                    {
                        InputPack_Key pack = new InputPack_Key(InputPackCmd.Key, 21, (InputCmd)BackStageInputCmdTrans[i - OfsInput_BsKeyStartBit - offsetRead], tmpBA[i]);
                        tmpRecivePack.Add(pack);
                    }
                }

                mInputBuff = tmpBA;
                foreach (InputPackage p in mInputPackageFromPlugin)
                {
                    tmpRecivePack.Add(p);
                }
                mInputPackageFromPlugin.Clear();
                _FlushInputPackToMainThread(tmpRecivePack);
            //TAG_BREAK_INPUT: ;

            }//if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) == 0)//?????????
             
        }//while //???while???   
        }//try
        catch (System.TimeoutException)
        {
            Debug.LogError("time out");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("SerialThreadUpdate Exception:" + ex.Message);
        }
        finally
        {
            Debug.Log("thread exit");
        }
    }
 

    void _FlushInputPackToMainThread(List<InputPackage> packLst)
    {
#if MOBILE_EDITION
        return;
#endif

        if (packLst.Count != 0)
        {
            Monitor.Enter(mRecivePack_ThreadLock);
            foreach (InputPackage p in packLst)
                mPackToReciveST.Add(p);
            Monitor.Exit(mRecivePack_ThreadLock);

            packLst.Clear();
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
#if MOBILE_EDITION
        return;
#endif
        Debug.Log("Handle_Device_Arrival");
        if (FindDevicePath(VID, PID) != "")
        {
            Open();
        }
    }

    void Handle_Device_RemoveComplete()
    {
#if MOBILE_EDITION
        return;
#endif
        Debug.Log("Msg_Device_RemoveComplete");
        if (FindDevicePath(VID, PID) == "")
        {
            Close();
        }
    }

    ////////////////////////////////////////////???????/////////////////////////////////////////////////////////////////////////////

    public void OutCoin(uint num, int ctrlIdx)
    {
#if MOBILE_EDITION
        return;
#endif
        OutCoin(ctrlIdx);
    }

    /// <summary>
    /// ????,,??BUG,???????????????????????
    /// </summary>
    public void OutCoin(int ctrlIdx)
    {
#if MOBILE_EDITION
        return;
#endif

        if (!IsOpen())
            return;
        OutputPack_OutBounty pack = new OutputPack_OutBounty(OutputPackCmd.OutCoin, ctrlIdx,true);

        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(pack);
        Monitor.Exit(mSendPack_ThreadLock); 
    }

    public void OutTicket(uint num, int ctrlIdx)
    {
#if MOBILE_EDITION
        return;
#endif
        OutTicket(ctrlIdx);
    }



    public void OutTicket(int ctrlIdx)
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;
        OutputPack_OutBounty pack = new OutputPack_OutBounty(OutputPackCmd.OutTicket, ctrlIdx, true);

        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(pack);
        Monitor.Exit(mSendPack_ThreadLock); 
    } 

    public void FlashLight(int ctrlIdx,int lightidx,bool isOn)
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;
        OutputPack_Light pack = new OutputPack_Light(OutputPackCmd.FlashLight, ctrlIdx,lightidx,isOn);

        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(pack);
        Monitor.Exit(mSendPack_ThreadLock); 
    }

    static string GetDevicePath(IntPtr hInfoSet, ref Win32Usb.DeviceInterfaceData oInterface)
    {
#if MOBILE_EDITION
        return "";
#endif
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
#if MOBILE_EDITION
        return "";
#endif
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

            Debug.LogWarning(string.Format("[????]??usbhid?ıÙ???? :{0}WinError:{1:X8}", ex.ToString(), Marshal.GetLastWin32Error()));
            //Console.WriteLine(ex.ToString());
        }
        finally
        {
            // Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
            Win32Usb.SetupDiDestroyDeviceInfoList(hInfoSet);
        }
        return "";	// oops, didn't find our device
    }
    
    public void FlashButtomLight(int i)
    {

    }

    /// <summary>
    /// ?????????????
    /// </summary>
    /// <param name="dataLen"></param>
    /// <param name="sendInterval"></param>
    /// <param name="onOff"></param>
    public void RequestDataBack(byte dataLen, byte sendInterval, bool isOn)
    {
    }

    /// <summary>
    /// ????MCU???
    /// </summary>
    public void RequestHardwareInfo()
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;
        OutputPackage pack = new OutputPackage(OutputPackCmd.RequestMCUInfo,0);

        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(pack);
        Monitor.Exit(mSendPack_ThreadLock); 
    }

    /// <summary>
    /// ?????ß’
    /// </summary>
    /// <param name="IsWrite"></param>
    /// <param name="address"></param>
    /// <param name="dataLen"></param>
    /// <param name="data"></param>
    public void RequestReadWrite(bool isWrite, uint address, byte dataLen, byte[] data)
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;

        OutputPack_ReadWriteRequest pack = new OutputPack_ReadWriteRequest(OutputPackCmd.ReadWriteRequest, isWrite, address,dataLen,data);

        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(pack);
        Monitor.Exit(mSendPack_ThreadLock); 
    }

    /// <summary>
    /// ??????(????????)
    /// </summary>
    /// <param name="address"></param>
    /// <param name="dataLen"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Read_Block(uint address, byte dataLen, out byte[] outVal)
    {
#if MOBILE_EDITION
        outVal = null;
        return false;
#endif
        if (!IsOpen())
        {
            outVal = null;
            return false;
        }

        RequestReadWrite(false, address, dataLen, null);//?????????
        float startTime = Time.realtimeSinceStartup;
        bool isReaded = false;//??????????????
        byte[] dataReaded = null;
        
        NemoCtrlIO_EventResultReadWrite delegateResultRead = (bool isW, uint ads, byte dl, byte rc, byte[] d) =>
        {
            //Debug.Log(string.Format("delegateResultRead IsWrite:{0} adress:{1:d} resultCode:{2:d}", IsWrite, ads, resultcode));
            if (!isW && ads == address && rc == 1)
            {
                isReaded = true;
                dataReaded = new byte[dl];
                System.Array.Copy(d, dataReaded, dl);
            }
        };

        EvtResultReadWrite += delegateResultRead;//????
        while (!isReaded)
        {
            if (Time.realtimeSinceStartup - startTime > 5F)//???????
            {
                Debug.Log("[NemoUsbHid_HardScan]?????????");
                break;
            }
            RecivePackages();
        }
        EvtResultReadWrite -= delegateResultRead;//???

        //??mcu?????? 
        if (isReaded)
            outVal = dataReaded;
        else
            outVal = null;

        return isReaded;

    }
    /// <summary>
    /// ?????????????Package
    /// </summary>
    /// <param name="p"></param>
    public void AddInputPackage(InputPackage p)
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;
        mInputPackageFromPlugin.Add(p);
    }
    /// <summary>
    /// ??????????
    /// </summary>
    /// <param name="gameIdx"></param>
    /// <param name="mainVersion"></param>
    /// <param name="subVersion"></param>
    /// <returns></returns>
    bool _UsbThread_ChangeMCUInfo(int gameIdx,int mainVersion,int subVersion)
    {
#if MOBILE_EDITION
        return false;
#endif
        byte[] tmpOutputBuff = new byte[LenOuputBuf];
        byte[] tmpInputBuff = new byte[LenInputBuf];
        //List<InputPackage> tmpRecivePack = new List<InputPackage>();
        byte[] plainDatasFromMCU = new byte[16];//???MCU??????
        uint numWrite = 0;
        uint readCount = 0;
        //???????MCU????
        tmpOutputBuff[1] = tmpOutputBuff[tmpOutputBuff.Length - 1] = 0xF1;
        tmpOutputBuff[2] = 1; 
        mWriteOL.Clear();
        Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);
 
        if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) != 0)//??????
            return false;
        //MCU??????????
        mReadOL.Clear();
        Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);

        //bool verfySucess = true;
        if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) != 0)//??????
            return false;
        if (tmpInputBuff[1] != tmpInputBuff[tmpInputBuff.Length - 1] || tmpInputBuff[1] != 0xF1)//???????
            return false;

        //???MCU????????,?????????????
        Array.Copy(tmpInputBuff,2,plainDatasFromMCU,0,plainDatasFromMCU.Length);
        HMACMD5 cryptor = new HMACMD5(System.Text.Encoding.ASCII.GetBytes("FX20120927YIDINGYAOCHANGAEF51FM2"));
        byte[] challengeAnswer = cryptor.ComputeHash(plainDatasFromMCU);
        Array.Clear(tmpOutputBuff, 0, tmpOutputBuff.Length);
        tmpOutputBuff[1] = tmpOutputBuff[tmpOutputBuff.Length - 1] = 0xF1;
        tmpOutputBuff[2] = 2;
        tmpOutputBuff[3] = (byte)gameIdx;//??????
        Array.Copy(System.BitConverter.GetBytes((ushort)mainVersion),0,tmpOutputBuff,4,2);//????∑⁄??
        Array.Copy(System.BitConverter.GetBytes((ushort)subVersion),0,tmpOutputBuff,6,2);//????∑⁄??
        Array.Copy(challengeAnswer, 0, tmpOutputBuff, 8, challengeAnswer.Length);
        mWriteOL.Clear();
        Win32Usb.WriteFile(mIOHandler, tmpOutputBuff, (uint)tmpOutputBuff.Length, ref numWrite, ref mWriteOL);

        if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) != 0)//ß’???
            return false;

        //????????????
        mReadOL.Clear();
        Win32Usb.ReadFile(mIOHandler, tmpInputBuff, (uint)tmpInputBuff.Length, ref readCount, ref mReadOL);
        if (Win32Usb.WaitForSingleObjectEx(mIOHandler, Timeout_Read, true) != 0)//??????
            return false;
        if (tmpInputBuff[1] != tmpInputBuff[tmpInputBuff.Length - 1]
            || tmpInputBuff[1] != 0xF1
            || tmpInputBuff[2] != 0x55
            || tmpInputBuff[3] != 0xAA)//?????????
            return false;
        return true;
        
    }


    public void EditMCUInfo(int gameIdx, int mainVersion, int subVersion)
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;

        OutputPack_EditMCUInfo pack = new OutputPack_EditMCUInfo(OutputPackCmd.EditMCUInfo, 0,gameIdx,mainVersion,subVersion);

        Monitor.Enter(mSendPack_ThreadLock);
        mPackToSendMT.Add(pack);
        Monitor.Exit(mSendPack_ThreadLock); 

    }
    public void RecivePackages()
    {
#if MOBILE_EDITION
        return;
#endif
        if (!IsOpen())
            return;

        if (Monitor.TryEnter(mRecivePack_ThreadLock))
        {
            List<InputPackage> tempList = mPackToReciveMT;
            mPackToReciveMT = mPackToReciveST;
            mPackToReciveST = tempList;
            Monitor.Exit(mRecivePack_ThreadLock);
        }
        foreach (InputPackage p in mPackToReciveMT)
        {
            switch (p.Cmd)
            {
                case InputPackCmd.Key:
                    
                    if (mEvtKey != null)
                    {
                        InputPack_Key packKey = (InputPack_Key)p;
                        mEvtKey(p.CtrllerIdx, (NemoCtrlIO_Key)packKey.Key_, packKey.DownOrUp);
                    }
                    break;
                case InputPackCmd.OutCoin:
                    if (mEvtOutCoinReflect != null)
                        mEvtOutCoinReflect(p.CtrllerIdx);
                    break;
                case InputPackCmd.OutTicket:
                    if (mEvtOutTicketReflect != null)
                        mEvtOutTicketReflect(p.CtrllerIdx);
                    break;
                case InputPackCmd.LackCoin:
                    if (mEvtLackCoin != null)
                        mEvtLackCoin(p.CtrllerIdx);
                    break;
                case InputPackCmd.LackTicket:
                    if (mEvtLackTicket != null)
                        mEvtLackTicket(p.CtrllerIdx);
                    break;
                case InputPackCmd.InsertCoin:
                    if (mEvtInsertCoin != null)
                    {
                        mEvtInsertCoin(1, p.CtrllerIdx);
                    }
                    break;

                case InputPackCmd.MCUInfo:
                    if (EvtHardwareInfo != null)
                    {
                        InputPack_MCUInfo pMCUInfo = (InputPack_MCUInfo)p;
                        EvtHardwareInfo(pMCUInfo.GameIdx, pMCUInfo.VersionMain, pMCUInfo.VersionSub, pMCUInfo.VerifySucess);
                    }
                    break;
                case InputPackCmd.CtrlBoardConnectState:
                    if (EvtCtrlBoardStateChanged != null)
                    {
                        InputPack_CtrlBoardState pCtrlBoardPack = (InputPack_CtrlBoardState)p;
                        EvtCtrlBoardStateChanged(pCtrlBoardPack.CtrlBoardId, pCtrlBoardPack.ConectOrDisconect);
                    }
                    break;
                case InputPackCmd.ResultEditMCUInfo:
                    if (EvtResultEditMCU != null)
                    {
                        InputPack_ResultEditMCUInfo op = (InputPack_ResultEditMCUInfo)p;
                        EvtResultEditMCU(op.Result);
                    }
                    break;
                case InputPackCmd.ResultReadWrite:
                    if (EvtResultReadWrite != null)
                    {
                        InputPack_ResultReadWrite op = (InputPack_ResultReadWrite)p;
                        //Debug.Log("EvtResultReadWrite.count = " + EvtResultReadWrite.GetInvocationList().Length);
                        EvtResultReadWrite(op.IsWrite, op.Address, op.DataLength, op.ResultCode, op.Data);
                    }
                    break;

            }
        }
        mPackToReciveMT.Clear();
    }
}

