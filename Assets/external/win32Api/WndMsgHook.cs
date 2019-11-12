using UnityEngine;

using System;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// 触发函数 
///     1.Msg_Device_Arrival 有设备加入
///     2.Msg_Device_RemoveComplete 有设备移除
/// 
/// </summary>
public class WndMsgHook : MonoBehaviour
{

    [DllImport("user32")]
    protected static extern IntPtr SetWindowsHookEx(
        HookType code, HookProc func, IntPtr hInstance, int threadID);

    [DllImport("user32")]
    protected static extern int UnhookWindowsHookEx(
        IntPtr hhook);

    [DllImport("user32")]
    protected static extern int CallNextHookEx(
        IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

    [DllImport("Kernel32")]
    protected static extern uint GetLastError();

    /// <summary>Windows message sent when a device is inserted or removed</summary>
    public const int WM_DEVICECHANGE = 0x0219;
    /// <summary>WParam for above : A device was inserted</summary>
    public const int DEVICE_ARRIVAL = 0x8000;
    /// <summary>WParam for above : A device was removed</summary>
    public const int DEVICE_REMOVECOMPLETE = 0x8004;


    public delegate void GeneralEvent();
    public static GeneralEvent Evt_DeviceArrived;
    public static GeneralEvent Evt_DeviceRemoved;
    static int mMsgArrivedNum;
    static int mMsgRemoveNum;
    // Hook Types
    protected enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }
     // Summary:
    //     Implements a Windows message.
    struct CWPSTRUCT
    {

        public IntPtr LParam;
        public IntPtr WParam;
        public int Msg;
        public IntPtr HWnd;
        
    }
    protected IntPtr m_hhook = IntPtr.Zero;
    protected HookType m_hookType = HookType.WH_CALLWNDPROC;

    protected delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

    protected bool Install(HookProc cbFunc)
    {
        if (m_hhook == IntPtr.Zero)
            m_hhook = SetWindowsHookEx(
                m_hookType,
                cbFunc,
                IntPtr.Zero,
                (int)AppDomain.GetCurrentThreadId());

        if (m_hhook == IntPtr.Zero)
            return false;

        return true;
    }

    protected void Uninstall()
    {
        if (m_hhook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(m_hhook);
            m_hhook = IntPtr.Zero;
        }
    }

    protected int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code < 0)
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);//

        //Debug.Log(
        //    "hook code =" + code.ToString() +
        //    " lparam=" + lParam.ToString() +
        //    " wparam=" + wParam.ToString());

        CWPSTRUCT d = (CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPSTRUCT));
        if (d.Msg == WM_DEVICECHANGE)
        {
            switch (d.WParam.ToInt32())	// Check the W parameter to see if a device was inserted or removed
            {
                case DEVICE_ARRIVAL:	// inserted
 
                    //SendMessage("Msg_Device_Arrival", SendMessageOptions.DontRequireReceiver);
                    if (Evt_DeviceArrived != null)
                        Evt_DeviceArrived();
                    //++mMsgArrivedNum;
                    break;
                case DEVICE_REMOVECOMPLETE:	// removed
                    //Debug.Log("DEVICE_REMOVECOMPLETE"); 
                    if (Evt_DeviceRemoved != null)
                        Evt_DeviceRemoved();
                    break;
            }
        }
        //// Yield to the next hook in the chain
        return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
 
    }
    //static int testI = 0;
    void Update()
    {
        if (mMsgArrivedNum != 0)
        {
            if (Evt_DeviceArrived != null)
                Evt_DeviceArrived();
            --mMsgArrivedNum;
        }
    }
    void outputSth()
    { 
//         if (Evt_DeviceArrived != null)
//             Evt_DeviceArrived();
    }
    // Use this for initialization
    void Start()
    {
        //Debug.Log("install hook");
        Install(CoreHookProc);
    }

    void OnDisable()
    {
        //Debug.Log("Uninstall hook");
        Uninstall();
    }

}