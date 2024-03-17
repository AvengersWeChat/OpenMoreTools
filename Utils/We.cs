using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static OpenMoreTools.Win32;
using static OpenMoreTools.Structs;

namespace OpenMoreTools
{
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //public struct SYSTEM_HANDLE_INFORMATION
    //{
    //    //public uint NumberOfHandles;
    //    public UInt32 OwnerPID;
    //    public Byte ObjectType;
    //    public Byte HandleFlags;
    //    public UInt16 HandleValue;
    //    public UIntPtr ObjectPointer;
    //    public IntPtr AccessMask;

    //}



    //struct SYSTEM_HANDLE_INFORMATION1
    //{
    //    public uint NumberOfHandles;
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    //    public SYSTEM_HANDLE_TABLE_ENTRY_INFO[] Handles;
    //}


    //struct SYSTEM_HANDLE_TABLE_ENTRY_INFO
    //{
    //    //USHORT UniqueProcessId;
    //    //USHORT CreatorBackTraceIndex;
    //    //UCHAR ObjectTypeIndex;
    //    //UCHAR HandleAttributes;
    //    //USHORT HandleValue;
    //    //PVOID Object;
    //    //ULONG GrantedAccess;
    //}



    internal class We
    {


        public static void Test()
        {
            IntPtr b = GetProcAddress(GetModuleHandleA("ntdll.dll"), "ZwQuerySystemInformation");
            //var diskDosName = Marshal.AllocHGlobal(120);
            //if (QueryDosDevice(fullPath[..2], diskDosName, 120) == 0)
            //if (QueryDosDevice(fullPath.Substring(0, 2), diskDosName, 120) == 0)
            //{
            //    Marshal.FreeHGlobal(diskDosName);
            //    throw new Win32Exception();
            //}
            //var dosPath = Marshal.PtrToStringUni(diskDosName) + fullPath.Substring(0, 2);
            //Marshal.FreeHGlobal(diskDosName);
            //var currentHandle = Process.GetCurrentProcess().Handle;
        }

      
        public static void Multiple()
        {
            string appName = "WeChat";
            try
            {

                List<Process> procesList = Util.GetProcesList(appName);
                List<int> pidList = new List<int>();
                if (procesList.Count > 0)
                {
                    foreach (Process process in procesList)
                    {
                        //Console.WriteLine(process.Id);
                        pidList.Append(process.Id);

                        List<SYSTEM_HANDLE_INFORMATION> handleList = Util.GetHandleList(process);
                        if (handleList.Count > 0)
                        {
                            foreach (SYSTEM_HANDLE_INFORMATION h in handleList)
                            {
                                //bool bl2 = Util.FindAndCloseAppMutexHandle(h, process.Id);
                                //if (bl2)
                                //{
                                //    Console.WriteLine(bl2);

                                //}

                            }
                        }

                        handleList.Clear();
                    }
                    procesList.Clear();
                };


                



            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            
            return;


            //if (procesList.Count > 0)
            //{
            //    foreach (Process process in procesList)
            //    {

            //        try
            //        {
            //            ProcessModuleCollection modules = process.Modules;
            //            // 遍历模块并打印
            //            foreach (ProcessModule module in modules)
            //            {
            //                //Console.WriteLine($"{module.ModuleName}");
            //            }

            //            long processHandle = process.Handle.ToInt64();

            //            //Console.WriteLine(process.HandleCount);

            //            Console.WriteLine(process.Handle);
            //            Console.WriteLine(processHandle);


            //            //Process p = Process.GetProcessById(pid);
            //            //p.Start();
            //        }

            //        catch (InvalidOperationException ex)
            //        {
            //            Console.WriteLine("无法获取模块信息： " + ex.Message);
            //        }
            //        finally
            //        {
            //            // 释放资源
            //            process.Dispose();
            //        }

            //    }
            //}





        }

    }


}
