using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using TW97.Common.Models;

namespace TW97.Common
{
    /// <summary>
    /// 文本日志记录
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 日志配置
        /// </summary>
        private static LogSettings _settings = new LogSettings();

        /// <summary>
        /// 日志锁
        /// </summary>
        private static readonly object InfoLocker = new object();

        /// <summary>
        /// 错误日志锁
        /// </summary>
        private static readonly object ErrorLocker = new object();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Log(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (_settings.Singleton)
            {
                return;
            }

            _settings = GetSettings(configuration);
            _settings.Singleton = true;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void InfoAsync(string content)
        {
            var dir = AppContext.BaseDirectory + _settings.Path;
            var filePath = dir + $"\\info-{DateTime.Now:yyyyMMdd}.txt";

            lock (InfoLocker)
            {
                Directory.CreateDirectory(dir);
                using var fs = new FileStream(filePath, FileMode.Append);

                content = $"{DateTime.Now:HH:mm:ss}\r\n\t{content}\r\n";
                var byteCount = Encoding.UTF8.GetByteCount(content);
                var buffer = Encoding.UTF8.GetBytes(content, 0, byteCount);

                fs.Write(buffer);
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void ErrorAsync(string content)
        {
            var dir = AppContext.BaseDirectory + _settings.Path;
            var filePath = dir + $"\\error-{DateTime.Now:yyyyMMdd}.txt";

            lock (ErrorLocker)
            {
                Directory.CreateDirectory(dir);
                using var fs = new FileStream(filePath, FileMode.Append);

                content = $"{DateTime.Now:HH:mm:ss}\r\n\t{content}\r\n";
                var byteCount = Encoding.UTF8.GetByteCount(content);
                var buffer = Encoding.UTF8.GetBytes(content, 0, byteCount);

                fs.Write(buffer);
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static void ErrorAsync(Exception exception)
        {
            var dir = AppContext.BaseDirectory + _settings;
            var filePath = dir + $"\\error-{DateTime.Now:yyyyMMdd}.txt";

            lock (ErrorLocker)
            {
                Directory.CreateDirectory(dir);
                using var fs = new FileStream(filePath, FileMode.Append);

                var content = $"{DateTime.Now:HH:mm:ss}\r\n\t{exception.Message}\r\n\t{exception.StackTrace}\r\n";
                var byteCount = Encoding.UTF8.GetByteCount(content);
                var buffer = Encoding.UTF8.GetBytes(content, 0, byteCount);

                fs.Write(buffer);
            }
        }

        /// <summary>
        /// 获取日志配置
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static LogSettings GetSettings(IConfiguration configuration)
        {
            try
            {
                var settings = configuration.GetSection($"{Settings.Section}:{Settings.LogSection}").Get<LogSettings>();
                if (string.IsNullOrEmpty(settings.Path))
                {
                    settings.Path = "logs";
                }

                return settings;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return new LogSettings();
            }
        }
    }
}
