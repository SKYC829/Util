using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.DataObject
{
    /// <summary>
    /// 数据行帮助类
    /// <para>封装了一些数据行的快速操作，如快速获取某一个值或快速设置某个字段的值</para>
    /// </summary>
    public class RowUtil
    {
        /// <summary>
        /// 获取一个数据行中某个字段的值
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static object Get(DataRow dataRow,string fieldName)
        {
            //如果要取值的数据行为空，则返回空
            if(dataRow == null)
            {
                return null;
            }
            //如果要取值的数据行所属的表不包含这个字段，则返回空
            if (!dataRow.Table.Columns.Contains(fieldName))
            {
                return null;
            }
            //如果这个数据行的状态为分离的(即不存在于数据表里)状态，则返回为空
            if(dataRow.RowState == DataRowState.Detached)
            {
                return null;
            }
            return dataRow[fieldName];
        }

        /// <summary>
        /// 获取一个数据行中某个字段的值，并转换为<paramref name="toType"/>的类型
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="toType">要转换的类型</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static object Get(DataRow dataRow,Type toType,string fieldName)
        {
            object fromValue = Get(dataRow, fieldName);
            return StringUtil.Get(toType, fromValue);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的值，并转换为<typeparamref name="T"/>的类型
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static T Get<T>(DataRow dataRow,string fieldName)
        {
            object fromValue = Get(dataRow, fieldName);
            return StringUtil.Get<T>(fromValue);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的bool值
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static bool GetBoolean(DataRow dataRow,string fieldName)
        {
            return Get<bool>(dataRow, fieldName);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的Int值
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static int GetInt(DataRow dataRow,string fieldName)
        {
            return Get<int>(dataRow, fieldName);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的double值
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static double GetDouble(DataRow dataRow,string fieldName)
        {
            return Get<double>(dataRow, fieldName);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的Decimal值
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static decimal GetDecimal(DataRow dataRow,string fieldName)
        {
            return Get<decimal>(dataRow, fieldName);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的String值
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static string GetString(DataRow dataRow,string fieldName)
        {
            return Get<string>(dataRow, fieldName);
        }

        /// <summary>
        /// 获取一个数据行中某个字段的DateTime值，如果无法转为DateTime,则DateTime?会为空
        /// </summary>
        /// <param name="dataRow">要取值的数据行</param>
        /// <param name="fieldName">要取值的字段</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(DataRow dataRow,string fieldName)
        {
            object fromValue = Get(dataRow, fieldName);
            return StringUtil.GetDateTime(fromValue);
        }

        /// <summary>
        /// 给数据行中的某个字段设置值，如果字段不存在则会自动添加字段
        /// </summary>
        /// <param name="dataRow">要设置值的数据行</param>
        /// <param name="fieldName">要设置值的字段</param>
        /// <param name="fieldValue">要设置的值</param>
        public static void Set(DataRow dataRow,string fieldName,object fieldValue)
        {
            //如果来源数据行的数据表没有该字段，则自动新增字段
            if (!dataRow.Table.Columns.Contains(fieldName))
            {
                dataRow.Table.Columns.Add(fieldName);
            }
            //设置字段数据
            dataRow[fieldName] = fieldValue;
        }
    }
}
