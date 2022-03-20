using System;
using Microsoft.Extensions.Configuration;

namespace TW97.Common
{
    /// <summary>
    /// 配置文件读取帮助
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 配置文件对象
        /// </summary>
        private static IConfiguration? _config;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AppSettings(IConfiguration config)
        {
            if (_config != null)
            {
                return;
            }

            _config = config ?? throw new ArgumentNullException(nameof(config), "空指针异常");
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Get<T>(string key)
        {
            if (_config == null) throw new ArgumentNullException(nameof(_config), "空指针异常");
            return _config.GetSection(key).Get<T>();
        }
    }
}