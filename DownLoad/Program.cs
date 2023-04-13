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
            string pathq = default;
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine("程序加载中...");
            Thread.Sleep(2000);
            var zipPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CopyTest.zip";
            string strContent = "{\"TestName\":\"" + "TestItem" + "\",\"DownloadName\":\"" + "CopyTest" + "\"}";
            string Error;
            Console.WriteLine("检测文件路径是否存在...");
            if (!Directory.Exists(Path.GetDirectoryName(zipPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
            Console.WriteLine("下载中...");
           bool res= HttpPost("http://10.55.22.160:20005/api/PostDownloadZIP", strContent, "POST", zipPath, out Error);
            if (res)
            {
                Console.WriteLine("下载成功...");
                DirectoryInfo path = new DirectoryInfo(zipPath);
                if (path.Parent == null) return;
                 pathq = path.Parent.FullName;//上 1层目录
           
                // 解压文件
                ZipFile.ExtractToDirectory(zipPath, pathq);
                Console.WriteLine("解压完成...");
                DirectoryInfo paths = new DirectoryInfo(exePath);
                if (paths.Parent == null) return;
                var pathh = path.Parent.Parent.FullName;//上 1层目录
                CopyFile(pathq+@"\CopyTest", pathh);
                Console.WriteLine("文件已复制...");
                 Thread.Sleep(2000);
            }
            else
            {
                  Console.WriteLine("更新失败...");
            }

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
                  Console.WriteLine("已删除:"+zipPath);
            }
            if (IsExistDirectory(pathq + @"\CopyTest"))
            {
                Directory.Delete((pathq + @"\CopyTest"), true);
                Thread.Sleep(1000);
                Console.WriteLine("已删除:"+pathq + @"\CopyTest");
            }
            Thread.Sleep(2000);
        }

        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <returns>1 || -1</returns>
        public static int CopyFile(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                //得到原文件根目录下的所有文件
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest, true);//复制文件

                }
                //得到原文件根目录下的所有文件夹
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(destFolder, name);
                    CopyFile(folder, dest);//构建目标路径,递归复制文件
                }
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        public static bool HttpPost(string url, string Writedata, string Method, string Path, out string Error)
        {
            Error = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(Writedata);
                //参数类型，这里是json类型
                //还有别的类型如"application/x-www-form-urlencoded"，不过我没用过(逃
                httpWebRequest.ContentType = "application/json";
                //参数数据长度
                httpWebRequest.ContentLength = bs.Length;
                //设置请求类型
                httpWebRequest.Method = Method;
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
                    using (FileStream fs = File.Create(Path))
                    {
                        //建立字节组，并设置它的大小是多少字节
                        byte[] bytes = new byte[102400];
                        int n = 1;
                        while (n > 0)
                        {
                            //一次从流中读多少字节，并把值赋给Ｎ，当读完后，Ｎ为０,并退出循环
                            n = stream.Read(bytes, 0, 10240);
                            fs.Write(bytes, 0, n); //将指定字节的流信息写入文件流中
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Error = ex.ToString();
                File.AppendAllText($@".\Log\错误信息{DateTime.Now:MM_dd}.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return false;
            }
        }
    }
}
