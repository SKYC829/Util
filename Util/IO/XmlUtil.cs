using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Util.DataObject;
using Util.IO.Log;

namespace Util.IO
{
    public class XmlUtil
    {
        /// <summary>
        /// 判断一个Xml节点的有序集合是否为空
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static bool IsEmpty(XmlNodeList nodeList)
        {
            return nodeList == null || nodeList.Count == 0;
        }

        /// <summary>
        /// 根据特性的值来获取Xml节点
        /// </summary>
        /// <param name="xmlNode">要获取子节点的根节点</param>
        /// <param name="nodeAttributeName">子节点上的特性的名称</param>
        /// <param name="nodeAttributeValue">子节点上的特性的值</param>
        /// <returns></returns>
        public static XmlElement GetElement(XmlNode xmlNode, string nodeAttributeName, string nodeAttributeValue)
        {
            //如果根节点为空，就认为获取不到子节点，直接返回空
            if (xmlNode == null)
            {
                return null;
            }
            //获取根节点下第一个不为空且包含nodeAttributeName特性且nodeAttributeName的值与传入的值相等的Xml节点
            XmlElement result = xmlNode.ChildNodes.Cast<XmlElement>().Where(p => p != null && p.GetAttribute(nodeAttributeName) == nodeAttributeValue).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 根据标签获取Xml节点
        /// </summary>
        /// <param name="xmlNode">要获取子节点的根节点</param>
        /// <param name="tag">
        /// 标签
        /// <para>例:</para>
        /// <para>&lt;HTML&gt;</para>
        /// <para>&lt;a&gt;</para>
        /// </param>
        /// <returns></returns>
        public static XmlElement GetElement(XmlNode xmlNode, string tag)
        {
            return GetElement(xmlNode, new string[] { tag }).FirstOrDefault();
        }

        /// <summary>
        /// 根据标签获取Xml节点
        /// </summary>
        /// <param name="xmlNode">要获取子节点的根节点</param>
        /// <param name="tags">
        /// 标签
        /// <para>例:</para>
        /// <para>&lt;HTML&gt;</para>
        /// <para>&lt;a&gt;</para>
        /// </param>
        /// <returns></returns>
        public static XmlElement[] GetElement(XmlNode xmlNode, params string[] tags)
        {
            //初始化返回值
            List<XmlElement> result = new List<XmlElement>();
            //如果根节点不为空才进行查找取值
            if (xmlNode != null)
            {
                //如果要用来查找的标签为空或没传入特定的标签，就获取根节点下所有子节点
                if (tags == null || tags.Length == 0)
                {
                    result.AddRange(xmlNode.ChildNodes.Cast<XmlElement>());
                }
                //否则根据传入的特定标签获取根节点下对应的子节点
                else
                {
                    result.AddRange(xmlNode.ChildNodes.Cast<XmlElement>().Where(p => Array.IndexOf<string>(tags, p.Name) > -1));
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的值
        /// </summary>
        /// <typeparam name="T">要获取的值的类型</typeparam>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static T Get<T>(XmlElement xmlElement, string attributeName)
        {
            //初始化返回值
            T result = default(T);
            try
            {
                //如果要取值的特性的名称不为空，且要取值的Xml节点不为空，就进行取值
                if (!string.IsNullOrEmpty(attributeName) && xmlElement != null)
                {
                    //将Xml节点中的对应的特性的值取出来
                    string attributeValue = xmlElement.GetAttribute(attributeName);
                    //如果取到值了，说明有这个特性
                    if (!string.IsNullOrEmpty(attributeValue))
                    {
                        //对取出来的值进行转换，如果是枚举类型的，就用枚举类型的转换
                        //否则用Convert强制转换
                        //Convert不能强制把string转换为枚举
                        if (typeof(T).IsEnum)
                        {
                            result = (T)Enum.Parse(typeof(T), attributeValue);
                        }
                        else
                        {
                            result = (T)Convert.ChangeType(attributeValue, typeof(T));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的string值
        /// </summary>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static string GetString(XmlElement xmlElement, string attributeName)
        {
            return Get<string>(xmlElement, attributeName);
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的bool值
        /// </summary>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static bool GetBool(XmlElement xmlElement, string attributeName)
        {
            return Get<bool>(xmlElement, attributeName);
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的int值
        /// </summary>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static int GetInt(XmlElement xmlElement, string attributeName)
        {
            return Get<int>(xmlElement, attributeName);
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的decimal值
        /// </summary>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static decimal GetDecimal(XmlElement xmlElement, string attributeName)
        {
            return Get<decimal>(xmlElement, attributeName);
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的double值
        /// </summary>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static double GetDouble(XmlElement xmlElement, string attributeName)
        {
            return Get<double>(xmlElement, attributeName);
        }

        /// <summary>
        /// 获取一个Xml节点上某个特性的float值
        /// </summary>
        /// <param name="xmlElement">要取值的Xml节点</param>
        /// <param name="attributeName">要取值的特性名称</param>
        /// <returns></returns>
        public static float GetFloat(XmlElement xmlElement, string attributeName)
        {
            return Get<float>(xmlElement, attributeName);
        }

        /// <summary>
        /// 设置一个节点上一个特性的值
        /// <para>注意:</para>
        /// <para>此操作只会更改已读到内存里的Xml节点的值，如果想要同步生效到Xml文件中</para>
        /// <para>需要将内存里的Xml文件数据输出出去保存一次</para>
        /// </summary>
        /// <param name="xmlElement">要设置值的节点</param>
        /// <param name="attributeName">要设置值的特性的名称</param>
        /// <param name="attributeValue">要设置的值</param>
        public static void Set(XmlElement xmlElement, string attributeName, object attributeValue)
        {
            xmlElement.SetAttribute(attributeName, StringUtil.GetString(attributeValue));
        }
    }
}
