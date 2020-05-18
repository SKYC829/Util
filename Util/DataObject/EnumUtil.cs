using System;
using Util.IO.Log;

namespace Util.DataObject
{
    /// <summary>
    /// 枚举帮助类
    /// <para>封装了一些转换以便于快速调用</para>
    /// </summary>
    public class EnumUtil
    {
        /// <summary>
        /// 将一个对象转换为枚举类型
        /// </summary>
        /// <typeparam name="T">要转换的枚举的类型</typeparam>
        /// <param name="from">要转换的对象</param>
        /// <param name="defaultValue">转换失败时的默认值</param>
        /// <returns></returns>
        public static T ConvertEnum<T>(string from, T defaultValue) where T : struct
        {
            //初始化返回值
            T result = defaultValue;
            try
            {
                //如果要转换的对象不是空，就认为可以转换
                if (!string.IsNullOrEmpty(from))
                {
                    //将对象转换为对应的类型，忽略名称大小写区别
                    result = (T)Enum.Parse(typeof(T), from, false);
                }
            }
            catch (Exception ex)
            {
                //如果发生异常则认为转换失败
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 将一个对象转换为枚举类型
        /// </summary>
        /// <typeparam name="T">要转换的枚举的类型</typeparam>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static T ConvertEnum<T>(string from) where T : struct
        {
            return ConvertEnum<T>(from, default(T));
        }

        /// <summary>
        /// 将一个对象转换为枚举类型
        /// </summary>
        /// <param name="type">要转换的枚举的类型</param>
        /// <param name="from">要转换的对象</param>
        /// <param name="defaultValue">转换失败时的默认值</param>
        /// <returns></returns>
        public static object ConvertEnum(Type type, string from, object defaultValue)
        {
            //初始化返回值
            object result = defaultValue;
            try
            {
                //如果要转换的对象不是空，就认为可以转换
                if (!string.IsNullOrEmpty(from))
                {
                    //将对象转换为对应的类型，忽略名称大小写区别
                    result = Enum.Parse(type, from, false);
                }
            }
            catch (Exception ex)
            {
                //如果发生异常则认为转换失败
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 讲一个对象转换为枚举类型
        /// </summary>
        /// <param name="type">要转换的枚举的类型</param>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static object ConvertEnum(Type type, string from)
        {
            //这里设置默认值是0是因为枚举只能是byte，int，所以当转换失败时，返回枚举中枚举值为0的那一项
            return ConvertEnum(type, from, 0);
        }
    }
}
