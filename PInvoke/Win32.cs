using System;
using System.Runtime.InteropServices;
using static OpenMoreTools.Enums;


namespace OpenMoreTools
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TOKEN_PRIVILEGE
    {
        public uint PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privilege;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public int LowPart;
        public int HighPart;
    }



    public class Win32
    {
        //private const string Ntdll = "ntdll.dll";

        [DllImport("ntdll.dll")]
        public static extern uint NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryObject(IntPtr ObjectHandle, int ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);
        [DllImport("advapi32.dll")]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);
        [DllImport("advapi32.dll")]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGE NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("advapi32.dll")]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandleA(string lpModuleName);//检索指定模块的模块句柄
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(int hModule, string lpProcName); //从指定的动态链接库 (DLL) 检索导出函数或变量的地址

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, bool bInheritHandle, uint dwOptions);

        [DllImport("ntdll.dll", EntryPoint = "ZwQuerySystemInformation")]
        public static extern uint ZwQuerySystemInformation(SystemInformationClass SystemInformationClass, IntPtr SystemInformation, uint SystemInformationLength, ref uint ReturnLength);

        [DllImport("ntdll.dll")]
        public static extern uint NtQueryInformationProcess(IntPtr ProcessHandle, uint ProcessInformationClass, IntPtr ProcessInformation, uint ProcessInformationLength, out uint ReturnLength);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

       
        //public delegate uint ZwQuerySystemInformationDelegate(
        //    SystemInformationClass SystemInformationClass,
        //    IntPtr SystemInformation,
        //    uint SystemInformationLength,
        //    out uint ReturnLength
        //);
    }
}
