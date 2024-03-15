using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static OpenMoreTools.Enums;
using static OpenMoreTools.Structs;
using static OpenMoreTools.Win32;

namespace OpenMoreTools
{

    public class TagResponseModel
    {
        public string Name { get; set; }
        // ... 其他属性
    }

    internal class Util
    {
        public static string GetAppInstallPath(string appName, string softwarePath,string queryKey)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(softwarePath);
            if (key == null)
            {
                return "";
            }
            object installPathObj = key.GetValue(queryKey);
            if (installPathObj is null)
            {
                key.Close();
                return "";
            }
            key.Close(); // 关闭注册表键

          
            if (appName == "DingTalk")
            {
                return Path.Combine(Path.GetDirectoryName(installPathObj.ToString()),"main","current", appName + ".exe");
            }

            if (installPathObj.ToString().Contains(".exe"))
            {
                return installPathObj.ToString();
            }
            return Path.Combine(installPathObj.ToString(), appName + ".exe");
        }

        public static bool OpenApp(string appRunPath)
        {
            string appName = Path.GetFileName(appRunPath);
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appRunPath,
                    Verb = "open",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"无法启动{appName}.exe，原因：{ex.Message}");
            }
            return true;
        }

        public static List<Process> GetProcesList(string processName)
        {

            List<Process> processLsit = new List<Process>();
            processLsit.Clear();
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                processLsit.Add(process);
            }

            //if (processIds.Count > 0)
            //{
            //    foreach (int item in processIds)
            //    {
            //        Console.WriteLine(item);
            //    }
            //}


            return processLsit;
        }

        public static List<SYSTEM_HANDLE_INFORMATION> GetHandleList(Process process)
        {
            uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
            int CNST_SYSTEM_HANDLE_INFORMATION = 0x10;

            List<SYSTEM_HANDLE_INFORMATION> aHandles = new List<SYSTEM_HANDLE_INFORMATION>();
            int handle_info_size = Marshal.SizeOf(new SYSTEM_HANDLE_INFORMATION()) * 1000;
            IntPtr ptrHandleData = IntPtr.Zero;
            int nLength = 0;
            try
            {
                ptrHandleData = Marshal.AllocHGlobal(handle_info_size);
                while (NtQuerySystemInformation(CNST_SYSTEM_HANDLE_INFORMATION, ptrHandleData, handle_info_size, ref nLength) == STATUS_INFO_LENGTH_MISMATCH)
                {
                    handle_info_size = nLength;
                    Marshal.FreeHGlobal(ptrHandleData);
                    ptrHandleData = Marshal.AllocHGlobal(nLength);
                }

                long handle_count = Marshal.ReadIntPtr(ptrHandleData).ToInt64();
                IntPtr ptrHandleItem = ptrHandleData + Marshal.SizeOf(ptrHandleData);


                
                for (long lIndex = 0; lIndex < handle_count; lIndex++)
                {
                    SYSTEM_HANDLE_INFORMATION oSystemHandleInfo = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ptrHandleItem, typeof(SYSTEM_HANDLE_INFORMATION));
                    ptrHandleItem += Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION));

                    if (oSystemHandleInfo.ProcessID != process.Id) { continue; }
                    aHandles.Add(oSystemHandleInfo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrHandleData);
            }
            return aHandles;
        }

        public static bool DuplicateHandleEx(int pid,IntPtr h,out IntPtr ipHandle, uint dwOptions)
        {
            ProcessAccessFlags flags = ProcessAccessFlags.PROCESS_DUP_HANDLE | ProcessAccessFlags.PROCESS_VM_READ;
            IntPtr hProc = OpenProcess(flags, false, pid);
            bool bl = DuplicateHandle(hProc, h, GetCurrentProcess(), out ipHandle, 0, false, dwOptions);
            CloseHandle(hProc);
            return bl;
        }

        public static IntPtr NtQueryObjectEx(IntPtr ipHandle, int objectInformationClass ) 
        {
            //int ObjectInformationClass = 0;
            //if (typeof(T) == typeof(OBJECT_NAME_INFORMATION)) {
            //    ObjectInformationClass = (int)OBJECT_INFORMATION_CLASS.ObjectNameInformation;
            //}

            //if (typeof(T) == typeof(OBJECT_TYPE_INFORMATION))
            //{
            //    ObjectInformationClass = (int)OBJECT_INFORMATION_CLASS.ObjectTypeInformation;
            //}

            IntPtr hObject = Marshal.AllocHGlobal(128 * 1024);
            int nLength = 0;
            while ((uint)(NtQueryObject(ipHandle, objectInformationClass, hObject, nLength, ref nLength)) == 0xC0000004)
            {
                Marshal.FreeHGlobal(hObject);
                if (nLength == 0)
                {
                    Console.WriteLine("Length returned at zero!");

                }
                hObject = Marshal.AllocHGlobal(nLength);
            }

            return hObject;
        }



        // 查找 app mutex handle
        public static bool FindAppMutexHandle(SYSTEM_HANDLE_INFORMATION systemHandleInformation, int pid,out IntPtr handle,string identifier)
        {

            IntPtr ipHandle;
            // 复制对象句柄 并且 源句柄具有相同的访问权限
            bool bl2 = Util.DuplicateHandleEx(pid, new IntPtr(systemHandleInformation.Handle), out ipHandle, (uint)DuplicateHandleDwOptions.DUPLICATE_SAME_ACCESS);

            // 检索句柄对象Type
            IntPtr hObjectType = NtQueryObjectEx(ipHandle, (int)ObjectInformationClass.ObjectTypeInformation);
            PUBLIC_OBJECT_TYPE_INFORMATION objectType = (PUBLIC_OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(hObjectType, typeof(PUBLIC_OBJECT_TYPE_INFORMATION));
            Marshal.FreeHGlobal(hObjectType);
            if (objectType.TypeName.Buffer != IntPtr.Zero)
            {
                string strObjectType = Marshal.PtrToStringUni(objectType.TypeName.Buffer);
                if (strObjectType.Contains("Mutant"))
                {
                    // 检索句柄对象Name
                    IntPtr hObjectName = Util.NtQueryObjectEx(ipHandle, (int)ObjectInformationClass.ObjectNameInformation);
                    OBJECT_NAME_INFORMATION objObjectName = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(hObjectName, typeof(OBJECT_NAME_INFORMATION));
                    Marshal.FreeHGlobal(hObjectName);

                    if (objObjectName.Name.Buffer != IntPtr.Zero)
                    {
                        string strObjectName = Marshal.PtrToStringUni(objObjectName.Name.Buffer);
                        if (strObjectName.Contains(identifier))
                        {
                            Console.WriteLine(strObjectName);
                            CloseHandle(ipHandle);
                            //CloseHandle(ipHandle);

                            //// 复制对象句柄 并且 关闭源句柄
                            //bl2 = Util.DuplicateHandleEx(pid, new IntPtr(systemHandleInformation.Handle), out ipHandle, (uint)DuplicateHandleDwOptions.DUPLICATE_CLOSE_SOURCE);
                            //if (bl2)
                            //{
                            //    CloseHandle(ipHandle);
                            //    return true;
                            //}
                            handle = new IntPtr(systemHandleInformation.Handle);
                            return true;
                        }
                    }
                }
    
            }
     
            handle = IntPtr.Zero;
            return false;
        }
        // 关闭 app mutex handle
        public static bool CloseAppMutexHandle(int pid ,IntPtr systemHandleInformationHandle) {
            IntPtr ipHandle;
            // 复制对象句柄 并且 关闭源句柄
            bool bl = DuplicateHandleEx(pid, systemHandleInformationHandle, out ipHandle, (uint)DuplicateHandleDwOptions.DUPLICATE_CLOSE_SOURCE);
            CloseHandle(ipHandle);
            return bl;
        }

        public static async Task<string> Get(string uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Mobile Safari/537.36");
                var response = await httpClient.GetAsync(uri);
                //response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }


    }
}
