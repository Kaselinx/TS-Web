

namespace TSL.Base.Platform.lnversionOfControl
{
    /// <summary>
    /// Register Dependency Injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class RegisterIOCAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterIOCAttribute"/> class.
        /// Auto Use The Class/Interface for IOC
        /// </summary>
        public RegisterIOCAttribute()
        {
            LifeCycle = IocType.Scoped;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterIOCAttribute"/> class.
        /// Specify LifeCycle
        /// </summary>
        /// <param name="lifeCycle">DI lifeCycle</param>
        public RegisterIOCAttribute(IocType lifeCycle)
        {
            LifeCycle = lifeCycle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterIOCAttribute"/> class.
        /// Specify IOC Type
        /// </summary>
        /// <param name="interfaceType">Interface</param>
        /// <param name="implementationType">Implementation</param>
        /// <param name="lifeCycle">LifeCycle</param>
        public RegisterIOCAttribute(Type interfaceType, Type implementationType, IocType lifeCycle)
        {
            TInterface = interfaceType;
            TImplementation = implementationType;
            LifeCycle = lifeCycle;
        }

        /// <summary>
        /// Interface for DI
        /// </summary>
        public Type TInterface { get; set; }

        /// <summary>
        /// Implement for DI
        /// </summary>
        public Type TImplementation { get; set; }

        /// <summary>
        /// DI LifeCycle
        /// </summary>
        public IocType LifeCycle { get; set; }
    }
}
