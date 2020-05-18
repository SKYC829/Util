using System;
using System.Text;

namespace Util.IO.Log
{
    /// <summary>
    /// 日志帮助类
    /// <para>封装了一些方法快捷的输出日志</para>
    /// </summary>
    public class LogDate
    {
        /// <summary>
        /// 一个用来获取当前时间的委托方法
        /// </summary>
        private static SimpleDelegateCode<DateTime> getNow;

        /// <summary>
        /// 记录日志时，两次日志的最小间隔时间（单位毫秒）
        /// </summary>
        private static int recordMinValue = 1;

        /// <summary>
        /// 起始时间，表示日志开始记录的时间
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// 结束时间，表示日志记录完成的时间
        /// </summary>
        private DateTime endTime;

        /// <summary>
        /// 表示最后一次执行时的耗时
        /// </summary>
        private double lastSecond;

        /// <summary>
        /// 表示执行的总耗时
        /// </summary>
        private double totalSecond;

        /// <summary>
        /// 日志正文
        /// </summary>
        private StringBuilder logBody;

        /// <summary>
        /// 记录日志时，两次日志的最小间隔时间（单位毫秒）
        /// </summary>
        public static int RecordMinValue
        {
            get { return recordMinValue; }
            set { recordMinValue = value; }
        }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static LogDate()
        {
            //初始化获取当前时间的委托方法
            getNow = delegate ()
            {
                return DateTime.Now;
            };
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogDate() : this(string.Empty)
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logTitle">日志的标题</param>
        public LogDate(string logTitle)
        {
            Reset();
            Begin(logTitle);
        }

        /// <summary>
        /// 重置此类的属性
        /// </summary>
        private void Reset()
        {
            //重新初始化所有属性
            startTime = getNow();
            endTime = getNow();
            lastSecond = 0.0;
            totalSecond = 0.0;
            logBody = new StringBuilder();
        }

        /// <summary>
        /// 开始记录日志
        /// </summary>
        private void Begin()
        {
            //重置开始记录日志的时间
            startTime = getNow();
        }

        /// <summary>
        /// 开始记录日志
        /// </summary>
        /// <param name="logTitle">日志的标题</param>
        private void Begin(string logTitle)
        {
            Begin();
            if (!string.IsNullOrEmpty(logTitle))
            {
                logBody.Append(string.Format("-----------------------------{0}-----------------------------", logTitle));
            }
        }

        /// <summary>
        /// 提交日志
        /// </summary>
        public void Commit()
        {
            //更新结束记录日志的时间
            endTime = getNow();
            //得到结束时间和开始时间之间相差的总毫秒数
            double totalMilliseconds = (endTime - startTime).TotalMilliseconds;
            //利用结束时间和开始时间之间相差的总毫秒数减掉上一次记录时的总耗时得到最新的耗时
            lastSecond = totalMilliseconds - totalSecond;
            //更新上一次记录的总耗时
            totalSecond = totalMilliseconds;
        }

        /// <summary>
        /// 提交日志
        /// </summary>
        /// <param name="logBody">日志内容</param>
        public void Commit(string logBody)
        {
            Commit();
            if (!string.IsNullOrEmpty(logBody))
            {
                this.logBody.AppendFormat("\r\n{0}.(耗时：{1:F2}ms，总耗时：{2:F2}ms).", logBody, lastSecond, totalSecond);
            }
        }

        /// <summary>
        /// 提交日志
        /// </summary>
        /// <param name="logFormat">日志内容的字符串</param>
        /// <param name="logArgs">日志内容字符串的参数</param>
        public void Commit(string logFormat, params object[] logArgs)
        {
            string text = logFormat;
            try
            {
                text = string.Format(logFormat, logArgs);
            }
            finally
            {
                Commit(text);
            }
        }

        /// <summary>
        /// 将日志输出到队列，排队输出到日志文件
        /// </summary>
        public void WriteLog()
        {
            //如果总耗时大于最小记录的耗时就添加日志到队列
            if (totalSecond >= RecordMinValue)
            {
                try
                {
                    string text = logBody.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        LogUtil.WriteLog(text, new object[0]);
                    }
                }
                catch (Exception logException)
                {
                    LogUtil.WriteLog(logException);
                }
                finally
                {
                    Reset();
                }
            }
        }
    }
}
