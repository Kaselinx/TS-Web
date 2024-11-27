using Microsoft.AspNetCore.Builder;
using NLog;
using NLog.Targets;
using System; // Add this using directive for Guid

namespace TSL.Base.Platform.Log
{
    /// <summary>
    /// Application Log Middleware
    /// </summary>
    public static class AppBuilderLogExtension
    {
        /// <summary>
        /// Middleware for Log Initial
        /// </summary>
        /// <param name="app">Application Builder</param>
        /// <param name="connectionString">Log Connection String</param>
        public static void UseLogInitial(this IApplicationBuilder app, string connectionString)
        {
            JsonConverters.JsonSerializerOptionsSetting.SetOption();
            // Binding target database connection string
            string[] targetItems = new string[]
            {
                "WebAPILog"
            };
            foreach (string val in targetItems)
            {
                Target target = LogManager.Configuration.FindTargetByName(val);

                if (target is DatabaseTarget dbTarget)
                {
                    dbTarget.ConnectionString = connectionString;
                    LogManager.ReconfigExistingLoggers();
                }
            }

            _ = app.Use(async (context, next) =>
            {
                // for same request has same log guid
                context.TraceIdentifier = Guid.NewGuid().ToString();
                await next().ConfigureAwait(false);
            });
        }
    }
}
