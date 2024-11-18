
namespace TSL.Base.Platform.lnversionOfControl
{
    /// <summary>
    /// Request LifeCycle
    /// </summary>
    public enum IocType
    {
        /// <summary>
        /// The instance is alive in all web process
        /// </summary>
        Singleton,

        /// <summary>
        /// The instance is alive in this http request.
        /// </summary>
        Scoped,

        /// <summary>
        /// Everytime you call the instance is a new instance.
        /// </summary>
        Transient,

        /// <summary>
        /// The defualt http client type where it will retry once and do the logging.
        /// </summary>
        // HttpClientDefault,

        /// <summary>
        /// Resilient type ioc uses resilient policy which realizes following rules：
        /// Retry - Keep retrying with exponential back-off gap: 1, 2, 8, 16 ... etc.
        /// CircuitBreaker - If retrying more than 3 times, it breaks.
        /// Transient Fallback - Publish the request to queue for later Control-M retry.
        /// </summary>
        // HttpClientResilient,

        /// <summary>
        /// Retry infinite type ioc uses retry infinite policy adn retry the request infinitly.
        /// </summary>
        // HttpClientRetryInfinite,

        /// <summary>
        /// The http client type without retry policy
        /// </summary>
        // HttpClientWithoutRetry,
    }
}
