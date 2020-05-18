using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Util.IO.Log;

namespace Util.DataObject
{
    /// <summary>
    /// 字符串帮助类
    /// <para>包含了一些字符串的转换的封装，方便调用</para>
    /// </summary>
    public class StringUtil
    {
        /// <summary>
        /// 比较两个类型
        /// </summary>
        /// <param name="type">要比较的类型</param>
        /// <param name="typeName">要比较的类型的名字</param>
        /// <returns></returns>
        internal static bool CompareType(Type type, string typeName)
        {
            //如果要比较的类型为空，就认为不一致，返回false
            if (type == null)
            {
                return false;
            }
            //否则要比较的类型和类型名称一致，就返回true
            else if (type.ToString() == typeName)
            {
                return true;
            }
            //如果类型名字为System.Object，说明比较到最基础的类了还没有一致的，返回false
            if (type.ToString() == "System.Object")
            {
                return false;
            }
            //否则就对类型的父类进行迭代比较
            else
            {
                return CompareType(type.BaseType, typeName);
            }
        }

        /// <summary>
        /// 比较两个类型
        /// </summary>
        /// <param name="type1">要比较的类型1</param>
        /// <param name="type2">要比较的类型2</param>
        /// <returns></returns>
        internal static bool CompareType(Type type1, Type type2)
        {
            return CompareType(type1, type2.ToString());
        }

        /// <summary>
        /// 比较两个对象是否相等
        /// </summary>
        /// <param name="value1">要比较的对象1</param>
        /// <param name="value2">要比较的对象2</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static bool SafeCompare(object value1, object value2, bool ignoreCase)
        {
            //初始化两个变量用于接收要比较的对象的字符串形式
            string fromValue1 = string.Empty;
            string fromValue2 = string.Empty;
            //将要比较的对象转换为字符串
            if (value1 != null)
            {
                fromValue1 = value1.ToString();
            }
            if (value2 != null)
            {
                fromValue2 = value2.ToString();
            }
            //进行字符串比较
            return SafeCompare(fromValue1, fromValue2, ignoreCase);
        }

        /// <summary>
        /// 比较两个字符串是否相等
        /// </summary>
        /// <param name="value1">要比较的字符串1</param>
        /// <param name="value2">要比较的字符串2</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static bool SafeCompare(string value1, string value2, bool ignoreCase)
        {
            return !string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2) //如果两个字符串都不是空
                && value1.Length == value2.Length //并且两个字符串长度相等
                && (string.Compare(value1, value2, ignoreCase, System.Globalization.CultureInfo.InvariantCulture) == 0); //并且C#默认封装的对比方法返回的值是0（0表示两个对象是一样的）
        }

        /// <summary>
        /// 计算某个时间的时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToTimeStamp(DateTime time)
        {
            return time.ToTimeStamp();
        }

        /// <summary>
        /// 将时间戳转换为时间格式
        /// </summary>
        /// <param name="timeStamp">要转换的时间戳</param>
        /// <returns></returns>
        public static DateTime? TimeStampToDateTime(string timeStamp)
        {
            return timeStamp.TimeStampToDateTime();
        }

        /// <summary>
        /// 判断一个对象是否为空
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool IsEmpty(object from)
        {
            //from不是空且不是string.empty且不是\0，则返回true
            return from == null || string.IsNullOrEmpty(from.ToString()) || string.IsNullOrWhiteSpace(from.ToString());
        }

        /// <summary>
        /// 将一个对象转换为字符串格式
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static string GetString(object from)
        {
            //先进行一步处理，将JToken的值获取出来
            from = ObjectUtil.Get(from);
            //如果处理完后对象是空的，则返回Empty，否则强制转换为字符串
            return from == null ? string.Empty : from.ToString();
        }

        /// <summary>
        /// 将一个对象转换为Bool值，当不能转换时，默认为false
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static bool GetBoolean(object from)
        {
            return GetBoolean(from, false);
        }

        /// <summary>
        /// 将一个对象转换为Bool值
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static bool GetBoolean(object from, bool defaultValue)
        {
            //初始化返回值
            bool result = defaultValue;
            try
            {
                if (!IsEmpty(from))
                {
                    result = (!SafeCompare(from, "0", true) //来源对象不等于0
                               && !SafeCompare(from, "false", true) //且来源对象不等于false
                               && !SafeCompare(from, "f", true) //且来源对象不等于f
                               && !SafeCompare(from, "no", true) //且来源对象不等于no
                               && !SafeCompare(from, "n", true) //且来源对象不等于n
                               && !IsEmpty(from)); //且来源对象不是空
                }
            }
            catch (Exception ex)
            {
                //发生异常则认为无法转换
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 将一个对象转换为Int类型
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static int GetInt(object from)
        {
            //初始化返回值
            int result = 0;
            try
            {
                //如果来源对象不是空就进行转换
                if (!IsEmpty(from))
                {
                    //将对象转换为int类型
                    result = Convert.ToInt32(from);
                }
            }
            catch (Exception ex)
            {
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 将一个对象转换为DateTime类型
        /// <para>这个方法弥补了扩展方法ToDateTime只能在string下使用的不足</para>
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object from)
        {
            //初始化返回值
            DateTime? result = null;
            //如果来源对象已经是DateTime类型了，直接强制转换即可
            if (from is DateTime)
            {
                result = (DateTime)from;
            }
            //如果来源对象是可空的DateTime类型(DateTime?)，也强制转换即可
            else if (from is DateTime?)
            {
                result = (DateTime?)from;
            }
            //如果到这返回值还是空，说明来源对象不是DateTime和DateTime?类型，则需要先转为字符串再进行转换
            if (result == null)
            {
                //将来源对象转换为字符串
                string fromStr = GetString(from);
                //如果转换完后字符串不是空的
                if (!IsEmpty(fromStr))
                {
                    //将字符串转换为DateTime类型
                    result = fromStr.ToDateTime();
                }
            }
            return result;
        }

        /// <summary>
        /// 将一个对象转换为Double类型
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static double GetDouble(object from)
        {
            //初始化返回值
            double result = 0d;
            try
            {
                //如果来源对象不是空，就进行转换
                if (!IsEmpty(from))
                {
                    //将对象转换为Double类型
                    result = Convert.ToDouble(from);
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
        /// 将一个对象转换为Decimal类型
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static decimal GetDecimal(object from)
        {
            //初始化返回值
            decimal result = 0m;
            try
            {
                //如果来源对象不是空就进行转换
                if (!IsEmpty(from))
                {
                    //将对象转换为Decimal形式
                    from = Convert.ToDecimal(from);
                }
            }
            catch (Exception ex)
            {
                //如果发生异常就认为无法转换
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 将一个对象转换为<paramref name="toType"/>的类型
        /// </summary>
        /// <param name="toType">要转换的类型</param>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static object Get(Type toType, object from)
        {
            //初始化返回值
            object result;
            //如果要转换的类型是枚举类型，则调用枚举帮助类进行转换
            if (toType.IsEnum)
            {
                string fromStr = GetString(from);
                result = EnumUtil.ConvertEnum(toType, fromStr);
            }
            //如果转换的类型是字符串类型，则直接调用GetString方法
            else if (CompareType(toType, "System.String"))
            {
                result = GetString(from);
            }
            //如果转换的类型是布尔值类型，则直接调用GetBoolean方法
            else if (CompareType(toType, "System.Boolean"))
            {
                string fromStr = GetString(from);
                result = GetBoolean(fromStr);
            }
            //如果转换的类型是Int类型，则直接调用GetInt方法
            else if (CompareType(toType, "System.Int32"))
            {
                result = GetInt(from);
            }
            //如果转换的类型是Double类型，则直接调用GetDouble方法
            else if (CompareType(toType, "System.Double"))
            {
                result = GetDouble(from);
            }
            //如果转换的类型是Decimal类型，则直接调用GetDecimal方法
            else if (CompareType(toType, "System.Decimal"))
            {
                result = GetDecimal(from);
            }
            //如果转换的类型是可空的DateTime类型，则直接调用GetDateTime方法
            else if (CompareType(toType, "System.Nullable`1[System.DateTime]"))
            {
                return GetDateTime(from);
            }
            //如果转换的类型是DateTime类型
            else if (CompareType(toType, "System.DateTime"))
            {
                //调用GetDateTime方法将对象转换为可空的DateTime类型
                DateTime? fromDataTime = GetDateTime(from);
                //然后判断对象是否可转为DateTime，如果不可以则返回系统支持的最小时间，否则返回转换后的对象
                result = (fromDataTime == null || !fromDataTime.HasValue) ? DateTime.MinValue : fromDataTime.Value;
            }
            //如果转换的类型是Class并且是JObject类型
            else if (toType.IsClass && from is JObject)
            {
                //先将来源对象序列化为Json字符串，然后再反序列化为要转换的类型
                result = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(from), toType);
            }
            //如果都不是上面的类型，则返回对象本身
            else
            {
                result = from;
            }
            return result;
        }

        /// <summary>
        /// 将一个对象转换为<typeparamref name="T"/>的类型
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        public static T Get<T>(object from)
        {
            return (T)Get(typeof(T), from);
        }
    }
}
