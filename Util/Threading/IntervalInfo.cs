using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Util.Threading
{
    /// <summary>
    /// 线程间隔的实体类对象
    /// </summary>
    public class IntervalInfo
    {
        /// <summary>
        /// 当前线程
        /// </summary>
        public Thread CurrentThread { get; set; }

        /// <summary>
        /// 线程循环执行时的间隔时间（单位:毫秒）
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 线程要执行的方法
        /// </summary>
        public SimpleDelegateCode ExecuteCode { get; set; }

        /// <summary>
        /// 是否中断
        /// </summary>
        public bool Break { get; set; }
    }
}
