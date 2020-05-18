using System;

namespace Util.IO.Log
{
    /// <summary>
    /// 日志信息的实体类对象
    /// </summary>
    [Serializable]
    internal class LogInfo
    {
        /// <summary>
        /// 记录日志的时间
        /// </summary>
        public DateTime LogTime { get; set; }

        /// <summary>
        /// 记录的日志信息
        /// </summary>
        public string LogBody { get; set; }

        /// <summary>
        /// 记录的异常信息
        /// </summary>
        public Exception InnerException { get; set; }

        /// <summary>
        /// 短时间内触发同一个异常的次数
        /// </summary>
        public int LogCount { get; set; }

        /// <summary>
        /// 记录的日志的类型
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logType">记录的日志的类型</param>
        /// <param name="logTime">记录日志的时间</param>
        /// <param name="logBody">记录的日志信息</param>
        public LogInfo(LogType logType, DateTime logTime, string logBody) : this(logType, logTime, null, logBody)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logType">记录的日志的类型</param>
        /// <param name="logTime">记录日志的时间</param>
        /// <param name="innerException">记录的异常信息</param>
        public LogInfo(LogType logType, DateTime logTime, Exception innerException) : this(logType, logTime, innerException, string.Empty)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logType">记录的日志的类型</param>
        /// <param name="logTime">记录日志的时间</param>
        /// <param name="innerException">记录的异常信息</param>
        /// <param name="logBody">记录的日志信息</param>
        public LogInfo(LogType logType, DateTime logTime, Exception innerException, string logBody)
        {
            LogType = logType;
            LogTime = logTime;
            InnerException = innerException;
            LogBody = logBody;
        }
    }
}
