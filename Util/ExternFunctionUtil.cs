using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Util.Common;
using Util.DataObject;
using Util.IO;
using Util.IO.Log;

namespace Util
{
    /// <summary>
    /// 扩展方法帮助类
    /// <para>定义了一些扩展方法方便调用</para>
    /// </summary>
    public static class ExternFunctionUtil
    {
        /// <summary>
        /// 将一个对象转换为键值对
        /// </summary>
        /// <param name="from">要转换的物体</param>
        /// <returns></returns>
        public static IDictionary ToDictionary(this object from)
        {
            //初始化返回值
            IDictionary result;
            //如果来源物体已经是键值对，就直接强制转换并返回
            if (from is Dictionary<string, object>)
            {
                result = from as Dictionary<string, object>;
            }
            else
            {
                //否则先尝试序列化为Json
                string tempJson = JsonConvert.SerializeObject(from);
                //然后再反序列化为键值对
                result = JsonConvert.DeserializeObject<Dictionary<string, object>>(tempJson);
            }

            //如果这时返回值还是空，则判断来源类型是不是JObject
            if (result == null && from is JObject)
            {
                //重新初始化返回值
                result = new Dictionary<string, object>();
                //将来源物体强制转换为JObject
                JObject jsObject = from as JObject;
                //遍历JObject
                foreach (KeyValuePair<string, JToken> item in jsObject)
                {
                    //获取Json的key
                    string key = item.Key;
                    //获取Json的value
                    //如果Json的value不是JValue类型，则保持为空，否则添加至返回值内
                    if (item.Value is JValue value)
                    {
                        result[key] = value.Value;
                    }
                    else
                    {
                        result[key] = null;
                    }
                }
            }
            //如果来源类型JObject，且返回值还是空，则认为无法转换，返回一个新的键值对
            else if (result == null)
            {
                result = new Dictionary<string, object>();
            }
            return result;
        }

        /// <summary>
        /// 将一个DataRow转换为键值对
        /// </summary>
        /// <param name="from">要转换的DataRow</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this DataRow from)
        {
            //初始化返回值
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (from != null)
            {
                //遍历数据行所属的表的所有字段
                foreach (DataColumn dataColumn in from.Table.Columns)
                {
                    //将字段和值添加到键值对中
                    result[dataColumn.ColumnName] = from[dataColumn];
                }
            }
            return result;
        }

        /// <summary>
        /// 将一个DataTable转换为键值对
        /// </summary>
        /// <param name="from">要转换的DataTable</param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDictionary(this DataTable from)
        {
            return ToDictionary(from.Rows.Cast<DataRow>().ToArray());
        }

        /// <summary>
        /// 将一个DataRow数组转换为键值对
        /// </summary>
        /// <param name="froms">要转换的DataRow数组</param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDictionary(this IEnumerable<DataRow> froms)
        {
            //初始化返回值
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (froms != null)
            {
                //遍历数据表所有的行
                foreach (DataRow item in froms)
                {
                    //将数据行转换并添加到返回值中
                    result.Add(item.ToDictionary());
                }
            }
            return result;
        }

        /// <summary>
        /// 将时间转换为时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToTimeStamp(this DateTime time)
        {
            //时间戳计算方法是当前时间减掉1970年1月1日0时0分0秒0毫秒的值的总秒数
            TimeSpan span = time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string result = span.TotalSeconds.ToString();
            return result;
        }

        /// <summary>
        /// 将时间戳转换为时间格式
        /// </summary>
        /// <param name="timeStamp">要转换的时间戳</param>
        /// <returns></returns>
        public static DateTime? TimeStampToDateTime(this string timeStamp)
        {
            //初始化返回值
            DateTime? result = null;
            try
            {
                if (!string.IsNullOrEmpty(timeStamp))
                {
                    //计算当前时区的1970年1月1日0时0分0秒0毫秒的标准时间
                    DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
                    //尝试将字符串格式的时间戳转换为long类型
                    if (long.TryParse(timeStamp, out long timeStampValue))
                    {
                        //时间戳计算是当前时间减掉1970年1月1日0时0分0秒0毫秒的值的总秒数，反过来要把时间戳转换为时间则是要让1970年1月1日0时0分0秒0毫秒加上1970年1月1日0时0分0秒0毫秒至时间戳的总秒数
                        result = startTime.AddSeconds(timeStampValue);
                    }
                }
            }
            catch (Exception ex)
            {
                //发生异常则认为不是时间戳格式，无法转换
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 将字符串转换为日期时间格式
        /// </summary>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string timeStr)
        {
            //初始化返回值
            DateTime? result = null;
            try
            {
                if (!string.IsNullOrEmpty(timeStr))
                {
                    //尝试将字符串转换为日期时间格式
                    if (DateTime.TryParse(timeStr, out DateTime dateTimeValue))
                    {
                        result = dateTimeValue;
                    }
                }
            }
            catch (Exception ex)
            {
                //发生异常则认为不是日期时间格式，无法转换
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 给数据表添加字段
        /// </summary>
        /// <param name="from">要添加字段的数据表</param>
        /// <param name="columns">要添加的字段的合集</param>
        public static void AddColumn(this DataTable from, params string[] columns)
        {
            //如果来源数据表为空或要添加的字段集合为空则直接返回
            if (from == null || columns == null)
            {
                return;
            }
            //遍历所有要添加的字段
            foreach (string columnName in columns)
            {
                //如果数据表中未包含同名的列，则把字段添加进去
                if (!from.Columns.Contains(columnName))
                {
                    from.Columns.Add(columnName);
                }
            }
        }

        /// <summary>
        /// 通过反射把一个对象的属性和字段添加到数据表中
        /// </summary>
        /// <param name="from">要添加字段的数据表</param>
        /// <param name="fromType">要反射添加字段的对象类型</param>
        public static void AddColumn(this DataTable from, Type fromType)
        {
            //反射遍历目标类型的所有属性
            foreach (PropertyInfo property in fromType.GetProperties())
            {
                //如果该属性能够读写，则添加到数据表中
                if (property.CanRead && property.CanWrite)
                {
                    from.AddColumn(property.Name, property.PropertyType);
                }
            }
            //反射遍历目标类型的所有字段
            foreach (FieldInfo field in fromType.GetFields())
            {
                //将字段添加到数据表中
                from.AddColumn(field.Name, field.FieldType);
            }
        }

        /// <summary>
        /// 给数据表添加字段
        /// </summary>
        /// <param name="from">要添加字段的数据表</param>
        /// <param name="columnName">要添加的字段</param>
        /// <param name="columnType">要添加的字段的类型</param>
        public static void AddColumn(this DataTable from, string columnName, Type columnType)
        {
            //如果数据表中已经有这个字段了，就直接返回
            if (from.Columns.Contains(columnName))
            {
                return;
            }
            //如果字段类型是可空的，就按默认的方式添加字段
            if (columnType.FullName.IndexOf("System.Nullable`1") == 0)
            {
                from.AddColumn(columnName);
            }
            else
            {
                //否则就添加字段并设置为指定类型
                from.Columns.Add(columnName, columnType);
            }
        }

        /// <summary>
        /// 复制一个数据表，并且复制原数据表的RowFilter
        /// </summary>
        /// <param name="from">要复制的数据表</param>
        /// <returns></returns>
        public static DataTable Clone(this DataTable from)
        {
            DataTable result = from.Copy();
            result.DefaultView.RowFilter = from.DefaultView.RowFilter;
            return result;
        }

        /// <summary>
        /// 复制一个数据行
        /// <para>注意:</para>
        /// <para>该方法所复制的数据行仍属于原数据表，</para>
        /// <para>如果想要将一个数据表的某个数据行复制到一张新数据表的数据行，</para>
        /// <para>请使用<see cref="CopyTo(DataRow, DataRow)"/>方法</para>
        /// </summary>
        /// <param name="dataRow">来源数据行</param>
        /// <returns></returns>
        public static DataRow Copy(this DataRow dataRow)
        {
            //获取原数据行的数据表
            DataTable fromTable = dataRow.Table;
            //创建一个属于原数据表的新数据行
            DataRow result = fromTable.NewRow();
            //将数据行的所有项进行复制
            result.ItemArray = dataRow.ItemArray;
            return result;
        }

        /// <summary>
        /// 将一个数据行复制到一个新的数据行
        /// </summary>
        /// <param name="fromDataRow">来源数据行</param>
        /// <param name="toDataRow">要复制的数据行</param>
        public static void CopyTo(this DataRow fromDataRow, DataRow toDataRow)
        {
            //得到来源数据表
            DataTable fromTable = fromDataRow.Table;
            //得到目标数据表
            DataTable toTable = toDataRow.Table;
            //遍历来源数据表的所有列
            foreach (DataColumn column in fromTable.Columns)
            {
                //给目标数据表增加相应的列
                toTable.AddColumn(column.ColumnName, column.DataType);
                //取出刚刚增加的那一列
                DataColumn toColumn = toTable.Columns[column.ColumnName];
                //对目标数据行进行赋值
                toDataRow[toColumn] = RowUtil.Get(fromDataRow, toColumn.DataType, toColumn.ColumnName);
            }
        }

        /// <summary>
        /// 将一个对象转换成Byte数组（二进制格式）
        /// </summary>
        /// <param name="fromObject"></param>
        /// <returns></returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException"/>
        public static byte[] ToBytes(this object fromObject)
        {
            return SerializeUtil.ToBinary(fromObject);
        }

        /// <summary>
        /// 将一个byte数组转换成一个对象（二进制格式）
        /// </summary>
        /// <typeparam name="T">要转换的对象的类型</typeparam>
        /// <param name="fromBytes">要转换的数组</param>
        /// <returns></returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException"/>
        public static T ToObject<T>(this byte[] fromBytes)
        {
            return SerializeUtil.FromBinary<T>(fromBytes);
        }

        /// <summary>
        /// AES加密
        /// <para>将一个对象用AES CBC的方式根据privateKey和publicKey进行加密</para>
        /// <para>公钥和私钥只能是可以通过Encoding.GetBytes转换的字符串，否则密钥验证会失效</para>
        /// </summary>
        /// <param name="content">要加密的对象</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string EncryptAES(this object content, string privateKey = "", string publicKey = "")
        {
            return EncryptionUtil.EncryptAES(content, privateKey, publicKey);
        }

        /// <summary>
        /// AES解密
        /// <para>将一段AES密文通过CBC的方式根据privateKey和publicKey进行解密</para>
        /// <para>公钥和私钥只能是可以通过Encoding.GetBytes转换的字符串，否则密钥验证会失效</para>
        /// </summary>
        /// <typeparam name="T">要将密文解析成的类型</typeparam>
        /// <param name="content">密文</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static T DeEncryptAES<T>(this string content, string privateKey = "", string publicKey = "")
        {
            return EncryptionUtil.DeEncryptAES<T>(content, privateKey, publicKey);
        }

        /// <summary>
        /// DES加密
        /// <para>将一个对象用DES CBC的方式根据privateKey和publicKey进行加密</para>
        /// <para>公钥和私钥只能是可以通过Encoding.GetBytes转换的字符串，否则密钥验证会失效</para>
        /// </summary>
        /// <param name="content">要加密的对象</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string EncryptDES(this object content, string privateKey, string publicKey)
        {
            return EncryptionUtil.EncryptDES(content, privateKey, publicKey);
        }

        /// <summary>
        /// DES解密
        /// <para>将一段DES密文通过CBC的方式根据privateKey和publicKey进行解密</para>
        /// <para>公钥和私钥只能是可以通过Encoding.GetBytes转换的字符串，否则密钥验证会失效</para>
        /// </summary>
        /// <typeparam name="T">要将密文解析成的类型</typeparam>
        /// <param name="content">密文</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static T DeEncryptDES<T>(this string content, string privateKey, string publicKey)
        {
            return EncryptionUtil.DeEncryptDES<T>(content, privateKey, publicKey);
        }

        /// <summary>
        /// MD5加密
        /// <para>将一个对象用MD5的方式进行加密</para>
        /// </summary>
        /// <param name="content">要加密的对象</param>
        /// <returns></returns>
        public static string EncryptMD5(this object content)
        {
            return EncryptionUtil.EncryptMD5(content);
        }

        /// <summary>
        /// 压缩字节流数据
        /// <para>注意:</para>
        /// <para>要压缩的字节流数据如果过小则有可能会起到反效果</para>
        /// </summary>
        /// <param name="fromBytes">要压缩的字节流数据</param>
        /// <returns></returns>
        public static byte[] Compress(this byte[] fromBytes)
        {
            //初始化返回值
            byte[] result = fromBytes;
            try
            {
                //声明一个内存流，在内存中压缩
                using (MemoryStream resultStream = new MemoryStream())
                {
                    //声明一个GZip输出压缩流
                    using (GZipOutputStream gZip = new GZipOutputStream(resultStream))
                    {
                        //设置压缩等级为6级（等级越高效果越好）
                        gZip.SetLevel(6);
                        //将压缩好的数据输出到内存流中
                        gZip.Write(fromBytes, 0, fromBytes.Length);
                    }
                    result = resultStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                //如果发生异常就认为压缩失败，返回原字节流数据
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 解压缩字节流数据
        /// </summary>
        /// <param name="fromBytes">要解压缩的字节流数据</param>
        /// <returns></returns>
        public static byte[] DeCompress(this byte[] fromBytes)
        {
            //初始化返回值
            byte[] result = fromBytes;
            try
            {
                //声明一个内存流，将字节流数据放到内存中处理
                using (MemoryStream fromStream = new MemoryStream(fromBytes))
                {
                    //声明一个GZip压缩输入流
                    using (GZipInputStream gZip = new GZipInputStream(fromStream))
                    {
                        //声明一个新的内存流，用来存放解压缩后的数据
                        using (MemoryStream resultStream = new MemoryStream())
                        {
                            //定义每次读取的数据包
                            byte[] zipDataBytes = new byte[1024];
                            //定义一个变量用来存储每次实际读取到的数据的数量
                            int count;
                            //如果读取到的数据的数量不是0，就将读取到的数据写入到存放压缩后的数据的内存流中
                            while ((count = gZip.Read(zipDataBytes, 0, zipDataBytes.Length)) != 0)
                            {
                                //写入数据到存放压缩后的数据的内存流中
                                resultStream.Write(zipDataBytes, 0, count);
                            }
                            result = resultStream.ToArray();
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
        /// 将一个Uri字符串进行转换
        /// <para>例如:</para>
        /// <code>
        /// string url = "~/HelloWorld";
        /// url.ParseUrl() => "X://ProjectPath//HelloWorld"
        /// </code>
        /// </summary>
        /// <param name="from">要转换的字符串</param>
        /// <returns></returns>
        public static string ParseUrl(this string from)
        {
            return from.ParseUrl(false);
        }

        /// <summary>
        /// 将一个Uri字符串进行转换
        /// <para>例如:</para>
        /// <code>
        /// string url = "~/HelloWorld";
        /// url.ParseUrl() => "X://ProjectPath//HelloWorld";
        /// url.ParseUrl(true) => "http://localhost:80//ProjectPath//HelloWorld"
        /// </code>
        /// </summary>
        /// <param name="from">要转换的字符串</param>
        /// <param name="isHttpUrl">是否要转换为Http的格式</param>
        /// <returns></returns>
        public static string ParseUrl(this string from, bool isHttpUrl)
        {
            //如果传入的字符串已~或.开头，对字符串开头进行裁剪
            if (from.StartsWith("~") || from.StartsWith("."))
            {
                from = from.TrimStart(new char[]
                {
                    '\\',
                    '/',
                    '~',
                    '.'
                });
                //如果是http格式，返回http的物理路径/传入的字符串
                if (isHttpUrl)
                {
                    from = Path.Combine(HttpRuntime.AppDomainAppPath, from);
                }
                //否则返回应用程序的物理路径/传入的字符串
                else
                {
                    from = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, from);
                }
            }
            //将网络路径的标志'/'转换为Windows特识别的本地路径的'\'标志
            from = from.Replace('/', '\\');
            //如果获取不到':'的位置，说明传入的字符串没有做开头裁剪处理，即没有盘符或http的物理路径
            if (from.IndexOf(':') == -1)
            {
                //那么就默认以本地路径处理
                from = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, from);
            }
            return from;
        }

        /// <summary>
        /// 判断文件是否被占用
        /// </summary>
        /// <param name="fileInfo">要判断的文件</param>
        /// <returns></returns>
        public static bool IsOccupied(this FileInfo fileInfo)
        {
            return FileUtil.IsFileOccupied(fileInfo);
        }

        /// <summary>
        /// 将一个文件夹复制到一个新的路径
        /// </summary>
        /// <param name="fromDirectory">要复制的文件夹</param>
        /// <param name="toPath">文件夹的新路径</param>
        public static void CopyTo(this DirectoryInfo fromDirectory, string toPath)
        {
            fromDirectory.CopyTo(toPath, true);
        }

        /// <summary>
        /// 将一个文件夹复制到一个新的路径
        /// </summary>
        /// <param name="fromDirectory">要复制的文件夹</param>
        /// <param name="toPath">文件夹的新路径</param>
        /// <param name="isRecursive">是否复制该文件夹下的子文件夹</param>
        public static void CopyTo(this DirectoryInfo fromDirectory, string toPath, bool isRecursive)
        {
            //如果要复制的文件夹不存在，就取消操作
            if (fromDirectory == null || !fromDirectory.Exists)
            {
                return;
            }
            //如果目标路径不存在，就创建目标路径
            if (!Directory.Exists(toPath))
            {
                Directory.CreateDirectory(toPath);
            }
            //获取该文件夹下所有文件，复制到目标文件夹下
            FileInfo[] fileInfos = fromDirectory.GetFiles();
            foreach (FileInfo file in fileInfos)
            {
                try
                {
                    if (!file.IsOccupied())
                    {
                        file.CopyTo(Path.Combine(toPath, file.Name), true);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.WriteException(ex.ToString());
                }
            }
            //如果要复制该文件夹下的子文件夹
            if (isRecursive)
            {
                //获取该文件夹下所有子文件夹
                DirectoryInfo[] subDirectorys = fromDirectory.GetDirectories();
                //重新递归调用此方法复制
                foreach (DirectoryInfo subDirectory in subDirectorys)
                {
                    subDirectory.CopyTo(Path.Combine(toPath, subDirectory.Name), isRecursive);
                }
            }
        }

        /// <summary>
        /// 根据键值对的值进行排序
        /// </summary>
        /// <typeparam name="T1">键的类型</typeparam>
        /// <typeparam name="T2">值的类型</typeparam>
        /// <param name="fromDictionary">要排序的键值对</param>
        /// <returns></returns>
        public static IList<KeyValuePair<T1, T2>> Sort<T1, T2>(this IDictionary<T1, T2> fromDictionary) where T2 : struct
        {
            List<KeyValuePair<T1, T2>> result = new List<KeyValuePair<T1, T2>>();
            if (fromDictionary != null)
            {
                result.AddRange(fromDictionary);
                try
                {
                    result.Sort(delegate (KeyValuePair<T1, T2> previewItem, KeyValuePair<T1, T2> nextItem)
                    {
                        dynamic value1 = previewItem;
                        dynamic value2 = nextItem;
                        return value2 - value1;
                    });
                }
                catch (Exception ex)
                {
                    LogUtil.WriteException(ex.ToString());
                }
            }
            return result;
        }
    }
}
