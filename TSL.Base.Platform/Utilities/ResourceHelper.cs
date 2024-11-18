using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using TSL.Base.Platform.Resource;

namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// 資源檔Helper
    /// </summary>
    public class ResourceHelper
    {
        private IOptions<ResourceOption> _option;
        private ResourceManager _resourceMan;

        /// <summary>
        /// 資源檔Helper
        /// </summary>
        /// <param name="configuration">IOptions<ResourceOption></param>
        public ResourceHelper(IOptions<ResourceOption> configuration)
        {
            _option = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ResourcePath = _option.Value.RootPath ?? throw new ArgumentNullException(nameof(_option.Value.RootPath));
        }

        /// <summary>
        /// 資源檔路徑
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// 設定ResourceManager
        /// </summary>
        public void SetResourceManager()
        {
            _resourceMan = new ResourceManager(ResourcePath, typeof(ResourceHelper).Assembly);
        }

        /// <summary>
        /// 取得資源檔的值
        /// </summary>
        /// <param name="name">名稱</param>
        /// <returns>值</returns>
        public virtual string GetValue(string name)
        {
            string message = _resourceMan.GetString(name, CultureInfo.CurrentCulture);
            return message ?? string.Empty;
        }

        /// <summary>
        /// 取得資源檔的值
        /// </summary>
        /// <param name="number">數字</param>
        /// <returns>值</returns>
        public virtual string GetValue(int number)
        {
            string message = _resourceMan.GetString($"{number}", CultureInfo.CurrentCulture);
            return message ?? string.Empty;
        }
    }
}
