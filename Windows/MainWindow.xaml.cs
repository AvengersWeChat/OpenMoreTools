using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OpenMoreTools.Apps;





namespace OpenMoreTools.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Base program = null;

        private string url = "https://github.com/AvengersWeChat/OpenMoreTools";

        private Version localVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        public MainWindow()
        {
            InitializeComponent();

            Title = "Open More Tools 多开工具 v" + localVer.ToString();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RadioWechat.Tag = new AppModel { Name = "WeChat", SoftwarePath = @"Software\\Tencent\\WeChat", HKey = "InstallPath", Short = "微信", Identifier = "_WeChat_App_Instance_Identity_Mutex_Name" };
            RadioWeWork.Tag = new AppModel { Name = "WXWork", SoftwarePath = @"Software\\Tencent\\WXWork", HKey = "Executable", Short = "企微", Identifier = "Tencent.WeWork.ExclusiveObject" };
            RadioDingTalk.Tag = new AppModel { Name = "DingTalk", SoftwarePath = @"Software\\DingTalk\\Scheme", HKey = "钉钉", Short = "钉钉", Identifier = "DingTalk" };
            RadioWechat.IsChecked = true;

            try
            {
                await Update();
            }
            catch (Exception)
            {

                DataContext = new { UpdateData = $"[ 当前已经是最新版本 ]", UpdateColour = "CadetBlue" };
            }

        }

        private async Task Update()
        {

            string version = await Util.Get("https://api.github.com/repos/AvengersWeChat/OpenMoreTools/releases/latest");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            TagResponseModel res = serializer.Deserialize<TagResponseModel>(version);

            Console.WriteLine(res.Name);
            Version serverVer = new Version(res.Name);
            int result = serverVer.CompareTo(localVer);
            if (result > 0)
            {
                DataContext = new { UpdateData = $"[ 点击下载最新版本 {res.Name} ]", UpdateColour = "Red" };
            }
            else
            {
                DataContext = new { UpdateData = $"[ 当前已经是最新版本 ]", UpdateColour = "CadetBlue" };
            }

        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.IsChecked == true)
            {
                string name = radioButton.Name;
                // 这里可以根据选中的RadioButton做进一步的处理
                switch (name)
                {
                    case "RadioWechat":
                        program = new WeChat((AppModel)RadioWechat.Tag);
                        break;
                    case "RadioWeWork":
                        program = new WeWork((AppModel)RadioWeWork.Tag);
                        break;
                    case "RadioDingTalk":
                        program = new DingTalk((AppModel)RadioDingTalk.Tag);
                        break;
                    default:
                        program = null;
                        break;
                }

                if (program != null)
                {
                    try
                    {
                        TextBoxInstallPath.Text = program.GetInstallPath();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                    //TextBlockVersion.Text = modifier.GetVersion();
                    BtnOpenApp.Content = "打开" + program.Config.Short;

                    Console.WriteLine($"路径：{TextBoxInstallPath.Text}");
                    Console.WriteLine($"版本：{TextBlockVersion.Text}");
                }


            }
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e)
        {
            
            bool bl = Util.ElevatePrivileges();
            if (!bl)
            {
                Console.WriteLine("进程提权失败");
            }
            program.Multiple();
            Util.OpenApp(program.Config.InstallPath);
        }

        private void Button_Path_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置文件过滤器，只显示.exe文件
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";

            if (openFileDialog.ShowDialog() == true)
            {
                // 用户选择了文件并且点击了“打开”按钮
                string selectedFilePath = openFileDialog.FileName;
                // 确保所选文件确实为.exe文件
                if (System.IO.Path.GetExtension(selectedFilePath).ToLower() == ".exe")
                {
                    TextBoxInstallPath.Text = selectedFilePath;
                }
                else
                {
                    // 如果用户没有选择.exe文件，则提示错误
                    MessageBox.Show("请选择.exe文件");
                }
            }
        }

        private void TextBoxInstallPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxInstallPath.Text == "")
            {
                return;
            }
            string filePath = TextBoxInstallPath.Text;
            string fileName = System.IO.Path.GetFileName(filePath);

            string name = program.Config.Name;

            bool bl = fileName.StartsWith(name);
            if (bl)
            {
                program.Config.InstallPath = TextBoxInstallPath.Text;
                TextBlockVersion.Text = program.GetVersion();
            }
            else
            {
                TextBoxInstallPath.Text = "";
                TextBlockVersion.Text = "";
            }

        }

        private void Show_Reward_Click(object sender, RoutedEventArgs e)
        {
            string imageName = "reward.png";
            var imageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/Png/" + imageName, UriKind.Absolute));

            // 创建一个模态对话框
            RewardWindow childWindow = new RewardWindow(imageSource);

            // 获取父窗口的大小和位置
            Rect parentRect = this.RestoreBounds; // 或者使用 this.Bounds 如果窗口已最大化或有边框样式影响的话

            // 计算子窗口应该显示的中心点
            double left = parentRect.Left + (parentRect.Width / 2) - (childWindow.Width / 2);
            double top = parentRect.Top + (parentRect.Height / 2) - (childWindow.Height / 2);

            // 设置子窗口的位置和所有者
            childWindow.Owner = this;
            childWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            childWindow.Left = Math.Max(left, 0); // 确保窗口在屏幕上可见
            childWindow.Top = Math.Max(top, 0);

            // 显示子窗口
            childWindow.ShowDialog();
        }

        private void Show_About_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个模态对话框
            AboutWindow childWindow = new AboutWindow();

            // 获取父窗口的大小和位置
            Rect parentRect = this.RestoreBounds; // 或者使用 this.Bounds 如果窗口已最大化或有边框样式影响的话

            // 计算子窗口应该显示的中心点
            double left = parentRect.Left + (parentRect.Width / 2) - (childWindow.Width / 2);
            double top = parentRect.Top + (parentRect.Height / 2) - (childWindow.Height / 2);

            // 设置子窗口的位置和所有者
            childWindow.Owner = this;
            childWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            childWindow.Left = Math.Max(left, 0); // 确保窗口在屏幕上可见
            childWindow.Top = Math.Max(top, 0);

            // 显示子窗口
            childWindow.ShowDialog();
        }

        private void Show_Home_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(url));
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(url));
        }

        private void Hyperlink_Update_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/AvengersWeChat/OpenMoreTools/releases"));
        }


        private object GetCheckedRadio()
        {
            if ((bool)RadioWechat.IsChecked)
            {
                return RadioWechat.Tag;
            }
            if ((bool)RadioWeWork.IsChecked)
            {
                return RadioWeWork.Tag;
            }
            return null;
        }

    }
}







