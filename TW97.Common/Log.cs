using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace TW97.Common
{
    /// <summary>
    /// 文本日志记录
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 日志文件存放路径
        /// </summary>
        private const string Path = "TW-Logs";

        /// <summary>
        /// 日志锁
        /// </summary>
        private static readonly object InfoLocker = new object();

        /// <summary>
        /// 错误日志锁
        /// </summary>
        private static readonly object ErrorLocker = new object();

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Task InfoAsync(string content)
        {
            return Task.Run(() =>
            {
                var dir = AppContext.BaseDirectory + Path + "\\Info";
                var filePath = dir + $"\\{DateTime.Now:yyyyMMdd}.txt";

                lock (InfoLocker)
                {
                    Directory.CreateDirectory(dir);
                    using var fs = new FileStream(filePath, FileMode.Append);

                    content = $"{DateTime.Now:HH:mm:ss} INFO {content}\r\n";
                    var byteCount = Encoding.UTF8.GetByteCount(content);
                    var buffer = Encoding.UTF8.GetBytes(content, 0, byteCount);

                    fs.Write(buffer);
                }
            });
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Task ErrorAsync(string content)
        {
            return Task.Run(() =>
            {
                var dir = AppContext.BaseDirectory + Path + "\\Error";
                var filePath = dir + $"\\{DateTime.Now:yyyyMMdd}.txt";

                lock (ErrorLocker)
                {
                    Directory.CreateDirectory(dir);
                    using var fs = new FileStream(filePath, FileMode.Append);

                    content = $"{DateTime.Now:HH:mm:ss} Error {content}\r\n";
                    var byteCount = Encoding.UTF8.GetByteCount(content);
                    var buffer = Encoding.UTF8.GetBytes(content, 0, byteCount);

                    fs.Write(buffer);
                }
            });
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Task ErrorAsync(Exception exception)
        {
            return Task.Run(() =>
            {
                var dir = AppContext.BaseDirectory + Path + "\\Error";
                var filePath = dir + $"\\{DateTime.Now:yyyyMMdd}.txt";

                lock (ErrorLocker)
                {
                    Directory.CreateDirectory(dir);
                    using var fs = new FileStream(filePath, FileMode.Append);

                    var content = $"{DateTime.Now:HH:mm:ss} Error {exception.Message} {exception.StackTrace}\r\n";
                    var byteCount = Encoding.UTF8.GetByteCount(content);
                    var buffer = Encoding.UTF8.GetBytes(content, 0, byteCount);

                    fs.Write(buffer);
                }
            });
        }
    }
}
