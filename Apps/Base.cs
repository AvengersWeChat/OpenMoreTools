using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenMoreTools.Structs;

namespace OpenMoreTools.Apps
{
    internal abstract class Base
    {
        protected AppModel config;
        public AppModel Config { get { return config; } }

        public string GetInstallPath()
        {
            config.InstallPath = Util.GetAppInstallPath(config.Name, config.SoftwarePath, config.HKey);
            //return string.IsNullOrEmpty(config.InstallPath);
        
            return config.InstallPath;
        }


        public string GetVersion()
        {
            FileVersionInfo exeFileVersionInfo = FileVersionInfo.GetVersionInfo(config.InstallPath);

            // 输出exe文件的版本信息
            Console.WriteLine($"产品名称: {exeFileVersionInfo.ProductName}");
            Console.WriteLine($"公司名称: {exeFileVersionInfo.CompanyName}");
            Console.WriteLine($"文件版本: {exeFileVersionInfo.FileVersion}");
            Console.WriteLine($"产品版本: {exeFileVersionInfo.ProductVersion}");
            return exeFileVersionInfo.FileVersion;
        }


        public bool Multiple()
        {
            try
            {
                List<Process> procesList = Util.GetProcesList(config.Name);
                List<int> pidList = new List<int>();
                List<(int, IntPtr)> closeHandleList = new List<(int, IntPtr)>();
                bool bl;
                int count = 0;
                if (procesList.Count > 0)
                {
                    foreach (Process process in procesList)
                    {
                        //Console.WriteLine(process.Id);
                        pidList.Append(process.Id);

                        List<SYSTEM_HANDLE_INFORMATION> handleList = Util.GetHandleList(process);
                        if (handleList.Count > 0)
                        {
                            IntPtr handle;
                            foreach (SYSTEM_HANDLE_INFORMATION hadnle in handleList)
                            {
                                bl = Util.FindAppMutexHandle(hadnle, process.Id, out handle, config.Identifier);
                                if (bl) { closeHandleList.Add((process.Id, handle)); }
                            }
                        }
                        handleList.Clear();
                    }
                    procesList.Clear();


                    int num = closeHandleList.Count;
                    if (num > 0)
                    {
                        foreach ((int pid, IntPtr handle) in closeHandleList)
                        {
                            if (Util.CloseAppMutexHandle(pid, handle))
                            {
                                count += 1;
                            };

                        }
                        return count == num;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

    }
}
