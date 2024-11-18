using Microsoft.AspNetCore.Mvc;
using SoapCore;
using StackExchange.Profiling.Storage;
using System;
using System.Configuration;
using System.Reflection;
using System.ServiceModel.Channels;
using TSL.Base.Platform.DataAccess;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.Services;
using TSL.Base.Platform.Utilities;
using TSL.TAAA.Service.Interface;
using TSL.TAAS.API.Helpers;
using TSL.TAAS.Service.OPT;
using TSL.TAAS.Service.POT;
using TSL.TOTP.Service.Interface;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// NLog: setup the logger first to catch all errors
var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
try
{
    logger.Debug("init main");

    // Call the ConfigureServices method to add services to the container
    ConfigureServices(builder.Services, builder.Environment);

    var app = builder.Build();

    // Set CacheDuration for MemoryCacheStorage
    var memoryCacheStorage = app.Services.GetService<MemoryCacheStorage>();
    if (memoryCacheStorage != null)
    {
        memoryCacheStorage.CacheDuration = TimeSpan.FromMinutes(60);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    // Configure the HTTP request pipeline.
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.UseRouting();
    app.UseSession(); // Enable session middleware

    // Configure SoapEncoderOptions
    var soapEncoderOptions = new SoapEncoderOptions
    {
        MessageVersion = MessageVersion.Soap12WSAddressing10,
        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
        {
            MaxDepth = 32,
            MaxStringContentLength = 8192,
            MaxArrayLength = 16384,
            MaxBytesPerRead = 4096,
            MaxNameTableCharCount = 16384
        }
    };

    // UseSoapEndpoint with configured SoapEncoderOptions
    app.UseSoapEndpoint<IOTPService>("/OTPService.asmx", soapEncoderOptions);
    app.UseSoapEndpoint<IPOTService>("/POTService.asmx", soapEncoderOptions);

    #region Set Web API Operation Logging

    try
    {
        app.UseMiddleware<WebAPIRequestResponseLogMiddleware>();
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error setting up WebAPIRequestResponseLogMiddleware");
    }

    #endregion

    #region Set Logger

    string connectionString = app.Configuration["Logging:ConnectionString"];
    if (!string.IsNullOrEmpty(connectionString))
    {
        app.UseLogInitial(connectionString);
    }
    else
    {
        logger.Warn("Logging connection string is not configured.");
    }

    #endregion

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

void ConfigureServices(IServiceCollection services, IWebHostEnvironment environment)
{
    var configuration = builder.Configuration;
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    #region database setting
    services.AddOptions();
    // Add the configuration to our DI container for later user add tsaa and tstd data access layer
    services.Configure<TAAADataAccessOption>(configuration.GetSection("TAAADataAccessLayer"));
    services.Configure<TSTDDataAccessOption>(configuration.GetSection("TSTDDataAccessLayer"));
    #endregion

    #region web services setting 

    services.AddSoapCore();
    services.AddSingleton<IOTPService, OTPService>();
    services.AddSingleton<IPOTService, POTService>();
    services.AddDataProtection();
    services.AddDistributedMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    #endregion

    services.Configure<TransactionTableWhiteListOption>(configuration.GetSection("TransactionTableWhitelist"));

    #region register require service in service layer
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddScoped<IHttpContextInfoService, HttpContextInfoService>();
    services.AddScoped(typeof(ILog<>), typeof(Log<>));
    #endregion

    services.AddSingleton<NLog.ILogger>(serviceProvider =>
    {
        var environmentName = environment.EnvironmentName;
        return NLogBuilder.ConfigureNLog($"nlog.{environmentName}.config").GetLogger("Main");
    });

    ConfigureInjection(services);

    #region mini profiler settings
    services.AddMiniProfiler(options =>
    {
        options.RouteBasePath = "/profiler";
        (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);
        options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
        options.ResultsAuthorize = request => environment.IsEnvironment("UAT") || environment.IsEnvironment("SIT") || true;
        options.ResultsListAuthorize = request => environment.IsEnvironment("UAT") || environment.IsEnvironment("SIT") || true;
        options.ResultsAuthorizeAsync = async request => environment.IsEnvironment("UAT") || environment.IsEnvironment("SIT") || true;
        options.ResultsListAuthorizeAsync = async request => environment.IsEnvironment("UAT") || environment.IsEnvironment("SIT") || true;
        options.ShouldProfile = request => true;
        options.UserIdProvider = request => null;
        options.TrackConnectionOpenClose = true;
        options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
        options.PopupDecimalPlaces = 1;
        options.EnableMvcFilterProfiling = true;
        options.EnableMvcViewProfiling = true;
    });
    #endregion  
}

static void ConfigureInjection(IServiceCollection services)
{
    services.AddTransient<HttpClientLoggingHandler>();

    var register = new RegisterIOC();
    register.DependencyInjection(
        (interfaceType, imType, life) =>
        {
            switch (life)
            {
                case IocType.Scoped:
                    services.AddScoped(interfaceType, imType);
                    break;
                case IocType.Singleton:
                    services.AddSingleton(interfaceType, imType);
                    break;
                case IocType.Transient:
                    services.AddTransient(interfaceType, imType);
                    break;
                default:
                    services.AddScoped(interfaceType, imType);
                    break;
            }
        }, string.Empty);
}
