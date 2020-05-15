using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Util.IO.Log;

namespace Util.IO
{
    /// <summary>
    /// 压缩帮助类
    /// <para>提供了一些方法将文件压缩到zip压缩包内</para>
    /// </summary>
    public class CompressUtil
    {
        /// <summary>
        /// 正在压缩事件
        /// </summary>
        public static event CompressDelegateCode OnCompress;

        /// <summary>
        /// 正在解压事件
        /// </summary>
        public static event CompressDelegateCode OnDeCompress;

        /// <summary>
        /// 当前已添加到要压缩的文件夹或文件的列表
        /// </summary>
        public static List<FileSystemInfo> ZipFileList { get; } = new List<FileSystemInfo>();

        /// <summary>
        /// 将一个文件或一个文件夹及其子文件夹和子文件添加到压缩包内
        /// </summary>
        /// <param name="zipStream">要添加文件的压缩文件输出流</param>
        /// <param name="fileSystem">要添加的文件或文件夹</param>
        /// <param name="rootPath">该文件或文件夹在压缩文件内的父级路径</param>
        public static void AddEntry(ZipOutputStream zipStream,FileSystemInfo fileSystem,string rootPath = "")
        {
            ZipEntry zipEntry = null;
            Console.WriteLine(fileSystem.FullName);
            //如果是文件夹
            if(fileSystem is DirectoryInfo)
            {
                //创建压缩文件内的路劲
                zipEntry = new ZipEntry(Path.Combine(rootPath, fileSystem.Name + "\\"));
                //调用事件通知外部处理事件信息
                OnCompress?.Invoke(fileSystem.Name, 1, 1f, true);
                //将压缩文件内的文件夹路径添加到压缩文件输出流
                zipStream.PutNextEntry(zipEntry);
                //遍历文件夹下的子文件夹，子文件递归添加
                foreach (FileSystemInfo nextFileSystem in (fileSystem as DirectoryInfo).GetFileSystemInfos())
                {
                    AddEntry(zipStream, nextFileSystem, zipEntry.Name);
                }
            }
            //如果是文件
            else
            {
                //创建压缩文件内的路劲
                zipEntry = new ZipEntry(Path.Combine(rootPath, fileSystem.Name));
                //将压缩文件内的文件夹路径添加到压缩文件输出流
                zipStream.PutNextEntry(zipEntry);
                //读取文件数据
                using (FileStream readFile = new FileStream(fileSystem.FullName,FileMode.Open,FileAccess.Read,FileShare.Read))
                {
                    ////将读文件流的索引定位到文件的开头
                    //readFile.Seek(0, SeekOrigin.Begin);
                    //定义一个1k的数据包
                    byte[] readDataBytes = new byte[1024];
                    //定义一个变量存放已读取的数据的总大小
                    uint totalReadNum = 0;
                    //只要读文件流的索引小于文件最大长度就不停的循环读取文件
                    while (readFile.Position < readFile.Length)
                    {
                        try
                        {
                            //读取文件并得到当前实际读取到的数据的大小
                            int readNum = readFile.Read(readDataBytes, 0, readDataBytes.Length);
                            //更新已读取的数据的总大小
                            totalReadNum += (uint)readNum;
                            //调用事件通知外部处理事件信息
                            OnCompress?.Invoke(fileSystem.Name, totalReadNum, readFile.Length, false);
                            //将文件数据写入到压缩文件输出流内
                            zipStream.WriteAsync(readDataBytes, 0, readNum).Wait();
                        }catch(Exception ex)
                        {
                            //有可能会发生文件占用，文件不存在等异常
                            LogUtil.WriteException(ex.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加文件到要压缩的文件列表
        /// </summary>
        /// <param name="fileSystem">要添加至压缩列表的文件或文件夹</param>
        public static void AddFile(FileSystemInfo fileSystem)
        {
            //如果没有同名文件则添加文件进去
            if(!ZipFileList.Exists(p=>p.Name == fileSystem.Name))
            {
                ZipFileList.Add(fileSystem);
            }
        }

        /// <summary>
        /// 将一个文件或文件夹移除出压缩列表
        /// </summary>
        /// <param name="fileSystem">要移除出压缩列表的文件或文件夹</param>
        public static void RemoveFile(FileSystemInfo fileSystem)
        {
            FileSystemInfo to = ZipFileList.Find(p => p.Name == fileSystem.Name);
            //如果能找到这个文件或文件夹
            if (to != null)
            {
                ZipFileList.Remove(to);
            }
        }

        /// <summary>
        /// 压缩已添加的文件并生成.zip文件
        /// </summary>
        /// <param name="toFile">输出的zip文件路径</param>
        public static void Compress(string toFile)
        {
            Compress(toFile, "");
        }

        /// <summary>
        /// 压缩已添加的文件并生成.zip文件
        /// </summary>
        /// <param name="toFile">输出的zip文件路径</param>
        /// <param name="passCode">压缩文件密码</param>
        public static void Compress(string toFile,string passCode)
        {
            InternalCompress(toFile, passCode);
        }

        /// <summary>
        /// 压缩已添加的文件并生成.zip文件
        /// </summary>
        /// <param name="toFile">输出的zip文件路径</param>
        /// <param name="passCode">压缩文件密码</param>
        internal static void InternalCompress(string toFile,string passCode)
        {
            //验证文件路径是否是以zip结尾，如果不是的话补上，防止输出的文件没有后缀名
            toFile = VerifyFileName(toFile);
            try
            {
                //如果文件已经存在则删除文件，防止输出的时候无法覆盖或Append数据进入已存在的文件导致数据损坏
                if (File.Exists(toFile))
                {
                    File.Delete(toFile);
                }
                //创建文件，并以写入的方式打开文件流
                using (FileStream fileStream = File.Create(toFile))
                {
                    //创建一个zip压缩输出流
                    using (ZipOutputStream zipStream = new ZipOutputStream(fileStream))
                    {
                        //设置压缩等级，越高压缩效果越好，最大值9
                        zipStream.SetLevel(6);
                        //如果压缩密码不为空则设置压缩密码
                        if (!string.IsNullOrEmpty(passCode))
                        {
                            zipStream.Password = passCode;
                        }
                        //循环已添加至压缩文件列表的文件进行压缩
                        foreach (FileSystemInfo zipFile in ZipFileList)
                        {
                            AddEntry(zipStream, zipFile);
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                //如果发生异常则认为压缩失败，删除已输出的文件
                LogUtil.WriteException(ex.ToString());
                if (File.Exists(toFile))
                {
                    File.Delete(toFile);
                }
            }
        }

        /// <summary>
        /// 文件名校验，防止传入或输出的不是zip压缩文件
        /// </summary>
        /// <param name="fromName"></param>
        private static string VerifyFileName(string fromName)
        {
            if (Path.GetExtension(fromName) != ".zip")
            {
                fromName = fromName + ".zip";
            }
            return fromName;
        }

        /// <summary>
        /// 测试压缩包
        /// </summary>
        /// <param name="fromFile">要测试的压缩包路径</param>
        /// <returns></returns>
        public static bool CheckArchive(string fromFile)
        {
            //如果文件不存在就返回测试失败
            if (!File.Exists(fromFile))
            {
                return false;
            }
            //定义zip文件
            ZipFile zipFile = new ZipFile(fromFile);
            //进行低数据的CRC检查，只要检查到一个错误就认为压缩包错误
            return zipFile.TestArchive(true, TestStrategy.FindFirstError, null);
        }

        /// <summary>
        /// 解压压缩文件
        /// </summary>
        /// <param name="fromFile">要解压的压缩文件</param>
        /// <param name="toPath">要解压到的目录</param>
        public static void DeCompress(string fromFile,string toPath)
        {
            DeCompress(fromFile, toPath, "");
        }

        /// <summary>
        /// 解压压缩文件
        /// </summary>
        /// <param name="fromFile">要解压的压缩文件</param>
        /// <param name="toPath">要解压到的目录</param>
        /// <param name="passCode">压缩文件密码</param>
        public static void DeCompress(string fromFile,string toPath,string passCode)
        {
            InternalDeCompress(fromFile, toPath, passCode);
        }

        /// <summary>
        /// 解压压缩文件
        /// </summary>
        /// <param name="fromFile">要解压的压缩文件</param>
        /// <param name="toPath">要解压到的目录</param>
        /// <param name="passCode">压缩文件密码</param>
        internal static void InternalDeCompress(string fromFile,string toPath,string passCode)
        {
            //验证文件路径是否是以zip结尾，如果不是的话补上，防止找不到要解压的文件
            fromFile = VerifyFileName(fromFile);
            //如果要解压的文件不存在或要解压的不是支持的压缩文件就直接返回
            if (!File.Exists(fromFile) || !CheckArchive(fromFile))
            {
                return;
            }
            //如果要解压至的路径不存在就创建
            if (!Directory.Exists(toPath))
            {
                Directory.CreateDirectory(toPath);
            }
            //定义一个zip压缩输入流并打开压缩文件，将压缩文件读入zip压缩输入流内
            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(fromFile)))
            {
                //如果压缩文件密码不为空就设置压缩文件密码
                if (!string.IsNullOrEmpty(passCode))
                {
                    zipStream.Password = passCode;
                }
                ZipEntry currentEntry;
                try
                {
                    //如果压缩文件输入流能获取到下一个压缩文件对象，就将他解压输出
                    while ((currentEntry = zipStream.GetNextEntry())!=null)
                    {
                        //如果压缩文件对象的名字为空，就认为是损坏或不支持的文件或文件路径，跳过解压
                        if (string.IsNullOrEmpty(currentEntry.Name))
                        {
                            continue;
                        }
                        //获取到压缩文件对象的完整路径
                        string fullEntryToPath = Path.Combine(toPath, currentEntry.Name);
                        //如果压缩文件对象是文件夹
                        if (currentEntry.IsDirectory)
                        {
                            //定义文件夹属性为普通文件夹
                            FileAttributes attributes = FileAttributes.Normal;
                            //创建文件夹
                            Directory.CreateDirectory(fullEntryToPath);
                            //如果文件夹以.开头，就认为是隐藏文件夹，如.svn是隐藏文件夹，.vs也是隐藏文件夹
                            if (currentEntry.Name.StartsWith("."))
                            {
                                //设置文件夹属性为隐藏文件夹
                                File.SetAttributes(fullEntryToPath, attributes | FileAttributes.Hidden);
                            }
                            //调用事件通知外部处理事件信息
                            OnDeCompress?.Invoke(fullEntryToPath, 1, 1f, true);
                        }
                        //如果压缩文件对象是文件
                        else if (currentEntry.IsFile)
                        {
                            //创建文件并打开写入的文件流
                            using (FileStream fileStream = File.Create(fullEntryToPath))
                            {
                                //定义一个变量存放实际从压缩文件流内读取到的文件数据量
                                int readNum=0;
                                //定义一个变量存放已读取的文件总数据量
                                uint totalReadSize = 0;
                                //定义一个1k的数据包
                                byte[] readDataBytes = new byte[1024];
                                do
                                {
                                    //从压缩文件流中读取数据
                                    readNum = zipStream.Read(readDataBytes, 0, readDataBytes.Length);
                                    //更新已读取到的总数据量
                                    totalReadSize += (uint)readNum;
                                    //调用事件通知外部处理事件信息
                                    OnDeCompress?.Invoke(fullEntryToPath, totalReadSize, currentEntry.Size, false);
                                    //输出文件数据到解压后的文件
                                    fileStream.WriteAsync(readDataBytes, 0, readNum).Wait();
                                } while (readNum >0);
                            }
                        }
                    }
                }
                //如果发生压缩文件异常
                catch (ZipException ze)
                {
                    //删除已解压的文件
                    if (File.Exists(toPath))
                    {
                        File.Delete(toPath);
                    }
                    //清空要解压到的目录并删除目录
                    else if (Directory.Exists(toPath))
                    {
                        Directory.Delete(toPath, true);
                    }
                    //如果错误信息包含No password set.则认为没有设置解压密码，抛出明文异常
                    if (ze.Message == "No password set.")
                    {
                        throw new ArgumentNullException("passCode", "请设置解压密码!");
                    }
                }
                //如果发生其他异常
                catch (Exception ex)
                {
                    ////删除已解压的文件
                    if (File.Exists(toPath))
                    {
                        File.Delete(toPath);
                    }
                    ////清空要解压到的目录并删除目录
                    else if (Directory.Exists(toPath))
                    {
                        Directory.Delete(toPath, true);
                    }
                    LogUtil.WriteException(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 压缩字节流
        /// </summary>
        /// <param name="fromBytes">要压缩的字节流</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] fromBytes)
        {
            return fromBytes.Compress();
        }

        /// <summary>
        /// 解压字节流
        /// </summary>
        /// <param name="fromBytes">要解压的字节流</param>
        /// <returns></returns>
        public static byte[] DeCompress(byte[] fromBytes)
        {
            return fromBytes.DeCompress();
        }
    }
}
