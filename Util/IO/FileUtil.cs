using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Util.IO.Log;

namespace Util.IO
{
    /// <summary>
    /// 文件帮助类
    /// <para>包含了一些对文件的快速操作</para>
    /// </summary>
    public class FileUtil
    {
        /// <summary>
        /// 获取应用程序的运行路径
        /// </summary>
        public static string ApplicationPath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        /// <summary>
        /// 获取应用程序的友好名称
        /// </summary>
        public static string ApplicationName
        {
            get { return AppDomain.CurrentDomain.FriendlyName; }
        }

        /// <summary>
        /// 判断一个路径是文件夹还是文件
        /// </summary>
        /// <param name="fromPath">要判断的路径</param>
        /// <returns></returns>
        public static bool IsDirectory(string fromPath)
        {
            //如果能获取到路径指向的文件或文件夹的扩展名，就认为是文件，否则是文件夹
            return Path.GetExtension(fromPath) == null;
        }

        /// <summary>
        /// 删除一个文件或文件夹
        /// <para>注意:</para>
        /// <para>此方法删除文件夹时，会将子文件、子文件夹一同删除</para>
        /// </summary>
        /// <param name="fromPath">要删除的文件或文件夹</param>
        public static void Delete(string fromPath)
        {
            try
            {
                //如果要删除的文件或文件夹不是空路径
                if (!string.IsNullOrEmpty(fromPath))
                {
                    //如果是文件夹
                    if (IsDirectory(fromPath))
                    {
                        //删除文件夹并删除子文件夹和子文件
                        Directory.Delete(fromPath, true);
                    }
                    else
                    {
                        //删除文件
                        File.Delete(fromPath);
                    }
                }
            }
            catch (IOException ie)
            {
                //有可能会出现文件占用、文件不存在、没有权限的错误
                LogUtil.WriteException(ie.ToString());
            }
            catch (Exception ex)
            {
                LogUtil.WriteException(ex.ToString());
            }
        }

        /// <summary>
        /// 展示保存文件对话框
        /// </summary>
        /// <param name="fileName">要保存的文件名</param>
        /// <param name="extensionFilter">要保存的文件的后缀
        /// <para>例如:</para>
        /// <para>所有文件(*.*)|*.*</para>
        /// </param>
        /// <param name="defaultExtension">默认文件后缀</param>
        /// <returns></returns>
        public static string ShowSaveFileDialog(string fileName, string extensionFilter, string defaultExtension)
        {
            return ShowSaveFileDialog(fileName, extensionFilter, defaultExtension, null);
        }

        /// <summary>
        /// 展示保存文件对话框
        /// </summary>
        /// <param name="fileName">要保存的文件名</param>
        /// <param name="extensionFilter">要保存的文件的后缀
        /// <para>例如:</para>
        /// <para>所有文件(*.*)|*.*</para>
        /// </param>
        /// <param name="defaultExtension">默认文件后缀</param>
        /// <param name="onSave">在保存时要执行的委托方法</param>
        /// <returns></returns>
        public static string ShowSaveFileDialog(string fileName, string extensionFilter, string defaultExtension, SimpleDelegateCode onSave)
        {
            //初始化返回值
            string result = string.Empty;
            //调用Win32的保存文件对话框
            SaveFileDialog fileDialog = new SaveFileDialog();
            //设置默认显示的名字
            fileDialog.FileName = fileName;
            //定义默认的文件后缀
            string defaultFilter = "所有文件(*.*)|*.*";
            //设置文件后缀和默认的文件后缀
            fileDialog.Filter = string.IsNullOrEmpty(extensionFilter) ? defaultFilter : extensionFilter;
            fileDialog.DefaultExt = string.IsNullOrEmpty(defaultExtension) ? defaultFilter : defaultExtension;
            //打开保存文件对话框
            bool? dialogResult = fileDialog.ShowDialog();
            //如果对话框点的是确定、OK、是，则获取保存的文件的名字，并执行委托方法
            if (dialogResult.HasValue && dialogResult.Value)
            {
                result = fileDialog.FileName;
                onSave?.Invoke();
            }
            return result;
        }

        /// <summary>
        /// 展示打开文件对话框
        /// </summary>
        /// <param name="rootPath">对话框初始路径</param>
        /// <param name="extensionFilter">要打开的文件后缀的限制
        /// <para>例如:</para>
        /// <para>所有文件(*.*)|*.*</para>
        /// </param>
        /// <returns></returns>
        public static string ShowOpenFileDialog(string rootPath, string extensionFilter)
        {
            return ShowOpenFileDialog(rootPath, extensionFilter, onOpen: null);
        }

        /// <summary>
        /// 展示打开文件对话框
        /// </summary>
        /// <param name="rootPath">对话框初始路径</param>
        /// <param name="extensionFilter">要打开的文件后缀的限制
        /// <para>例如:</para>
        /// <para>所有文件(*.*)|*.*</para>
        /// </param>
        /// <param name="onOpen">在打开时要执行的委托方法</param>
        /// <returns></returns>
        public static string ShowOpenFileDialog(string rootPath, string extensionFilter, SimpleDelegateCode onOpen)
        {
            return ShowOpenFileDialog(rootPath, extensionFilter, false, onOpen).FirstOrDefault();
        }

        /// <summary>
        /// 展示打开文件对话框
        /// </summary>
        /// <param name="rootPath">对话框初始路径</param>
        /// <param name="extensionFilter">要打开的文件后缀的限制
        /// <para>例如:</para>
        /// <para>所有文件(*.*)|*.*</para>
        /// </param>
        /// <param name="canMulti">是否允许多选</param>
        /// <returns></returns>
        public static string[] ShowOpenFileDialog(string rootPath, string extensionFilter, bool canMulti)
        {
            return ShowOpenFileDialog(rootPath, extensionFilter, canMulti, null);
        }

        /// <summary>
        /// 展示打开文件对话框
        /// </summary>
        /// <param name="rootPath">对话框初始路径</param>
        /// <param name="extensionFilter">要打开的文件后缀的限制
        /// <para>例如:</para>
        /// <para>所有文件(*.*)|*.*</para>
        /// </param>
        /// <param name="canMulti">是否允许多选</param>
        /// <param name="onOpen">在打开时要执行的委托方法</param>
        /// <returns></returns>
        public static string[] ShowOpenFileDialog(string rootPath, string extensionFilter, bool canMulti, SimpleDelegateCode onOpen)
        {
            //初始化返回值
            List<string> results = new List<string>();
            //调用Win32的打开文件对话框
            OpenFileDialog fileDialog = new OpenFileDialog();
            //如果初始路径为空，就设置为应用程序根目录，否则会被定位到C盘
            if (string.IsNullOrEmpty(rootPath))
            {
                rootPath = ApplicationPath;
            }
            //设置初始路径
            fileDialog.InitialDirectory = rootPath;
            //定义默认的文件后缀
            string defaultFilter = "所有文件(*.*)|*.*";
            //设置文件后缀和默认的文件后缀
            fileDialog.Filter = string.IsNullOrEmpty(extensionFilter) ? defaultFilter : extensionFilter;
            //设置是否允许选择多个文件
            fileDialog.Multiselect = canMulti;
            //打开 打开文件对话框
            bool? dialogResult = fileDialog.ShowDialog();
            //如果对话框点的是确定、OK、是，则获取保存的文件的名字，并执行委托方法
            if (dialogResult.HasValue && dialogResult.Value)
            {
                results.AddRange(fileDialog.FileNames);
                onOpen?.Invoke();
            }
            return results.ToArray();
        }

        /// <summary>
        /// 判断文件是否被占用
        /// </summary>
        /// <param name="filePath">要判断的文件的路径</param>
        /// <returns></returns>
        public static bool IsFileOccupied(string filePath)
        {
            return IsFileOccupied(new FileInfo(filePath));
        }

        /// <summary>
        /// 判断文件是否被占用
        /// </summary>
        /// <param name="fileInfo">要判断的文件信息</param>
        /// <returns></returns>
        public static bool IsFileOccupied(FileInfo fileInfo)
        {
            //初始化返回值
            bool result = true;
            try
            {
                //通过文件流打开读取文件，如果能打开读取，就认为没被占用
                using (FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    result = false;
                }
            }
            catch (FileNotFoundException)
            {
                //如果报错说文件不存在，也认为没被占用
                result = false;
            }
            catch (IOException)
            {
                //如果是其他的报错，就认为文件被占用了
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="fileName">要获取MD5值的文件路径</param>
        /// <returns></returns>
        public static string GetFileMD5(string fileName)
        {
            return GetFileMD5(new FileInfo(fileName));
        }

        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="fileInfo">要获取MD5值的文件信息</param>
        /// <returns></returns>
        public static string GetFileMD5(FileInfo fileInfo)
        {
            //初始化返回值
            string result = string.Empty;
            //如果文件被占用，则认为计算失败，返回空值
            if (fileInfo.IsOccupied())
            {
                return result;
            }
            try
            {
                //如果文件不存在，则认为计算失败，返回空值
                if (!fileInfo.Exists)
                {
                    return result;
                }
                //用文件流打开文件，读取出所有数据
                using (FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    //定义一个数据包，用于存放文件数据
                    byte[] readFileDataBytes = new byte[fileInfo.Length];
                    //读取数据
                    fileStream.Read(readFileDataBytes, 0, readFileDataBytes.Length);
                    //将读取到的数据进行MD5加密
                    result = readFileDataBytes.EncryptMD5();
                }
            }
            catch (Exception ex)
            {
                //如果发生仍和异常就认为加密失败
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 对比两个文件的MD5
        /// </summary>
        /// <param name="fromFilePath">要对比的文件1的路径</param>
        /// <param name="toFilePath">要对比的文件2的路径</param>
        /// <returns></returns>
        public static bool VerifyFileMD5(string fromFilePath, string toFilePath)
        {
            return VerifyFileMD5(new FileInfo(fromFilePath), new FileInfo(toFilePath));
        }

        /// <summary>
        /// 对比两个文件的MD5
        /// </summary>
        /// <param name="fromFileInfo">要对比的文件1的信息</param>
        /// <param name="toFileInfo">要对比的文件2的信息</param>
        /// <returns></returns>
        public static bool VerifyFileMD5(FileInfo fromFileInfo, FileInfo toFileInfo)
        {
            return VerifyMD5(GetFileMD5(fromFileInfo), GetFileMD5(toFileInfo));
        }

        /// <summary>
        /// 对比两个二进制数组的MD5
        /// </summary>
        /// <param name="fromMD5Bytes">要对比的二进制数组1</param>
        /// <param name="toMD5Bytes">要对比的二进制数组2</param>
        /// <returns></returns>
        public static bool VerifyMD5(byte[] fromMD5Bytes, byte[] toMD5Bytes)
        {
            return VerifyMD5(fromMD5Bytes.EncryptMD5(), toMD5Bytes.EncryptMD5());
        }

        /// <summary>
        /// 对比两个MD5
        /// </summary>
        /// <param name="fromMD5">要对比的MD51</param>
        /// <param name="toMD5">要对比的MD52</param>
        /// <returns></returns>
        internal static bool VerifyMD5(string fromMD5, string toMD5)
        {
            return !string.IsNullOrEmpty(fromMD5)
                && !string.IsNullOrEmpty(toMD5)
                && fromMD5 == toMD5;
        }
    }
}
