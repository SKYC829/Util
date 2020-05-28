using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Util.DataObject;
using Util.Threading;

namespace Util.IO.Log
{
    /// <summary>
    /// 日志帮助类
    /// <para>封装了一些方法快捷的输出日志</para>
    /// </summary>
    public class LogUtil
    {
        /// <summary>
        /// 日志信息缓存，缓存了触发过的异常
        /// </summary>
        private static Dictionary<string, LogInfo> logCache = new Dictionary<string, LogInfo>();

        /// <summary>
        /// 日志队列
        /// </summary>
        private static Queue<LogInfo> logQueue = new Queue<LogInfo>();

        /// <summary>
        /// 正在输出日志的委托方法
        /// </summary>
        public static MultiParamDelegateCode<LogType, string> OnWriteLog { get; set; }

        /// <summary>
        /// 输出日志时，要跳过记录的日志
        /// <para>存在多个类型时，请使用或运算</para>
        /// <para>例如:</para>
        /// <code>
        ///  <see cref="SkipLog"/> = <see cref="LogType.None"/> | <see cref="LogType.Normal"/>
        /// </code>
        /// </summary>
        public static LogType SkipLog { get; set; } = LogType.None;

        /// <summary>
        /// 存放日志的路径
        /// </summary>
        public static string LogPath
        {
            get
            {
                return "~/Temp/Log".ParseUrl();
            }
        }

        /// <summary>
        /// 静态方法，启动输出日志线程
        /// </summary>
        static LogUtil()
        {
            StartLog();
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logInfo">日志信息</param>
        private static void AddEnqueue(LogInfo logInfo)
        {
            //得到日志队列的引用
            Queue<LogInfo> m_LogQueue = logQueue;
            //对日志队列进行上锁，防止在入队时有新数据插入
            lock (m_LogQueue)
            {
                //对日志信息进行入队
                m_LogQueue.Enqueue(logInfo);
            }
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logException">错误信息</param>
        public static void WriteLog(Exception logException)
        {
            WriteLog(logException, string.Empty);
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logTime">记录日志的时间</param>
        /// <param name="logException">错误信息</param>
        public static void WriteLog(DateTime logTime, Exception logException)
        {
            WriteLog(logTime, logException, string.Empty);
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logException">错误信息</param>
        /// <param name="logMessage">日志内容</param>
        public static void WriteLog(Exception logException, string logMessage)
        {
            WriteLog(DateTime.Now, logException, logMessage);
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logTime">记录日志的时间</param>
        /// <param name="logException">错误信息</param>
        /// <param name="logMessage">日志内容</param>
        public static void WriteLog(DateTime logTime, Exception logException, string logMessage)
        {
            AddEnqueue(new LogInfo((logException == null) ? LogType.Normal : LogType.Exception, logTime, logException, logMessage));
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logTime">记录日志的时间</param>
        /// <param name="logType">记录日志的类型</param>
        /// <param name="logMessage">日志内容</param>
        /// <param name="logArgs">日志内容字符串的参数</param>
        public static void WriteLog(DateTime logTime, LogType logType, string logMessage, params object[] logArgs)
        {
            if (logArgs != null && logArgs.Length != 0)
            {
                try
                {
                    logMessage = string.Format(logMessage, logArgs);
                }
                catch (Exception logException)
                {
                    WriteLog(logException);
                }
            }
            AddEnqueue(new LogInfo(logType, logTime, logMessage));
        }

        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="logType">记录日志的类型</param>
        /// <param name="logMessage">日志内容</param>
        /// <param name="logArgs">日志内容字符串的参数</param>
        public static void WriteLog(LogType logType, string logMessage, params object[] logArgs)
        {
            WriteLog(DateTime.Now, logType, logMessage, logArgs);
        }

        /// <summary>
        /// 记录异常信息
        /// </summary>
        /// <param name="logMessage">异常信息</param>
        /// <param name="logArgs">异常信息的参数</param>
        public static void WriteException(string logMessage, params object[] logArgs)
        {
            WriteLog(LogType.Exception, logMessage, logArgs);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="logMessage">警告信息</param>
        /// <param name="logArgs">警告信息的参数</param>
        public static void WriteWarn(string logMessage, params object[] logArgs)
        {
            WriteLog(LogType.Warn, logMessage, logArgs);
        }

        /// <summary>
        /// 记录日志信息
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="logArgs">日志信息的参数</param>
        public static void WriteLog(string logMessage, params object[] logArgs)
        {
            WriteLog(LogType.Normal, logMessage, logArgs);
        }

        /// <summary>
        /// 删除<paramref name="logDays" />天内大于1k的日志
        /// </summary>
        /// <param name="logDays">删除几天内的日志</param>
        public static void DeleteLog(int logDays)
        {
            DeleteLog(logDays, 1024);
        }


        /// <summary>
        /// 删除<paramref name="logDays" />天内大于<paramref name="logSize" />的日志
        /// </summary>
        /// <param name="logDays">删除几天内的日志</param>
        /// <param name="logSize">删除的日志的大小不能小于的大小</param>
        public static void DeleteLog(int logDays, long logSize)
        {
            foreach (FileInfo fileInfo in GetLogFiles(logDays))
            {
                if (fileInfo.Length > logSize)
                {
                    fileInfo.Delete();
                }
            }
        }

        /// <summary>
        /// 获取日志文件
        /// </summary>
        /// <param name="logDays">获取几天内的日志文件</param>
        /// <returns></returns>
        public static List<FileInfo> GetLogFiles(int logDays)
        {
            List<FileInfo> result = new List<FileInfo>();
            DirectoryInfo directoryInfo = new DirectoryInfo(LogPath);
            if (directoryInfo.Exists)
            {
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.log", SearchOption.AllDirectories))
                {
                    DateTime creationTime = fileInfo.CreationTime;
                    if ((DateTime.Now - creationTime).TotalDays >= logDays)
                    {
                        result.Add(fileInfo);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 将日志输出到文件
        /// </summary>
        private static void WriteToFile()
        {
            //获取日志队列的引用
            Queue<LogInfo> m_LogQueue = logQueue;
            //如果日志队列没有内容，就直接返回，等待下次输出
            if (m_LogQueue.Count < 1)
            {
                return;
            }
            //得到输出的日志文件名
            string path = string.Format("{0}/{1}.log", LogPath, DateTime.Now.ToString("yyyyMMdd"));
            //得到要输出的日志的路径
            string directoryName = Path.GetDirectoryName(path);
            //如果路径不存在就创建
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            //定义一个输出流
            StreamWriter streamWriter = new StreamWriter(path, true, Encoding.Default);
            //循环处理日志信息
            do
            {
                try
                {
                    //定义一个变量存放队列中的第一个日志信息
                    LogInfo logInfo = null;
                    //如果队列中没有元素，就直接跳出循环
                    if (m_LogQueue.Count == 0)
                    {
                        break;
                    }
                    //给队列上锁，防止出队时有日志信息插入
                    lock (m_LogQueue)
                    {
                        //出队得到第一个日志信息
                        logInfo = m_LogQueue.Dequeue();
                    }
                    //如果日志信息为空，就进行下一次循环
                    if (logInfo == null)
                    {
                        continue;
                    }
                    //如果日志信息的日志类型在要跳过输出的日志类型中,就进行下一次循环
                    if ((SkipLog & logInfo.LogType) != 0)
                    {
                        continue;
                    }
                    //处理日志的异常信息
                    if (logInfo.InnerException != null)
                    {
                        //如果日志内容为空，就把异常信息的消息作为日志内容
                        if (string.IsNullOrEmpty(logInfo.LogBody))
                        {
                            logInfo.LogBody = string.Format("{0}:\r\n{1}", logInfo.InnerException.Message, logInfo.InnerException.StackTrace);
                        }
                        //否则将异常信息的消息作为标题，日志内容作为主要内容拼接起来
                        else
                        {
                            logInfo.LogBody = string.Format("{0}:\r\n{1}\r\n{2}", logInfo.LogBody, logInfo.InnerException.Message, logInfo.InnerException.StackTrace);
                        }
                        //通过日志内容计算一个唯一的值
                        string key = logInfo.LogBody.EncryptMD5();
                        //在日志信息缓存中获取已缓存的日志信息
                        LogInfo cacheLogInfo = DictionaryUtil.Get<LogInfo>(logCache, key);
                        //如果获取不到，就把当前日志信息记录到缓存中
                        if (cacheLogInfo == null)
                        {
                            logCache[key] = logInfo;
                        }
                        else
                        {
                            //否则让缓存中的日志信息的触发次数+1
                            cacheLogInfo.LogCount++;
                            //如果已缓存的日志信息的记录时间减掉当前日志信息的记录时间小于15分钟，就认为是频繁触发的日志，频繁触发的日志每15分钟记录一次，防止过于频繁的输出日志导致日志可读性太差
                            if ((logInfo.LogTime - cacheLogInfo.LogTime).TotalMinutes < 15)
                            {
                                continue;
                            }
                            //将十五分钟内触发的次数拼接到日志内容中
                            logInfo.LogBody = string.Format("{0}.(15分钟内触发了{1}次)", logInfo.LogBody, cacheLogInfo.LogCount);
                            //重置缓存内的日志的触发次数
                            cacheLogInfo.LogCount = 0;
                            //重置缓存内的日志的记录时间
                            cacheLogInfo.LogTime = logInfo.LogTime;
                        }
                    }
                    //如果日志信息的内容为空，就不输出日志
                    if (string.IsNullOrEmpty(logInfo.LogBody))
                    {
                        continue;
                    }
                    //将日志记录时间和日志信息内容拼接成一条字符串用于输出
                    string text = string.Format("\r\n{0:HH:mm:ss}:\r\n{1}", logInfo.LogTime, logInfo.LogBody);
#if DEBUG
					//如果是Debug模式，就不输出到文件，节约磁盘资源
					Console.WriteLine(text);
					Debug.WriteLine(text);
#else
                    //如果输出流不为空，就将日志输出到文件
                    if (streamWriter != null)
                    {
                        streamWriter.WriteLine(text);
                    }
#endif
                    //触发正在记录日志委托方法通知外部进行处理
                    OnWriteLog?.Invoke(logInfo.LogType, logInfo.LogBody);
                }
                catch (Exception ex)
                {
                    //如果发生异常就迭代记录日志
                    WriteLog(ex);
                }
                //如果输出流不为空且日志队列已经没有元素
                if (streamWriter != null && m_LogQueue.Count == 0)
                {
                    //等待100毫秒，防止文件打开关闭消耗资源
                    Thread.Sleep(100);
                }
            } while (true);
            try
            {
                //如果输出流不为空
                if (streamWriter != null)
                {
                    //关闭输出流并释放资源
                    streamWriter.Close();
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
            catch (Exception ex)
            {
                //如果发生异常就迭代记录日志
                WriteLog(ex);
            }
        }

        /// <summary>
        /// 开启一个线程，开始循环记录日志
        /// </summary>
        private static void StartLog()
        {
            //启动一个线程循环输出日志到文件
            ThreadUtil.RunThread(new IntervalInfo()
            {
                Interval = 100,
                ExecuteCode = delegate ()
                {
                    WriteToFile();
                }
            }).Name = "Output Log Thread";
        }
    }
}
