using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
 
public class win32Api : MonoBehaviour {

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMiliseconds;
    }

    /// <summary>
    /// 有时区转换
    /// </summary>
    /// <param name="sysTime"></param>
    /// <returns></returns>
    [DllImport("Kernel32.dll")]
    public static extern bool SetSystemTime(ref SystemTime sysTime);

    /// <summary>
    /// 即输入即所得
    /// </summary>
    /// <param name="sysTime"></param>
    /// <returns></returns>
    [DllImport("Kernel32.dll")]
    public static extern bool SetLocalTime(ref SystemTime sysTime);
    [DllImport("Kernel32.dll")]
    public static extern void GetSystemTime(ref SystemTime sysTime);
    [DllImport("Kernel32.dll")]
    public static extern void GetLocalTime(ref SystemTime sysTime);

    public static bool SetLocalTimeByDateTime(System.DateTime dt)
    {
        bool flag = false;
        SystemTime timeData = new SystemTime();
        timeData.wYear = (ushort)dt.Year;
        timeData.wMonth = (ushort)dt.Month;
        timeData.wDay = (ushort)dt.Day;
        timeData.wHour = (ushort)dt.Hour;
        timeData.wMinute = (ushort)dt.Minute;
        timeData.wSecond = (ushort)dt.Second;

        flag = SetLocalTime(ref timeData);
        return flag;
    }


    [DllImport("user32.dll")]
    static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
    #region win32 api
    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    private struct TokPriv1Luid
    {

        public int Count;

        public long Luid;

        public int Attr;

    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool ExitWindowsEx(int flg, int rea);

    #endregion

    private const int SE_PRIVILEGE_ENABLED = 0x00000002;

    private const int TOKEN_QUERY = 0x00000008;

    private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

    private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

    #region Exit Windows Flags
    private const int EWX_LOGOFF = 0x00000000;

    private const int EWX_SHUTDOWN = 0x00000001;

    private const int EWX_REBOOT = 0x00000002;

    private const int EWX_FORCE = 0x00000004;

    private const int EWX_POWEROFF = 0x00000008;

    private const int EWX_FORCEIFHUNG = 0x00000010;

    #endregion
    /// <summary>
    /// 重启系统
    /// </summary>
    public static void RebootSystem()
    {
        //give current process SeShutdownPrivilege
        TokPriv1Luid tp;

        IntPtr hproc = GetCurrentProcess();

        IntPtr htok = IntPtr.Zero;

        if (!OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok))
        {
            throw new Exception("Open Process Token fail");
        }

        tp.Count = 1;

        tp.Luid = 0;

        tp.Attr = SE_PRIVILEGE_ENABLED;

        if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid))
        {
            throw new Exception("Lookup Privilege Value fail");
        }

        if (!AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
        {
            throw new Exception("Adjust Token Privileges fail");
        }


        ExitWindowsEx(2 | 4, 0);//reboot | force
    }
}
