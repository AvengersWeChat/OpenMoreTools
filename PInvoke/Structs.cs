using System;
using System.Runtime.InteropServices;

namespace OpenMoreTools
{


    public class Structs
    {

 

        internal struct AppInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public AppInfo(string name, string path)
            {
                Name = name;
                Path = path;
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_HANDLE_TABLE_ENTRY_INFO1
        {
            public ushort UniqueProcessId;
            public ushort CreatorBackTraceIndex;
            public byte ObjectTypeIndex;
            public byte HandleAttributes;
            public ushort Handle;
            public IntPtr Object;
            public IntPtr GrantedAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_HANDLE_INFORMATION1
        {
            public uint NumberOfHandles;
            //public SYSTEM_HANDLE_TABLE_ENTRY_INFO1[] Handles;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct OBJECT_TYPE_INFORMATION
        {
            public UNICODE_STRING Name;
            public ulong TotalNumberOfObjects;
            public ulong TotalNumberOfHandles;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct OBJECT_NAME_INFORMATION
        { 
            public UNICODE_STRING Name;
        }
        [StructLayout(LayoutKind.Sequential)]  //指定结构体的成员按声明顺序进行线性布局，即每个字段紧随前一个字段之后存储在内存中。
        internal struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_HANDLE_INFORMATION
        {
            public ushort ProcessID;
            public ushort CreatorBackTrackIndex;
            public byte ObjectType;
            public byte HandleAttribute;
            public ushort Handle;
            public IntPtr Object_Pointer;
            public IntPtr AccessMask;
        }

        // NtQueryObject objectInformationClass
        [StructLayout(LayoutKind.Sequential)]
        internal struct PUBLIC_OBJECT_TYPE_INFORMATION
        {
            public UNICODE_STRING TypeName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
            public uint[] Reserved;
        }





    }
}
