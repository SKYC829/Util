using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Util.Common;
using Util.DataObject;
using Util.IO;
using Util.IO.Log;

namespace Util.Net
{
    public class NetUtil
    {
        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            //定义一个TCP客户端
            TcpClient tcpClient = new TcpClient();
            //初始化返回值
            string result = string.Empty;
            try
            {
                //用TCP客户端连接百度的80端口
                tcpClient.Connect("https://www.baidu.com", 80);
                //连接成功之后TCP客户端的LocalEndPoint的地址就是我们的IP地址
                result = ((IPEndPoint)tcpClient.Client.LocalEndPoint).Address.ToString();
                //关闭TCP客户端
                tcpClient.Close();
            }
            catch (SocketException)
            {
                //如果发生了Socket异常，就认为百度崩了或DNS解析有问题或网络异常
                //改用安全的方式获取本机IP
                result = GetIPSafe();
            }
            catch (Exception ex)
            {
                LogUtil.WriteLog(ex);
            }
            return result;
        }

        /// <summary>
        /// 安全的方式获取本机IP
        /// <para>不一定准确，但肯定没报错</para>
        /// </summary>
        /// <returns></returns>
        public static string GetIPSafe()
        {
            //初始化返回值
            string result = string.Empty;
            //通过Dns来查找本机环境中地址类型为InterNetwork的IP
            IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(p => p.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
            result = ip.ToString();
            return result;
        }

        /// <summary>
        /// 获取本机公网地址
        /// </summary>
        /// <returns></returns>
        public static string GetNetIP()
        {
            //初始化返回值
            string result = string.Empty;
            //创建一个Web请求
            WebRequest request = WebRequest.Create("http://pv.sohu.com/cityjson?ie=utf-8");
            //定义一个流读取器
            StreamReader reader = null;
            try
            {
                //读取Web请求的返回值
                reader = new StreamReader(request.GetResponse().GetResponseStream());
                //得到读取出来的网页内容
                string htmlContent = reader.ReadToEnd();
                //利用正则得到IP地址
                result = RegularUtil.GetIPv4(htmlContent);
            }
            catch (Exception ex)
            {
                //如果发生异常就输出到日志
                LogUtil.WriteLog(ex);
            }
            finally
            {
                //最后不管怎样只要流读取器不为空就关闭流读取器，释放资源
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 获取与百度之间的网络延迟
        /// </summary>
        /// <returns></returns>
        public static int Ping()
        {
            return Ping("https://www.baidu.com");
        }

        /// <summary>
        /// 获取与一个域名或IP之间的网络延迟
        /// <para>超时时间为30秒</para>
        /// </summary>
        /// <param name="host">要获取延迟的域名或IP</param>
        /// <returns></returns>
        public static int Ping(string host)
        {
            //初始化返回值
            int result = 0;
            //计算与域名或IP之间的网络延迟，超时为30秒
            PingReply pingReply = Ping(host, 30);
            //如果没有超时没有发生异常
            if (pingReply != null)
            {
                //就获取延迟
                result = StringUtil.GetInt(pingReply.RoundtripTime);
            }
            return result;
        }

        /// <summary>
        /// 获取与一个域名或IP之间的网络延迟
        /// </summary>
        /// <param name="host">要获取延迟的域名或IP</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public static PingReply Ping(string host, int timeOut)
        {
            //初始化返回值
            PingReply result = null;
            try
            {
                //计算与域名或IP之间的网络延迟
                result = new Ping().Send(host, timeOut);
            }
            catch (Exception ex)
            {
                //如果发生异常就输出到日志
                LogUtil.WriteLog(ex);
            }
            return result;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="receiverAddress">收件人邮箱</param>
        /// <param name="senderAddress">发件人邮箱</param>
        /// <param name="senderPasscode">发件人密码</param>
        /// <param name="emailBody">邮件内容</param>
        /// <param name="emailTitle">邮件标题</param>
        public static void SendEmail(string receiverAddress, string senderAddress, string senderPasscode, string emailBody, string emailTitle)
        {
            SendEmail(receiverAddress, senderAddress, senderPasscode, emailBody, emailTitle, null);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="receiverAddress">收件人邮箱</param>
        /// <param name="senderAddress">发件人邮箱</param>
        /// <param name="senderPasscode">发件人密码</param>
        /// <param name="emailBody">邮件内容/param>
        /// <param name="emailTitle">邮件标题</param>
        /// <param name="emailAttachments">邮件附件</param>
        public static void SendEmail(string receiverAddress, string senderAddress, string senderPasscode, string emailBody, string emailTitle, params FileInfo[] emailAttachments)
        {
            SendEmail(receiverAddress, senderAddress, senderPasscode, emailBody, emailTitle, true, emailAttachments);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="receiverAddress">收件人邮箱</param>
        /// <param name="senderAddress">发件人邮箱</param>
        /// <param name="senderPasscode">发件人密码</param>
        /// <param name="emailBody">邮件内容</param>
        /// <param name="emailTitle">邮件标题</param>
        /// <param name="enableSsl">是否启用SSL</param>
        /// <param name="emailAttachments">邮件附件</param>
        public static void SendEmail(string receiverAddress, string senderAddress, string senderPasscode, string emailBody, string emailTitle, bool enableSsl, params FileInfo[] emailAttachments)
        {
            SendEmail(new List<string>() { receiverAddress }, senderAddress, senderPasscode, emailBody, emailTitle, enableSsl, emailAttachments);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="receivers">收件人邮箱列表</param>
        /// <param name="senderAddress">发件人邮箱</param>
        /// <param name="senderPasscode">发件人密码</param>
        /// <param name="emailBody">邮件内容</param>
        /// <param name="emailTitle">邮件标题</param>
        /// <param name="enableSsl">是否启用SSL</param>
        /// <param name="emailAttachments">邮件附件</param>
        public static void SendEmail(List<string> receivers, string senderAddress, string senderPasscode, string emailBody, string emailTitle, bool enableSsl, params FileInfo[] emailAttachments)
        {
            if (string.IsNullOrEmpty(senderAddress) || !RegularUtil.RegexEmail(senderAddress))
            {
                throw new ArgumentException("请填写有效的发件人邮箱", "senderAddress");
            }
            //定义一个邮件对象
            MailMessage mailMessage = new MailMessage();
            //循环收件人列表添加收件人
            foreach (string receiveAddress in receivers)
            {
                if (string.IsNullOrEmpty(receiveAddress) || !RegularUtil.RegexEmail(receiveAddress))
                {
                    continue;
                }
                mailMessage.To.Add(receiveAddress);
            }
            if (mailMessage.To.Count <= 0)
            {
                throw new ArgumentException("请填写收件人");
            }
            //添加发件人
            mailMessage.From = new MailAddress(senderAddress, DeviceUtil.GetSystemUserName());
            //设置邮件标题
            mailMessage.Subject = emailTitle;
            //设置邮件内容
            mailMessage.Body = emailBody;
            //默认使用HTML格式的邮件内容
            mailMessage.IsBodyHtml = true;
            //邮件内容编码使用默认编码
            mailMessage.BodyEncoding = Encoding.Default;
            //设置邮件优先级为普通，似乎是过高容易被当成垃圾邮件
            mailMessage.Priority = MailPriority.Normal;
            //如果附件列表不为空且包含附件
            if (emailAttachments != null && emailAttachments.Length > 0)
            {
                //循环附件列表添加附件
                foreach (FileInfo attachmentFile in emailAttachments)
                {
                    //定义一个附件对象并设置附件路径，附件的创建，修改，访问时间
                    Attachment attachment = new Attachment(attachmentFile.FullName);
                    ContentDisposition contentDisposition = attachment.ContentDisposition;
                    contentDisposition.CreationDate = attachmentFile.CreationTime;
                    contentDisposition.ModificationDate = attachmentFile.LastWriteTime;
                    contentDisposition.ReadDate = attachmentFile.LastAccessTime;
                    mailMessage.Attachments.Add(attachment);
                }
            }
            //定义一个Smtp客户端对象
            SmtpClient smtpClient = new SmtpClient();
            //获取第一个收件人的地址
            string firstReciever = receivers.Where(p => p.IndexOf('@') > -1).FirstOrDefault();
            //计算收件人的Smtp协议
            int hostStart = firstReciever.IndexOf('@');
            string smtpHost = string.Format("smtp{0}", firstReciever.Substring(hostStart, firstReciever.Length - hostStart).Replace('@', '.'));
            //设置smtp协议
            smtpClient.Host = smtpHost;
            //设置是否启用SSL
            smtpClient.EnableSsl = enableSsl;
            //设置Smtp客户端使用默认的身份验证方式
            smtpClient.UseDefaultCredentials = true;
            //设置Smtp客户端的用户密码，用于发送邮件
            smtpClient.Credentials = new NetworkCredential(mailMessage.From.Address, senderPasscode);
            //设置客户端发送邮件的方式为网络的方式
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                //开始发送邮件
                smtpClient.SendMailAsync(mailMessage).Wait();
            }
            catch (Exception ex)
            {
                LogUtil.WriteLog(ex);
                //如果错误提示为需要开启SSL并且没有打开SSL
                if (ex.Message == "need EHLO and AUTH first" && !enableSsl)
                {
                    //迭代发送邮件并启用SSL
                    SendEmail(receivers, senderAddress, senderPasscode, emailBody, emailTitle, true, emailAttachments);
                }
                else
                {
                    //否则有可能发件失败了，抛出异常
                    throw ex;
                }
            }
        }
    }
}
