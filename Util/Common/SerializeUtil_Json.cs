using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Util.IO.Log;

namespace Util.Common
{
    /// <summary>
    /// 序列化帮助类
    /// <para>可以将对象序列化、反序列化为</para>
    /// <list type="table">
    /// <item>
    /// Json
    /// </item>
    /// <item>
    /// 二进制(Binary)
    /// </item>
    /// <item>
    /// XML
    /// </item>
    /// </list>
    /// </summary>
    public partial class SerializeUtil
    {
        /// <summary>
        /// 序列化一个对象为Json字符串
        /// </summary>
        /// <param name="from">要序列化的对象</param>
        /// <returns></returns>
        public static string ToJson(object from)
        {
            return ToJson(from, false);
        }

        /// <summary>
        /// 序列化一个对象为Json字符串
        /// </summary>
        /// <param name="from">要序列化的Json</param>
        /// <param name="formatJson">是否格式化</param>
        /// <returns></returns>
        public static string ToJson(object from,bool formatJson)
        {
            //调用JsonConvert直接序列化对象，如果formatJson为true则会让返回的字符串格式更好看
            return JsonConvert.SerializeObject(from, formatJson ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// 把一个Json字符串反序列化为一个对象
        /// </summary>
        /// <param name="fromJsonStr">要反序列化的字符串</param>
        /// <returns></returns>
        public static object FromJson(string fromJsonStr)
        {
            return FromJson<object>(fromJsonStr);
        }

        /// <summary>
        /// 把一个Json字符串反序列化为一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="fromJsonStr">要反序列化的字符串</param>
        /// <returns></returns>
        public static T FromJson<T>(string fromJsonStr)
        {
            //调用JsonConvert直接反序列化字符串
            return JsonConvert.DeserializeObject<T>(fromJsonStr);
        }

        /// <summary>
        /// 根据键获取Json字符串中对应的值
        /// </summary>
        /// <param name="fromJsonStr">要获取值的Json字符串</param>
        /// <param name="key">要获取的键</param>
        /// <returns></returns>
        public static object GetJsonValue(string fromJsonStr,string key)
        {
            //定义一个js的序列化工具
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            //将字符串解析为键值对的格式
            Dictionary<string, object> jsonDic = serializer.DeserializeObject(fromJsonStr) as Dictionary<string, object>;
            object result = null;
            try
            {
                //如果解析成功并且键值对内包含要获取的键
                if(jsonDic!= null && jsonDic.ContainsKey(key))
                {
                    //取出对应的值
                    result = jsonDic[key];
                }
            }catch(Exception ex)
            {
                //如果发生错误则序列化失败
                LogUtil.WriteException(ex.Message);
            }
            return result;
        }
    }
}
