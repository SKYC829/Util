using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Util.IO.Log;

namespace Util.DataObject
{
    /// <summary>
    /// 对象帮助类
    /// <para>封装了一些对对象的快速操作</para>
    /// <para>注意:</para>
    /// <para>这个帮助类并不能帮你找到对象</para>
    /// </summary>
    public class ObjectUtil
    {
        /// <summary>
        /// 获取一个对象的值，如果传入的对象不是<see cref="JToken"/>则会返回对象本身
        /// </summary>
        /// <param name="from">要取值的对象</param>
        /// <returns></returns>
        public static object Get(object from)
        {
            //初始化返回值
            object result = from;
            //如果对象是JToken类型，则进行取值
            if (from is JToken)
            {
                JToken token = from as JToken;
                if (token == null)
                {
                    result = null;
                }
                //如果对象是JValue则返回JValue的值
                else if (token is JValue)
                {
                    result = (token as JValue).Value;
                }
                //如果对象是JProperty则返回JProperty的值
                else if (token is JProperty)
                {
                    result = (token as JProperty).Value;
                }
                //如果不是JValue也不是JProperty，只是单纯的是JToken，则返回JToken
                else
                {
                    result = token;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取一个对象的值
        /// </summary>
        /// <param name="from">要取值的对象</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static object Get(object from, string fieldName)
        {
            //初始化返回值
            object result = null;
            //如果来源对象不为空就开始取值
            if (from != null)
            {
                //获取来源对象的类型
                Type fromType = from.GetType();
                //如果是值类型或者枚举类型，是不包含key的，无法获取
                if (fromType.IsValueType || fromType.IsEnum)
                {
                    result = null;
                }
                //如果是键值对类型，则用DictionaryUtil获取
                else if (from is IDictionary)
                {
                    result = DictionaryUtil.Get(from as IDictionary, fieldName);
                }
                //如果是数据行，则用RowUtil获取
                else if (from is DataRow)
                {
                    result = RowUtil.Get(from as DataRow, fieldName);
                }
                //如果是Json对象，则直接以集合的形式获取
                else if (from is JObject)
                {
                    result = Get((from as JObject)[fieldName]);
                }
                //如果是一个类
                else if (fromType.IsClass)
                {
                    //反射获取到和要取值的字段名一样的属性
                    PropertyInfo property = fromType.GetProperty(fieldName);
                    //如果属性存在，就获取属性的值
                    if (property != null)
                    {
                        result = property.GetValue(from);
                    }
                    else
                    {
                        //否则获取同名字段
                        FieldInfo field = fromType.GetField(fieldName);
                        //如果字段存在，就获取字段的值
                        if (field != null)
                        {
                            result = field.GetValue(from);
                        }
                    }
                }
                //当到这一步，如果返回结果不是空的话
                if (result != null)
                {
                    //获取到返回结果的类型
                    Type resultType = result.GetType();
                    Type genericType = null;
                    //如果返回结果是泛型，再获取泛型的类型
                    if (resultType.IsGenericType)
                    {
                        genericType = resultType.GetGenericArguments().FirstOrDefault();
                    }
                    //如果返回结果是Json数组或者数组，或者泛型是Json对象，则把返回结果转换为List的形式，并把数组中的Json对象转换为键值对
                    if (result is JArray || result is ArrayList || (genericType != null && (genericType == typeof(JObject) || genericType.FullName == "System.Object")))
                    {
                        IList fromList = result as IList;
                        IList toList = ConvertList(fromList);

                        result = toList;
                    }
                    //如果返回结果就是个Json对象，则把Json对象转换为键值对
                    else if (result is JObject)
                    {
                        result = result.ToDictionary();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取一个对象的<typeparamref name="T"/>值
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="from">要取值的对象</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static T Get<T>(object from, string fieldName)
        {
            object result = Get(from, fieldName);
            return StringUtil.Get<T>(result);
        }

        /// <summary>
        /// 获取一个对象的Int值
        /// </summary>
        /// <param name="from">要取值的对象</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static int GetInt(object from, string fieldName)
        {
            return Get<int>(from, fieldName);
        }

        /// <summary>
        /// 获取一个对象的String值
        /// </summary>
        /// <param name="from">要取值的对象</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static string GetString(object from, string fieldName)
        {
            return Get<string>(from, fieldName);
        }

        /// <summary>
        /// 获取一个对象的Decimal值
        /// </summary>
        /// <param name="from">要取值的对象</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static decimal GetDecimal(object from, string fieldName)
        {
            return Get<decimal>(from, fieldName);
        }

        /// <summary>
        /// 设置一个对象的某个字段的值
        /// </summary>
        /// <param name="from">要设置值的对象</param>
        /// <param name="fieldName">要设置值的字段名</param>
        /// <param name="fieldValue">要设置的值</param>
        public static void Set(object from, string fieldName, object fieldValue)
        {
            //如果来源对象为空，则无法设置值
            if (from == null)
            {
                return;
            }
            //获取到来源对象的类型
            Type fromType = from.GetType();
            //如果来源类型是值类型或枚举类型，也无法设置值
            if (fromType.IsValueType || fromType.IsEnum)
            {
                return;
            }
            //如果来源类型是键值对，直接设置即可
            else if (from is IDictionary)
            {
                (from as IDictionary)[fieldName] = fieldValue;
            }
            //如果来源对象是数据行，则调用RowUtil进行设置
            else if (from is DataRow)
            {
                RowUtil.Set(from as DataRow, fieldName, fieldValue);
            }
            //如果来源对象是Json对象，则将值转换为JValue然后进行设置
            else if (from is JObject)
            {
                (from as JObject)[fieldName] = new JValue(fieldValue);
            }
            //如果来源对象是一个类，则根据类型进行设置值
            else if (fromType.IsClass)
            {
                Set(fromType, from, fieldName, fieldValue);
            }
        }

        /// <summary>
        /// 设置一个对象的某个字段的值
        /// </summary>
        /// <param name="fromType">要设置值的对象的类型</param>
        /// <param name="fromObject">要设置值的对象</param>
        /// <param name="fieldName">要设置值的字段名</param>
        /// <param name="fieldValue">要设置的值</param>
        public static void Set(Type fromType, object fromObject, string fieldName, object fieldValue)
        {
            //获取这个对象类型中同名的属性
            PropertyInfo property = fromType.GetProperty(fieldName);
            //如果属性不为空
            if (property != null)
            {
                //获取到属性的类型
                Type propertyType = property.PropertyType;
                //将要设置的值的类型转换为属性所能接收的类型
                object convertFieldValue = StringUtil.Get(propertyType, fieldValue);
                //给属性赋值
                property.SetValue(fromObject, convertFieldValue);
                return;
            }
            //获取这个对象类型中同名的字段
            FieldInfo field = fromType.GetField(fieldName);
            //如果字段不为空
            if (field != null)
            {
                //获取到字段的类型
                Type fieldType = field.FieldType;
                //将要设置的值的类型转换为字段所能接收的类型
                object convertFieldValue = StringUtil.Get(fieldType, fieldValue);
                //给字段设置值
                field.SetValue(fromObject, convertFieldValue);
            }
        }

        /// <summary>
        /// 将Jobject转换为数组
        /// </summary>
        /// <param name="from">要转换的对象</param>
        /// <returns></returns>
        private static IList ConvertList(IList from)
        {
            //初始化返回值
            IList result = new List<object>();
            //遍历来源数组
            foreach (object item in from)
            {
                //如果是Jobject的话，就把Jobject转换为字典，再添加到列表里
                if (item is JObject)
                {
                    JObject token = item as JObject;
                    result.Add(token.ToDictionary());
                }
                //否则直接添加到列表里
                else
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 判断一个值是否能被写入到一个类型的对象里
        /// </summary>
        /// <param name="fromType">要写入值的对象的类型</param>
        /// <param name="value">要写入的值</param>
        /// <returns></returns>
        private static bool CanWrite(Type fromType, object value)
        {
            //如果来源对象类型不是类，则可以写入
            if (!fromType.IsClass)
            {
                return true;
            }
            //如果来源对象类型是字符串，也可以写入
            if (fromType == typeof(string))
            {
                return true;
            }
            //如果来源对象是空，也可以写入
            if (StringUtil.IsEmpty(value))
            {
                return true;
            }
            //如果来源对象是类，并且要设置的值是Json对象，则可以写入
            if (value is JObject && fromType.IsClass)
            {
                return true;
            }
            return fromType.IsAssignableFrom(value.GetType());
        }

        /// <summary>
        /// 把一个对象转换为另一个对象
        /// <para>注意:</para>
        /// <para>目标对象只允许是class</para>
        /// </summary>
        /// <typeparam name="T">要转换的对象的类型</typeparam>
        /// <param name="toObject">目标对象</param>
        /// <param name="fromObject">要转换的对象</param>
        /// <returns></returns>
        public static T Convert<T>(T toObject, object fromObject) where T : class
        {
            //如果来源对象就是目标对象，直接强制转换
            if (fromObject is T)
            {
                return fromObject as T;
            }
            //如果目标对象是空，则无法转换
            if (toObject == null)
            {
                return null;
            }
            //进行转换
            Convert(toObject, fromObject);
            return toObject;
        }

        /// <summary>
        /// 把一个对象转换为另一个对象
        /// <para>注意:</para>
        /// <para>目标对象<typeparamref name="T"/>必须是class并且可以new</para>
        /// </summary>
        /// <typeparam name="T">要转换的对象的类型</typeparam>
        /// <param name="fromObject">要转换的对象</param>
        /// <returns></returns>
        public static T Convert<T>(object fromObject) where T : class, new()
        {
            //如果来源对象就是目标对象，直接强制转换即可
            if (fromObject is T)
            {
                return fromObject as T;
            }
            //新建目标对象，防止空引用异常
            T toObject = new T();
            //进行转换
            return Convert<T>(toObject: toObject, fromObject: fromObject);
        }

        /// <summary>
        /// 把一个对象写入另一个对象里
        /// </summary>
        /// <param name="toObject">目标对象</param>
        /// <param name="fromObject">来源对象</param>
        /// <returns></returns>
        public static object Convert(object toObject, object fromObject)
        {
            if (toObject == null)
            {
                return null;
            }
            Type toType = toObject.GetType();
            PropertyInfo[] properties = toType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }
                object fieldValue = Get(fromObject, property.Name);
                if (fieldValue == null || fieldValue.GetType() == typeof(DBNull))
                {
                    fieldValue = null;
                }
                try
                {
                    if (CanWrite(property.PropertyType, fieldValue))
                    {
                        property.SetValue(toObject, StringUtil.Get(property.PropertyType, fieldValue));
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.WriteException(ex.ToString());
                }
            }

            FieldInfo[] fields = toType.GetFields();
            foreach (FieldInfo field in fields)
            {
                object fieldValue = Get(fromObject, field.Name);
                try
                {
                    if (CanWrite(field.FieldType, fieldValue))
                    {
                        field.SetValue(toObject, StringUtil.Get(field.FieldType, fieldValue));
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.WriteException(ex.ToString());
                }
            }
            return toObject;
        }
    }
}
