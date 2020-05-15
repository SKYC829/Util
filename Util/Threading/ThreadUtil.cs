using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util.IO.Log;

namespace Util.Threading
{
    /// <summary>
    /// 线程帮助类
    /// <para>包含了一些快速运行线程的方法</para>
    /// </summary>
    public class ThreadUtil
    {
        /// <summary>
        /// 让线程休眠<paramref name="interval"/>毫秒
        /// </summary>
        /// <param name="interval">要休眠的时间</param>
        public static void Sleep(int interval)
        {
            Thread.Sleep(interval);
        }

        /// <summary>
        /// 创建并运行一个线程
        /// </summary>
        /// <param name="executeCode">线程要执行的方法</param>
        /// <returns></returns>
        public static Thread RunThread(SimpleDelegateCode executeCode)
        {
            //创建线程运行的方法
            ThreadStart threadStart = new ThreadStart(delegate ()
            {
                try
                {
                    //如果要执行的方法不为空，就执行该方法
                    executeCode?.Invoke();
                }
                catch (Exception ex)
                {
                    //如果发生异常就记录异常信息
                    LogUtil.WriteLog(ex);
                }
            });
            //创建一个新线程
            Thread executeThread = new Thread(threadStart);
            //设置为后台运行
            executeThread.IsBackground = true;
            //启动线程
            executeThread.Start();
            return executeThread;
        }

        /// <summary>
        /// 创建并运行一个线程
        /// </summary>
        /// <param name="runInterval"></param>
        /// <returns></returns>
        public static Thread RunThread(IntervalInfo runInterval)
        {
            //如果线程间隔的实体类对象为空，就无法创建线程
            if (runInterval == null)
            {
                return null;
            }
            //创建一个线程
            Thread executeThread = RunThread(delegate ()
            {
                //如果实体类对象没有标记为中断，就循环执行
                while (!runInterval.Break)
                {
                    try
                    {
                        //如果实体类对象要执行的方法不为空，就执行该方法
                        runInterval.ExecuteCode?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        //如果发生异常就记录异常信息
                        LogUtil.WriteLog(ex);
                    }
                    //让线程休眠一会
                    Sleep(runInterval.Interval);
                }
            });
            //设置实体类对象当前的线程的信息
            runInterval.CurrentThread = executeThread;
            return executeThread;
        }
    }
}
