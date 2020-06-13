using System;
using System.Collections.Generic;
using System.Linq;
using Util.DataObject;

namespace Util.Common
{
    /// <summary>
    /// 随机数帮助类
    /// <para>可以返回一个或多个随机数</para>
    /// <para>也可以根据权重返回一个或多个随机数</para>
    /// </summary>
    public class RandomUtil
    {
        /// <summary>
        /// 随机抽取个数
        /// </summary>
        public static uint RandomCount { get; private set; }

        /// <summary>
        /// 是否允许抽取的结果存在重复
        /// </summary>
        public static bool CanRepeat { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public RandomUtil() : this(1)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="randomCount">随机数的个数，默认为1，最小可以为0</param>
        public RandomUtil(uint randomCount)
        {
            RandomCount = randomCount;
        }

        /// <summary>
        /// 获取随机数
        /// <para>将根据传入的随机数个数返回一组随机数</para>
        /// </summary>
        /// <returns></returns>
        public static int[] Next()
        {
            return Next(new Random());
        }

        /// <summary>
        /// 获取随机数
        /// <para>将根据传入的随机数个数返回一组随机数</para>
        /// </summary>
        /// <param name="random">随机数生成器</param>
        /// <returns></returns>
        public static int[] Next(Random random)
        {
            //初始化返回值
            List<int> result = new List<int>();
            //如果随机数生成器不为空
            if (random != null)
            {
                //循环随机数抽取个数的次数计算最大值为随机数抽取个数+1的数字
                for (int i = 0; i < RandomCount; i++)
                {
                    int item = random.Next((int)RandomCount + 1);
                    //如果返回值列表已包含这个数字并且不允许重复
                    //将i减一并重新循环
                    if (result.Contains(item) && !CanRepeat)
                    {
                        i--;
                        continue;
                    }
                    result.Add(item);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 从一组对象中随机抽取对象
        /// <para>注意:</para>
        /// <para>此方法若关闭了<see cref="CanRepeat"/></para>
        /// <para>返回的对象个数不一定会等于要抽取的个数！</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jackPot">对象列表</param>
        /// <returns></returns>
        public static T[] Draw<T>(IList<T> jackPot)
        {
            return Draw(new Random(), jackPot);
        }

        /// <summary>
        /// 从一组对象中随机抽取对象
        /// <para>注意:</para>
        /// <para>此方法若关闭了<see cref="CanRepeat"/></para>
        /// <para>返回的对象个数不一定会等于要抽取的个数！</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="random">随机数生成器</param>
        /// <param name="jackPot">对象列表</param>
        /// <returns></returns>
        public static T[] Draw<T>(Random random, IList<T> jackPot)
        {
            List<ushort> result = new List<ushort>();
            //为所有对象生成统一的权重
            for (int i = 0; i < jackPot.Count; i++)
            {
                result.Add(1);
            }
            return Draw<T>(random, jackPot, result);
        }

        /// <summary>
        /// 从一组对象中随机抽取对象
        /// <para>注意:</para>
        /// <para>此方法若关闭了<see cref="CanRepeat"/></para>
        /// <para>返回的对象个数不一定会等于要抽取的个数！</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="random">随机数生成器</param>
        /// <param name="jackPot">对象列表</param>
        /// <param name="weights">对象的权重<para>此属性控制对象被抽中的概率</para><para>权重为0则永远不会被抽中，权重值越大，抽中的概率越高</para><para>该值按百分比计算，可以高于100%</para></param>
        /// <returns></returns>
        public static T[] Draw<T>(Random random, IList<T> jackPot, IList<ushort> weights)
        {
            //将对象和权重关联起来
            Dictionary<T, ushort> jackPotAndWeights = new Dictionary<T, ushort>();
            for (int i = 0; i < jackPot.Count; i++)
            {
                jackPotAndWeights.Add(jackPot[i], (ushort)(random.Next(100) * weights[i]));
            }
            return Draw<T>(random, jackPotAndWeights);
        }

        /// <summary>
        /// 从一组对象中随机抽取对象
        /// <para>注意:</para>
        /// <para>此方法若关闭了<see cref="CanRepeat"/></para>
        /// <para>返回的对象个数不一定会等于要抽取的个数！</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="random">随机数生成器</param>
        /// <param name="jackPotAndWeights">对象和权重的映射（map）</param>
        /// <returns></returns>
        public static T[] Draw<T>(Random random, IDictionary<T, ushort> jackPotAndWeights)
        {
            //初始化返回值
            List<T> result = new List<T>();
            //如果随机数生成器不为空
            if (random != null)
            {
                //根据对象和权重的映射的权重对字典进行排序并获取前RandomCount位
                List<KeyValuePair<T, ushort>> sortJackPot = jackPotAndWeights.Sort().ToList().GetRange(0, (int)RandomCount);
                //遍历排序后的每一个元素
                for (int i = 0; i < sortJackPot.Count; i++)
                {
                    //如果返回值列表已包含当前对象并且不可重复，就进行下一次循环
                    KeyValuePair<T, ushort> jackPot = sortJackPot[i];
                    if (result.Contains(jackPot.Key) && !CanRepeat)
                    {
                        continue;
                    }
                    result.Add(jackPot.Key);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 生成固定长度的随机数字字符串
        /// <para>可用于生成订单编号或交易流水号</para>
        /// <para>绝对不会重复</para>
        /// </summary>
        /// <param name="length">要生成的字符串的长度</param>
        /// <returns></returns>
        public static string GenerateRandomNo(int length)
        {
            //根据GUID计算种子
            int seed = Guid.NewGuid().GetHashCode();
            //根据种子生成随机数
            Random random = new Random(seed);
            //生成初始随机数数组
            int[] index = new int[length];
            for (int i = 0; i < length; i++)
            {
                index[i] = i + 1;
            }
            //用来保存随机生成的不重复的数 
            int[] resultArray = new int[length];
            //设置上限
            int max = length;
            int currentIndex;
            //获取index数组中索引为currentIndex位置的数据，赋给结果数组resultArray的i索引位置
            for (int i = 0; i < length; i++)
            {
                //生成随机索引数
                currentIndex = random.Next(0, max - 1);
                //在随机索引位置取出一个数，保存到结果数组
                resultArray[i] = index[currentIndex];
                //作废当前索引位置数据，并用数组的最后一个数据代替之
                index[currentIndex] = index[max - 1];
                //索引位置的上限减一（弃置最后一个数据）
                max--;
            }
            string result = string.Empty;
            //将生成出来的随机数数组转换为字符串
            for (int i = 0; i < resultArray.Length; i++)
            {
                result += StringUtil.GetString(resultArray[i]);
            }
            //限制字符串长度为生成的字符串的长度
            if (result.Length > length)
            {
                result = result.Substring(0,length);
            }
            return result;
        }
    }
}
