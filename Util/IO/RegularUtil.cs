using System.Linq;
using System.Text.RegularExpressions;
using Regular = System.Text.RegularExpressions;

namespace Util.IO
{
    /// <summary>
    /// 正则表达式帮助类
    /// <para>包含了一些简单的正则以及正则比对的方法</para>
    /// </summary>
    public class RegularUtil
    {
        /// <summary>
        /// 匹配邮箱地址的正则表达式
        /// </summary>
        public const string EMAIL = @"\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}";

        /// <summary>
        /// 中国大陆手机号的正则表达式
        /// </summary>
        public const string MAINLAND_CELLPHONE = @"1[3-8][0-9]{9}";

        /// <summary>
        /// 中国台湾手机号的正则表达式
        /// </summary>
        public const string TAIWAN_CELLPHONE = @"09[0-9]{8}";

        /// <summary>
        /// 中国香港手机号的正则表达式
        /// </summary>
        public const string HONGKONG_CELLPHONE = @"(5|6|8|9)[0-9]{7}";

        /// <summary>
        /// 中国澳门手机号的正则表达式
        /// </summary>
        public const string MACAO_CELLPHONE = @"6(6|8)[0-9]{5}";

        /// <summary>
        /// IPV4的正则表达式
        /// </summary>
        public const string IPADDRESS = @"(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)";

        /// <summary>
        /// 中国大陆18位身份证的正则表达式
        /// </summary>
        public const string MAINLAND_IDCARD_18 = @"[1-9]\d{5}(18|19|([23]\d))\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]";

        /// <summary>
        /// 中国大陆15位身份证的正则表达式
        /// </summary>
        public const string MAINLAND_IDCARD_15 = @"[1-9]\d{5}\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}";

        /// <summary>
        /// 网址的正则表达式
        /// </summary>
        public const string URL = @"((https|http|ftp|rtsp|mms)?:\/\/)[^\s]+";

        /// <summary>
        /// 中文字符
        /// </summary>
        public const string CHINESE_CHARACTER = @"[\u4e00-\u9fa5]+";

        /// <summary>
        /// 判断传入的字符串是否符合正则表达式的规则
        /// </summary>
        /// <param name="fromString">传入的字符串</param>
        /// <param name="regexExpression">正则表达式</param>
        /// <returns></returns>
        public static bool Regex(string fromString, string regexExpression)
        {
            return Regular.Regex.IsMatch(fromString, regexExpression);
        }

        /// <summary>
        /// 获取一段文本中符合正则表达式规则的第一串字符串
        /// </summary>
        /// <param name="fromString">要取值的文本</param>
        /// <param name="regexExpression">正则表达式</param>
        /// <returns></returns>
        public static string Get(string fromString, string regexExpression)
        {
            return GetAll(fromString, regexExpression).FirstOrDefault().Value;
        }

        /// <summary>
        /// 获取一段文本中符合正则表达式规则的所有字符串
        /// </summary>
        /// <param name="fromString">要取值的文本</param>
        /// <param name="regexExpression">正则表达式</param>
        /// <returns></returns>
        public static Match[] GetAll(string fromString, string regexExpression)
        {
            return Regular.Regex.Matches(fromString, regexExpression).Cast<Match>().ToArray();
        }

        /// <summary>
        /// 判断传入的字符串是否是邮箱
        /// </summary>
        /// <param name="fromString">要判断的文本</param>
        /// <returns></returns>
        public static bool RegexEmail(string fromString)
        {
            return Regex(fromString, EMAIL);
        }

        /// <summary>
        /// 判断传入的字符串是否是手机号
        /// <para>支持港澳台手机号判断</para>
        /// </summary>
        /// <param name="fromString">要判断的文本</param>
        /// <returns></returns>
        public static bool RegexCellPhone(string fromString)
        {
            string cellPhoneExpression = string.Format(@"^{0}|{1}|{2}|{3}$", MAINLAND_CELLPHONE, TAIWAN_CELLPHONE, HONGKONG_CELLPHONE, MACAO_CELLPHONE);
            return Regex(fromString, cellPhoneExpression);
        }

        /// <summary>
        /// 判断传入的字符串是否是IPv4地址
        /// </summary>
        /// <param name="fromString">要判断的文本</param>
        /// <returns></returns>
        public static bool RegexIPv4(string fromString)
        {
            return Regex(fromString, IPADDRESS);
        }

        /// <summary>
        /// 判断传入的字符串是否是身份证
        /// <para>15位和18位都支持</para>
        /// </summary>
        /// <param name="fromString">要判断的文本</param>
        /// <returns></returns>
        public static bool RegexIDCard(string fromString)
        {
            string idCardExpression = string.Format(@"^{0}|{1}$", MAINLAND_IDCARD_18, MAINLAND_IDCARD_15);
            return Regex(fromString, idCardExpression);
        }

        /// <summary>
        /// 判断传入的字符串是否是有效的网址
        /// </summary>
        /// <param name="fromString">要判断的文本</param>
        /// <returns></returns>
        public static bool RegexURL(string fromString)
        {
            return Regex(fromString, URL);
        }

        /// <summary>
        /// 获取传入的字符串中的邮箱
        /// </summary>
        /// <param name="fromString">要获取的文本</param>
        /// <returns></returns>
        public static string GetEmail(string fromString)
        {
            return Get(fromString, EMAIL);
        }

        /// <summary>
        /// 获取传入的字符串中的手机号
        /// <para>支持港澳台手机号获取</para>
        /// </summary>
        /// <param name="fromString">要获取的文本</param>
        /// <returns></returns>
        public static string GetCellPhone(string fromString)
        {
            string cellPhoneExpression = string.Format(@"{0}|{1}|{2}|{3}", MAINLAND_CELLPHONE, TAIWAN_CELLPHONE, HONGKONG_CELLPHONE, MACAO_CELLPHONE);
            return Get(fromString, cellPhoneExpression);
        }

        /// <summary>
        /// 获取传入的字符串中的IPv4地址
        /// </summary>
        /// <param name="fromString">要获取的文本</param>
        /// <returns></returns>
        public static string GetIPv4(string fromString)
        {
            return Get(fromString, IPADDRESS);
        }

        /// <summary>
        /// 获取传入的字符串中的身份证
        /// <para>15位和18位都支持</para>
        /// </summary>
        /// <param name="fromString">要获取的文本</param>
        /// <returns></returns>
        public static string GetIDCard(string fromString)
        {
            string idCardExpression = string.Format(@"^{0}|{1}$", MAINLAND_IDCARD_18, MAINLAND_IDCARD_15);
            return Get(fromString, idCardExpression);
        }

        /// <summary>
        /// 获取传入的字符串中的网址
        /// </summary>
        /// <param name="fromString">要获取的文本</param>
        /// <returns></returns>
        public static string GetURL(string fromString)
        {
            return Get(fromString, URL);
        }

        /// <summary>
        /// 获取传入的字符串中的中文字符
        /// </summary>
        /// <param name="fromString">要取值的文本</param>
        /// <returns></returns>
        public static string GetChineseCharacter(string fromString)
        {
            return Get(fromString, CHINESE_CHARACTER);
        }
    }
}
