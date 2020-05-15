using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.Common
{
    /// <summary>
    /// 运行方法帮助类
    /// <para>可以将一系列方法按先后顺序,以流水线的方式调用</para>
    /// </summary>
    public class StartCodesUtil
    {
        /// <summary>
        /// 要执行的方法列表
        /// </summary>
        private List<SimpleDelegateCode> executeCodes;

        /// <summary>
        /// 在执行<see cref="Execute"/>方法时是否要停止
        /// </summary>
        public bool IsBreak { get; set; }

        /// <summary>
        /// 当前要执行的方法列表中的方法是否已全部执行完成
        /// </summary>
        public bool IsFinaly
        {
            get
            {
                return executeCodes != null && executeCodes.Count == 0;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StartCodesUtil()
        {
            if(executeCodes == null)
            {
                executeCodes = new List<SimpleDelegateCode>();
            }
        }

        /// <summary>
        /// 开始循环执行要执行的方法列表中的所有方法
        /// </summary>
        public void Execute()
        {
            //只要要执行的方法列表中还有元素,就循环执行
            while (executeCodes.Count>0)
            {
                //如果被标识为终止,则跳出循环
                if (IsBreak)
                {
                    break;
                }
                //执行要执行的方法列表的下一个元素
                ExecuteNext();
            }
        }

        /// <summary>
        /// 执行要执行的方法列表中的第一个方法
        /// </summary>
        public void ExecuteNext()
        {
            //如果要执行的方法列表中没用元素,就直接返回
            if (executeCodes.Count == 0)
            {
                return;
            }
            //锁住要执行的方法列表,防止在取出列表中第一个元素时有新元素插入或移除,导致执行顺序错误
            lock (executeCodes)
            {
                //取出列表中第一个元素
                SimpleDelegateCode executeCode = executeCodes.FirstOrDefault();
                //将他从列表中移除,此步骤相当于队列的出队
                executeCodes.Remove(executeCode);
                //如果取出的元素不为空,就执行该方法
                executeCode?.Invoke();
            }
        }

        /// <summary>
        /// 向要执行的方法列表中增加方法
        /// </summary>
        /// <param name="executeCode">待执行的方法</param>
        public void Add(SimpleDelegateCode executeCode)
        {
            //锁住要执行的方法列表,防止在向要执行的方法列表中增加方法时有新元素插入或移除,导致执行顺序错误
            lock (executeCodes)
            {
                //添加方法到要执行的方法列表
                executeCodes.Add(executeCode);
            }
        }

        /// <summary>
        /// 向要执行的方法列表中插入方法,并放到第一位
        /// </summary>
        /// <param name="executeCode">待执行的方法</param>
        public void Shift(SimpleDelegateCode executeCode)
        {
            Insert(0, executeCode);
        }

        /// <summary>
        /// 向要执行的方法列表中插入方法,并放到指定位置
        /// </summary>
        /// <param name="codeIndex">方法要放至的元素位置</param>
        /// <param name="executeCode">待执行的方法</param>
        public void Insert(int codeIndex,SimpleDelegateCode executeCode)
        {
            //锁住要执行的方法列表,防止在向要执行的方法列表中插入方法,并放到指定位置时有新元素插入或移除,导致执行顺序错误
            lock (executeCodes)
            {
                executeCodes.Insert(codeIndex, executeCode);
            }
        }

        /// <summary>
        /// 从要执行的方法列表中移除待执行的方法
        /// </summary>
        /// <param name="executeCode">待执行的方法</param>
        public void Remove(SimpleDelegateCode executeCode)
        {
            //锁住要执行的方法列表,防止在从要执行的方法列表中移除待执行的方法时有新元素插入或移除,导致执行顺序错误
            lock (executeCodes)
            {
                executeCodes.Remove(executeCode);
            }
        }
    }
}
