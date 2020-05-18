using System;
using System.Collections;
using System.Collections.Generic;
using Util.IO.Log;

namespace Util.DataObject
{
    /// <summary>
    /// 键值对帮助类
    /// <para>可以较便捷的操作键值对</para>
    /// </summary>
    public partial class DictionaryUtil
    {
        /// <summary>
        /// 根据key获取键值对中对应的值
        /// </summary>
        /// <typeparam name="T">要获取的值的类型</typeparam>
        /// <param name="fromDictionary">要获取值的键值对</param>
        /// <param name="key">要获取值的键</param>
        /// <returns></returns>
        public static T Get<T>(IDictionary fromDictionary, string key)
        {
            //初始化返回值
            T result = default(T);
            //如果来源键值对不是空并且包含要获取值的键
            if (fromDictionary != null && fromDictionary.Contains(key))
            {
                try
                {
                    //获取对应键的值并进行类型转换
                    result = (T)Convert.ChangeType(fromDictionary[key], typeof(T));
                }
                catch (Exception ex)
                {
                    //如果发生异常则认为获取失败
                    LogUtil.WriteException(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 根据key获取键值对中对应的值
        /// </summary>
        /// <typeparam name="T">key的类型</typeparam>
        /// <typeparam name="M">要获取的值的类型</typeparam>
        /// <param name="fromDictionary">要获取值的键值对</param>
        /// <param name="key">要获取值的键</param>
        /// <returns></returns>
        public static M Get<T, M>(IDictionary<T, M> fromDictionary, T key)
        {
            //初始化返回值
            M result = default(M);
            //如果来源键值对不是空并且包含要获取值的键
            if (fromDictionary != null && fromDictionary.ContainsKey(key))
            {
                try
                {
                    //获取对应键的值并进行类型转换
                    result = (M)Convert.ChangeType(fromDictionary[key], typeof(M));
                }
                catch (Exception ex)
                {
                    //如果发生异常则认为获取失败
                    LogUtil.WriteException(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 根据key获取键值对中对应的值
        /// </summary>
        /// <typeparam name="T">要获取的值的类型</typeparam>
        /// <param name="fromDictionary">要获取值的键值对</param>
        /// <param name="key">要获取值的键</param>
        /// <returns></returns>
        public static T Get<T>(Dictionary<string, T> fromDictionary, string key)
        {
            return Get<string, T>(fromDictionary, key);
        }

        /// <summary>
        /// 根据key获取键值对中对应的值
        /// </summary>
        /// <param name="fromDictionary">要获取值的键值对</param>
        /// <param name="key">要获取值的键</param>
        /// <returns></returns>
        public static object Get(IDictionary fromDictionary, string key)
        {
            return Get<object>(fromDictionary, key);
        }
    }
}
