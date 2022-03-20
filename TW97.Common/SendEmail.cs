using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TW97.Common
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    public class SendEmail
    {
        /// <summary>
        /// 验证邮箱正则
        /// </summary>
        private static readonly Regex EmailRegex;

        /// <summary>
        /// 发件人-授权码 字典
        /// </summary>
        private static List<SenderSetting>? _senderDict;

        /// <summary>
        /// 静态构造
        /// </summary>
        static SendEmail()
        {
            EmailRegex = new Regex(@"^[a-zA-Z\d-._]+[@][a-zA-Z\d]+([-][a-zA-Z\d]+)?[.][a-zA-Z]{2,}$");
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="config"></param>
        public SendEmail(IConfiguration config)
        {
            if (_senderDict != null && _senderDict.Any())
            {
                return;
            }

            try
            {
                _senderDict = config.GetSection("TW.Email").Get<List<SenderSetting>>();
            }
            catch
            {
                _senderDict = new List<SenderSetting>();
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="sender">发件人</param>
        /// <param name="addressee">收件人列表</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="isBodyHtml">内容是否为HTML</param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Task<bool> SendAsync(string sender,List<string> addressee,string title,string content,bool isBodyHtml,string? code = null)
        {
            return Task.Run(() =>
            {
                ValidSender(ref sender);
                ValidAddressee(ref addressee);
                ValidTitle(ref title);
                ValidContent(ref content);
                ValidCode(sender, ref code);

                var address = new MailAddress(sender);
                var message = new MailMessage
                {
                    From = address, // 发件人
                    Subject = title, // 标题
                    SubjectEncoding = Encoding.UTF8, // 标题编码格式
                    Body = content, // 内容
                    BodyEncoding = Encoding.UTF8, // 内容编码格式
                    Priority = MailPriority.High, // 优先级
                    IsBodyHtml = isBodyHtml // 正文是否为HTML格式
                };

                // 添加发件人
                addressee.ForEach(w =>
                {
                    message.To.Add(w);
                });

                var smtp = new SmtpClient
                {
                    Host = GetSmtp(sender),
                    Credentials = new NetworkCredential(sender, code)
                };

                try
                {
                    smtp.Send(message);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 验证发件人
        /// </summary>
        /// <param name="sender"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ValidSender(ref string sender)
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            sender = (sender ?? "").Trim();

            if (string.IsNullOrEmpty(sender))
            {
                throw new ArgumentNullException(nameof(sender), "发件人不能为空");
            }

            if (!EmailRegex.IsMatch(sender))
            {
                throw new ArgumentNullException(nameof(sender), "发件人邮箱格式不正确");
            }
        }

        /// <summary>
        /// 验证收件人格式
        /// </summary>
        /// <param name="addressee"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ValidAddressee(ref List<string> addressee)
        {
            if (addressee == null || !addressee.Any())
            {
                throw new ArgumentNullException(nameof(addressee), "收件人不能为空");
            }

            // ReSharper disable once ConstantNullCoalescingCondition
            addressee = addressee.Select(w => (w ?? "").Trim()).ToList();

            if (addressee.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentNullException(nameof(addressee), "收件人不能为空");
            }

            if (addressee.Any(w => !EmailRegex.IsMatch(w)))
            {
                throw new ArgumentNullException(nameof(addressee), "收件人邮箱格式不正确");
            }
        }

        /// <summary>
        /// 验证标题
        /// </summary>
        /// <param name="title"></param>
        private static void ValidTitle(ref string title)
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            title = (title ?? "").Trim();

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title), "标题不能为空");
            }
        }

        /// <summary>
        /// 验证内容
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ValidContent(ref string content)
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            content = (content ?? "").Trim();

            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "内容不能为空");
            }
        }

        /// <summary>
        /// 验证发件人SMTP授权码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ValidCode(string sender, ref string? code)
        {
            var dictCode = "";
            if (_senderDict != null && _senderDict.Any(w => w.Sender == sender))
            {
                // ReSharper disable once ConstantNullCoalescingCondition
                dictCode = _senderDict.FirstOrDefault(w => w.Sender == sender)?.Code.Trim();
            }

            code = (code ?? "").Trim();

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(dictCode))
            {
                throw new ArgumentNullException(nameof(code), "发件人SMTP授权码不能为空");
            }

            code = string.IsNullOrEmpty(code) ? dictCode : code;
        }

        /// <summary>
        /// 获取发件人SMTP
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string GetSmtp(string sender)
        {
            var msp = Regex.Match(sender, @"[@](.+)[.]").Groups[1].ToString().ToLower();
            return msp switch
            {
                "qq" => "smtp.qq.com",
                _ => throw new Exception($"暂不支持用 @{msp} 类型的邮箱发送邮件")
            };
        }

        /// <summary>
        /// 发件人配置对象
        /// </summary>
        internal class SenderSetting
        {
            /// <summary>
            /// 发件人邮箱
            /// </summary>
            public string Sender { get; set; } = "";

            /// <summary>
            /// 授权码
            /// </summary>
            public string Code { get; set; } = "";
        }
    }
}
