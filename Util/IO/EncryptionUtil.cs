using System;
using System.Security.Cryptography;
using System.Text;
using Util.DataObject;
using Util.IO.Log;

namespace Util.IO
{
    /// <summary>
    /// 加密帮助类
    /// <para>提供了以下类型的加密、解密方法</para>
    /// <list type="bullet">
    /// <item>AES</item>
    /// <item>DES</item>
    /// <item>
    /// <term>MD5</term>
    /// <description>不支持解密</description>
    /// </item>
    /// </list>
    /// </summary>
    public class EncryptionUtil
    {
        /// <summary>
        /// 计算并生成密钥的数组
        /// </summary>
        /// <param name="fromBytes">原密钥数组</param>
        /// <param name="limitSize">密钥最大长度</param>
        /// <returns></returns>
        private static byte[] GeneralKeyBytes(byte[] fromBytes, int limitSize)
        {
            //如果最大长度小于等于0，则返回原密钥自身
            if (limitSize <= 0)
            {
                return fromBytes;
            }
            //定义一个新数组，用于接收新密钥
            byte[] result = new byte[limitSize];
            //如果来源密钥长度大于密钥最大长度，则对密钥进行裁剪
            if (fromBytes.Length > limitSize)
            {
                Buffer.BlockCopy(fromBytes, 0, result, 0, limitSize);
            }
            else
            {
                Buffer.BlockCopy(fromBytes, 0, result, 0, fromBytes.Length);
            }
            return result;
        }

        /// <summary>
        /// 执行加密、解密操作
        /// </summary>
        /// <param name="transform">加密、解密器</param>
        /// <param name="value">要加密、解密的二进制数组</param>
        /// <returns></returns>
        private static byte[] DoTransform(ICryptoTransform transform, byte[] value)
        {
            byte[] result = null;
            try
            {
                result = transform.TransformFinalBlock(value, 0, value.Length);
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("加密不匹配！");
            }
            finally
            {
                transform.Dispose();
            }
            return result;
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
        public static string EncryptAES(object content, string privateKey, string publicKey)
        {
            //如果要加密的对象是空，就无法加密，直接返回空
            if (StringUtil.IsEmpty(content))
            {
                return string.Empty;
            }
            //如果私钥是空，则使用加密器支持的随机私钥
            if (StringUtil.IsEmpty(privateKey))
            {
                privateKey = string.Empty;
            }
            //如果公钥是空，则使用加密器支持的随机公钥
            if (StringUtil.IsEmpty(publicKey))
            {
                publicKey = string.Empty;
            }
            //定义一个加密管理的实例
            RijndaelManaged managed = new RijndaelManaged();
            //定义加密模式为CBC
            managed.Mode = CipherMode.CBC;
            //定义加密的填充方式为ISO10126
            managed.Padding = PaddingMode.PKCS7;
            //定义密码大小和加密区块大小
            managed.KeySize = 128;
            managed.BlockSize = 128;
            //序列化私钥
            byte[] privateKeyBytes = Encoding.Default.GetBytes(privateKey);
            //计算私钥并设置
            managed.Key = GeneralKeyBytes(privateKeyBytes, 16);
            //序列化公钥
            byte[] publicKeyBytes = Encoding.Default.GetBytes(publicKey);
            //计算公钥并设置
            managed.IV = GeneralKeyBytes(publicKeyBytes, 16);
            //创建一个加密器并进行加密
            byte[] resultBytes = DoTransform(managed.CreateEncryptor(), content.ToBytes());
            managed.Dispose();
            //初始化返回值
            //将加密后的对象转换为字符串
            string result = Convert.ToBase64String(resultBytes);
            return result;
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
        public static T DeEncryptAES<T>(string content, string privateKey, string publicKey)
        {
            T result = default(T);
            //如果要解密的对象是空，就无法解密，直接返回空
            if (StringUtil.IsEmpty(content))
            {
                return result;
            }
            //如果私钥是空，则使用加密器支持的随机私钥
            if (StringUtil.IsEmpty(privateKey))
            {
                privateKey = string.Empty;
            }
            //如果公钥是空，则使用加密器支持的随机公钥
            if (StringUtil.IsEmpty(publicKey))
            {
                publicKey = string.Empty;
            }
            //定义一个加密管理的实例
            RijndaelManaged managed = new RijndaelManaged();
            //解密模式和填充方式必须要和加密时的一致
            //定义解密模式为CBC
            managed.Mode = CipherMode.CBC;
            //定义解密的填充方式为ISO10126
            managed.Padding = PaddingMode.PKCS7;
            //定义密码大小和加密区块大小
            managed.KeySize = 128;
            managed.BlockSize = 128;
            //序列化私钥
            byte[] privateKeyBytes = Encoding.Default.GetBytes(privateKey);
            //计算私钥并设置
            managed.Key = GeneralKeyBytes(privateKeyBytes, 16);
            //序列化公钥
            byte[] publicKeyBytes = Encoding.Default.GetBytes(publicKey);
            //计算公钥并设置
            managed.IV = GeneralKeyBytes(publicKeyBytes, 16);
            //创建解密器
            //得到要解密的密文的二进制数组
            byte[] contentBytes = Convert.FromBase64String(content);
            try
            {
                //定义一个临时变量存放解密后的明文的二进制数组
                //进行解密
                byte[] resultBytes = DoTransform(managed.CreateDecryptor(), contentBytes);
                //将解密后的明文数组反序列化为目标对象
                result = resultBytes.ToObject<T>();
            }
            catch (CryptographicException ce)
            {
                //如果发生解密异常则认为是密钥错误
                LogUtil.WriteException(ce.ToString());
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                //如果发生序列化异常则认为是密文错误
                LogUtil.WriteException(se.ToString());
            }
            finally
            {
                managed.Dispose();
            }
            return result;
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
        public static string EncryptDES(object content, string privateKey, string publicKey)
        {
            //如果要加密的对象是空，就无法加密，直接返回空
            if (StringUtil.IsEmpty(content))
            {
                return string.Empty;
            }
            //如果私钥是空，则使用加密器支持的随机私钥
            if (StringUtil.IsEmpty(privateKey))
            {
                privateKey = string.Empty;
            }
            //如果公钥是空，则使用加密器支持的随机公钥
            if (StringUtil.IsEmpty(publicKey))
            {
                publicKey = string.Empty;
            }
            //定义一个DES加密、解密器
            DESCryptoServiceProvider manager = new DESCryptoServiceProvider();
            manager.Mode = CipherMode.CBC;
            //计算私钥
            byte[] privateKeyBytes = Encoding.Default.GetBytes(privateKey);
            //因为DES只支持8位密钥，所以这里公钥私钥长度都限制为8
            manager.Key = GeneralKeyBytes(privateKeyBytes, 8);
            //计算公钥
            byte[] publicKeyBytes = Encoding.Default.GetBytes(publicKey);
            //因为DES只支持8位密钥，所以这里公钥私钥长度都限制为8
            manager.IV = GeneralKeyBytes(publicKeyBytes, 8);
            //创建一个加密器并进行加密
            byte[] resultBytes = DoTransform(manager.CreateEncryptor(), content.ToBytes());
            //将加密得到的二进制数组转换为字符串
            string result = Convert.ToBase64String(resultBytes);
            manager.Dispose();
            return result;
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
        public static T DeEncryptDES<T>(string content, string privateKey, string publicKey)
        {
            T result = default(T);
            //如果要解密的对象是空，就无法解密，直接返回空
            if (StringUtil.IsEmpty(content))
            {
                return result;
            }
            //如果私钥是空，则使用加密器支持的随机私钥
            if (StringUtil.IsEmpty(privateKey))
            {
                privateKey = string.Empty;
            }
            //如果公钥是空，则使用加密器支持的随机公钥
            if (StringUtil.IsEmpty(publicKey))
            {
                publicKey = string.Empty;
            }
            //定义一个DES加密、解密器
            DESCryptoServiceProvider manager = new DESCryptoServiceProvider();
            manager.Mode = CipherMode.CBC;
            //计算私钥
            byte[] privateKeyBytes = Encoding.Default.GetBytes(privateKey);
            //因为DES只支持8位密钥，所以这里公钥私钥长度都限制为8
            manager.Key = GeneralKeyBytes(privateKeyBytes, 8);
            //计算公钥
            byte[] publicKeyBytes = Encoding.Default.GetBytes(publicKey);
            //因为DES只支持8位密钥，所以这里公钥私钥长度都限制为8
            manager.IV = GeneralKeyBytes(publicKeyBytes, 8);
            byte[] contentBytes = Convert.FromBase64String(content);
            try
            {
                //定义一个临时变量存放解密后的明文的二进制数组
                //进行解密
                byte[] resultBytes = DoTransform(manager.CreateDecryptor(), contentBytes);
                //将解密后的明文数组反序列化为目标对象
                result = resultBytes.ToObject<T>();
            }
            catch (CryptographicException ce)
            {
                //如果发生解密异常则认为是密钥错误
                LogUtil.WriteException(ce.ToString());
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                //如果发生序列化异常则认为是密文错误
                LogUtil.WriteException(se.ToString());
            }
            finally
            {
                manager.Dispose();
            }
            return result;
        }

        /// <summary>
        /// MD5加密
        /// <para>将一个对象用MD5的方式进行加密</para>
        /// </summary>
        /// <param name="content">要加密的对象</param>
        /// <returns></returns>
        public static string EncryptMD5(object content)
        {
            //如果要加密的对象是空，就无法加密，直接返回空
            if (StringUtil.IsEmpty(content))
            {
                return string.Empty;
            }
            //定义一个MD5加密器
            MD5CryptoServiceProvider manager = new MD5CryptoServiceProvider();
            //初始化返回值
            string result = string.Empty;
            try
            {
                //进行加密
                byte[] resultBytes = manager.ComputeHash(content.ToBytes());
                //将加密后的字节转为二进制
                for (int i = 0; i < resultBytes.Length; i++)
                {
                    result += resultBytes[i].ToString("x2");
                }
            }
            catch (Exception ex)
            {
                //如果发生异常则认为加密失败
                LogUtil.WriteException(ex.ToString());
            }
            finally
            {
                manager.Dispose();
            }
            return result;
        }
    }
}
