
using Microsoft.Extensions.Options;
using static Google.Protobuf.Reflection.FieldOptions.Types;
using TSL.Base.Platform.Resource;
using TSL.Base.Platform.lnversionOfControl;

namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// ResourceHelper
    /// </summary>
    [RegisterIOC(IocType.Transient)]
    public class ApiHandlerResourceHelper : ResourceHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHandlerResourceHelper"/> class.
        /// </summary>
        /// <param name="option">
        /// IOptions<ResourceOption/>
        /// </param>
        public ApiHandlerResourceHelper(IOptions<ResourceOption> option)
            : base(option)
        {
            ResourcePath += option.Value.ApiHandling;
            SetResourceManager();
        }
    }
}
