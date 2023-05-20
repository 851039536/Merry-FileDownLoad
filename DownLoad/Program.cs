using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DownLoad
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string parentPath = default;
            const string testName = "TestItem";
            const string downloadName = "CopyTest";


            // 获取当前程序集的执行路径(根目录)D:\sw\Console\FileDownLoad\DownLoad\bin\Debug
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Console.WriteLine("程序加载中...");
            Thread.Sleep(2000);

            // 文件下载路径及zip文件名称
            var zipName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + downloadName +
                          ".zip";

            // 定义一个字符串变量，用于存储 JSON 格式的数据

            var strContent = "{\"TestName\":\"" + testName + "\",\"DownloadName\":\"" + downloadName + "\"}";

            Console.WriteLine("检测文件路径是否存在...");

            // 检查目录是否存在，如果不存在则创建目录
            Console.WriteLine("检测文件路径是否存在...");
            EnsureDirectoryExists(Path.GetDirectoryName(zipName) ?? string.Empty);

            Console.WriteLine("下载中...");

            // 发送HTTP POST请求，下载ZIP文件
            // var data = HttpPost("http://10.55.22.160:20005/api/PostDownloadZIP",
            var data = HttpPost("http://10.55.2.25:20005/api/PostDownloadZIP",
                strContent, "POST", zipName, out _);

            if (data)
            {
                Console.WriteLine("下载成功...");

                var zipPath = new DirectoryInfo(zipName);
                if (zipPath.Parent == null) return;
                //上 1层目录
                parentPath = zipPath.Parent.FullName;

                // 解压文件
                ExtractZipFile(zipName, parentPath);
                Console.WriteLine("解压完成...");

                if (exePath != null)
                {
                    var path = new DirectoryInfo(exePath);
                    if (path.Parent == null) return;
                }

                if (zipPath.Parent.Parent != null)
                {
                    //上 1层目录
                    var path = zipPath.Parent.Parent.FullName;
                    // 复制文件
                    CopyFile(parentPath + @"\" + downloadName, path);
                }

                Console.WriteLine("文件已复制...");
                Thread.Sleep(2000);
            }
            else
            {
                Console.WriteLine("更新失败...");
            }

            if (File.Exists(zipName))
            {
                File.Delete(zipName);
                Console.WriteLine("已删除:" + zipName);
            }

            //如果存在则删除目录文件
            if (IsExistDirectory(parentPath + @"\" + downloadName))
            {
                Directory.Delete((parentPath + @"\" + downloadName), true);
                Thread.Sleep(1000);
                Console.WriteLine("已删除:" + parentPath + @"\" + downloadName);
            }

            Thread.Sleep(2000);
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="zipFilePath">要解压的zip</param>
        /// <param name="extractPath">解压到指定路径</param>
        private static void ExtractZipFile(string zipFilePath, string extractPath)
        {
           ZipFile.ExtractToDirectory(zipFilePath, extractPath,Encoding.UTF8);
        }

        /// <summary>
        /// 检查目录是否存在，如果不存在则创建目录
        /// </summary>
        /// <param name="directoryPath"></param>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 判断目录是否存在
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns>bool</returns>
        private static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <returns>1 || -1</returns>
        private static int CopyFile(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }

                //得到原文件根目录下的所有文件
                var files = Directory.GetFiles(sourceFolder);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    var dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest, true); //复制文件
                }

                //得到原文件根目录下的所有文件夹
                var folders = Directory.GetDirectories(sourceFolder);
                foreach (var folder in folders)
                {
                    var name = Path.GetFileName(folder);
                    var dest = Path.Combine(destFolder, name);
                    CopyFile(folder, dest); //构建目标路径,递归复制文件
                }

                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        private static bool HttpPost(string url, string writeData, string method, string path, out string error)
        {
            error = "";
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                //字符串转换为字节码
                var bs = Encoding.UTF8.GetBytes(writeData);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                //参数数据长度
                httpWebRequest.ContentLength = bs.Length;
                //设置请求类型
                httpWebRequest.Method = method;
                //设置超时时间
                httpWebRequest.Timeout = 20000;
                //将参数写入请求地址中
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                //发送请求
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //流对象使用完后自动关闭
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    //文件流，流信息读到文件流中，读完关闭
                    using (FileStream fs = File.Create(path))
                    {
                        //建立字节组，并设置它的大小是多少字节
                        byte[] bytes = new byte[102400];
                        int n = 1;
                        while (n > 0)
                        {
                            //一次从流中读多少字节，并把值赋给Ｎ，当读完后，Ｎ为０,并退出循环
                            if (stream != null) n = stream.Read(bytes, 0, 10240);
                            fs.Write(bytes, 0, n); //将指定字节的流信息写入文件流中
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n",
                    Encoding.UTF8);
                return false;
            }
        }
    }
}