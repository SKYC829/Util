﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
        /// 将一个对象序列化为XML数据的二进制数组
        /// </summary>
        /// <param name="from">要序列化的对象</param>
        /// <returns></returns>
        public static byte[] ToXml(object from)
        {
            //声明一个序列化器
            XmlSerializer serializer = new XmlSerializer(from.GetType());
            //声明一个内存流，在内存中序列化
            using (MemoryStream result = new MemoryStream())
            {
                //将对象序列化到刚刚声明的内存流result里
                serializer.Serialize(result, from);
                //声明一个临时数组，用于保存结果
                byte[] buffer = new byte[(int)result.Length];
                //将内存流的索引定位到开头
                result.Seek(0, SeekOrigin.Begin); //Seek和Position的效果是一样的，用Seek或Position=0根据个人喜好来即可
                //将内存流的数据读取到临时数组
                result.Read(buffer, 0, buffer.Length);
                //将读取到的数据返回，因为使用了using语句，返回后MemoryStream的内存将会被自动释放
                return buffer;
            }
        }

        /// <summary>
        /// 将一个XML字符串数据反序列化为一个对象
        /// </summary>
        /// <param name="fromString">要反序列化的XML字符串数据</param>
        /// <returns></returns>
        public static object FromXml(string fromString)
        {
            if (string.IsNullOrEmpty(fromString))
            {
                return null;
            }
            byte[] fromBytes = Encoding.Default.GetBytes(fromString);
            return FromXml(fromBytes);
        }

        /// <summary>
        /// 将一个XML二进制数组数据反序列化为一个对象
        /// </summary>
        /// <param name="fromBytes">要反序列化的XML二进制数组数据</param>
        /// <returns></returns>
        public static object FromXml(byte[] fromBytes)
        {
            if(fromBytes == null || fromBytes.Length <= 0)
            {
                return null;
            }
            using (MemoryStream fromStream = new MemoryStream(fromBytes))
            {
                return FromXml(fromStream);
            }
        }

        /// <summary>
        /// 将一个XML数据流反序列化为一个对象
        /// </summary>
        /// <param name="fromStream">要反序列化的XML数据流</param>
        /// <returns></returns>
        public static object FromXml(Stream fromStream)
        {
            return FromXml<object>(fromStream);
        }

        /// <summary>
        /// 将一个XML字符串数据反序列化为一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="fromString">要反序列化的XML字符串数据</param>
        /// <returns></returns>
        public static T FromXml<T>(string fromString)
        {
            if (string.IsNullOrEmpty(fromString))
            {
                return default(T);
            }
            byte[] fromBytes = Encoding.Default.GetBytes(fromString);
            return FromXml<T>(fromBytes);
        }

        /// <summary>
        /// 将一个XML数据流反序列化为一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="fromBytes">要反序列化的XML数据流</param>
        /// <returns></returns>
        public static T FromXml<T>(byte[] fromBytes)
        {
            if(fromBytes == null || fromBytes.Length <= 0)
            {
                return default(T);
            }
            using (MemoryStream fromStream = new MemoryStream(fromBytes))
            {
                return FromXml<T>(fromStream);
            }
        }

        /// <summary>
        /// 将一个XML数据流反序列化为一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="fromStream">要反序列化的XML数据流</param>
        /// <returns></returns>
        public static T FromXml<T>(Stream fromStream)
        {
            //初始化返回值
            T result = default(T);
            //声明一个序列化器
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            try
            {
                //反序列化数据流
                result = (T)serializer.Deserialize(fromStream);
            }catch(Exception ex)
            {
                //如果发生错误则序列化失败
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }
    }
}
