

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TSL.Base.Platform.lnversionOfControl
{
    /// <summary>
    /// Get Request Container
    /// </summary>
    [RegisterIOC(IocType.Singleton)]
    public class IOCContainer
    {
        private readonly ILogger<IOCContainer> _logger;
        private readonly IHttpContextAccessor _httpContext;

        /// <summary>
        /// IOC Container Default Use HttpContext as Request Container
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        public IOCContainer(ILogger<IOCContainer> logger, IHttpContextAccessor httpContext)
        {
            _logger = logger;
            _httpContext = httpContext;
        }

        public static Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; } = default!;

        /// <summary>
        /// Application Host Service Collection
        /// </summary>
        public static IServiceProvider ApplicationContainer { get; set; }

        public static IServiceCollection Services { get; set; }

        /// <summary>
        /// Get Service from Container
        /// </summary>
        /// <param name="useScope">use create scope method. Use it when not rely on http context</param>
        /// <typeparam name="T">Service Type</typeparam>
        /// <returns></returns>
        public T GetService<T>(bool useScope = false)
        {
            if (!useScope && _httpContext.HttpContext?.RequestServices != null)
            {
                try
                {
                    return _httpContext.HttpContext.RequestServices.GetRequiredService<T>();
                }
                catch (Exception ex)
                {
                    Exception innerex = ex;
                    while (innerex.InnerException != null)
                    {
                        innerex = innerex.InnerException;
                    }

                    //when _container.GetService<DALInterface.Common.ICommonDataProvider>(); is raised from
                    //speedup service instead of regular HTTP request, ObjectDisposed exception can happen.

                    // prevent service resovler throw "Cannot access a disposed object" on normal case
                    if (innerex.GetType() == typeof(System.ObjectDisposedException))
                    //&& _httpContext.HttpContext.Request.Body == System.IO.Stream.Null)
                    {
                        using (IServiceScope scope = IOCContainer.ApplicationContainer.CreateScope())
                        {
                            return scope.ServiceProvider.GetService<T>();
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                // for RabbitMQ Handler
                using (IServiceScope scope = IOCContainer.ApplicationContainer.CreateScope())
                {
                    return scope.ServiceProvider.GetService<T>();
                }
            }
        }

        /// <summary>
        /// Get Service from Container
        /// </summary>
        /// <param name="type">Service Type</param>
        /// <param name="useScope">useScope</param>
        /// <returns></returns>
        public object GetService(Type type, bool useScope = false)
        {
            if (_httpContext.HttpContext != null && _httpContext.HttpContext.RequestServices != null && useScope == false)
            {
                return _httpContext.HttpContext.RequestServices.GetRequiredService(type);
            }
            else
            {
                // for RabbitMQ Handler
                using (IServiceScope scope = IOCContainer.ApplicationContainer.CreateScope())
                {
                    return scope.ServiceProvider.GetService(type);
                }
            }
        }

        /// <summary>
        /// Service WarmUp
        /// </summary>
        /// <returns></returns>
        public Task<List<KeyValuePair<string, double>>> WarmUp()
        {
            var result = new List<KeyValuePair<string, double>>();
            var serviceSW = new System.Diagnostics.Stopwatch();
            _logger.LogDebug("===== start =====");

            using (IServiceScope scope = ApplicationContainer.CreateScope())
            {
                foreach (var service in GetWarmUpService())
                {
                    serviceSW.Reset();
                    serviceSW.Start();
                    _logger.LogDebug(service.FullName);
                    scope.ServiceProvider.GetServices(service);
                    serviceSW.Stop();
                    _logger.LogDebug($"{service.FullName}, {serviceSW.Elapsed.TotalMilliseconds}");
                    result.Add(new KeyValuePair<string, double>(service.FullName, serviceSW.Elapsed.TotalMilliseconds));
                }
            }

            _logger.LogDebug("===== end =====");

            return Task.FromResult(result);
        }

        private static IEnumerable<Type> GetWarmUpService()
        {
            return Services
                //.Where(descriptor => descriptor.Lifetime != ServiceLifetime.Transient)
                .Where(descriptor => descriptor.ImplementationType != typeof(IOCContainer))
                .Where(descriptor => descriptor.ServiceType.ContainsGenericParameters == false)
                .Where(descriptor => string.Equals(descriptor.ServiceType.FullName, "SKH.HIS.Model.Schedule.SkmhContext", StringComparison.OrdinalIgnoreCase) == false)
                .Where(descriptor => descriptor.ServiceType.FullName.StartsWith("SKH.HIS.", StringComparison.OrdinalIgnoreCase))
                .Select(descriptor => descriptor.ServiceType)
                .Distinct();
        }
    }
}
