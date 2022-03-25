namespace TW97.Common.Models
{
    /// <summary>
    /// 日志配置
    /// </summary>
    internal class LogSettings
    {
        /// <summary>
        /// 日志存放路径
        /// </summary>
        internal string Path { get; set; } = "logs";

        /// <summary>
        /// 单例
        /// </summary>
        internal bool Singleton { get; set; }
    }
}
