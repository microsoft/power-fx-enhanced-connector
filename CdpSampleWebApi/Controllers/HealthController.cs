// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Dataverse;

#pragma warning disable CA1024 // Use properties where appropriate (GetHealth)
#pragma warning disable CA1034 // Do not nest type HealthStats, FxVersions

namespace CdpSampleWebApi.Controllers
{
    /// <summary>
    /// Controller for health and versioning endpoints for the Power Apps connector API.
    /// </summary>
    // For Power Apps, routes must be hosted directly at /, and not have any additional prefix.
    // Webapp can have the extra prefix and things. It's just that the connector needs to have extra logic in Policies.xml
    // which is a runtime logic where we can update the backend url with prefixes.
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        public HealthController()
        {
        }

        /// <summary>
        /// Gets health and versioning statistics for the service.
        /// </summary>
        /// <returns>A <see cref="HealthStats"/> object containing health information.</returns>
        [HttpGet]
        [Route("$health")]
        public HealthStats GetHealth()
        {
            var stats = new HealthStats();
            return stats;
        }

        /// <summary>
        /// Contains health and versioning statistics for the service.
        /// </summary>
        public class HealthStats
        {
            /// <summary>
            /// Gets or sets the process ID of the running service.
            /// </summary>
            public int ProcessId { get; set; } = Environment.ProcessId;

            /// <summary>
            /// Gets or sets the .NET runtime version.
            /// </summary>
            public string DotNetVer { get; set; } = typeof(object).Assembly.ImageRuntimeVersion;

            /// <summary>
            /// Gets or sets the UTC time when the service started.
            /// </summary>
            public DateTime StartupUtctime { get; set; } = Program.StartupTime;

            /// <summary>
            /// Gets or sets the current UTC time.
            /// </summary>
            public DateTime CurrentUtctime { get; set; } = DateTime.UtcNow;

            /// <summary>
            /// Gets the uptime of the service.
            /// </summary>
            public TimeSpan Uptime => this.CurrentUtctime - StartupUtctime;

            /// <summary>
            /// Gets the operating system version string.
            /// </summary>
            public string OsVersion => Environment.OSVersion.VersionString;

            /// <summary>
            /// Gets or sets the versions of various Power Fx components.
            /// </summary>
            public FxVersions FxVersions { get; set; } = new FxVersions();
        }

        /// <summary>
        /// Captures versions of various core Power Fx components.
        /// </summary>
        public class FxVersions
        {
            /// <summary>
            /// Gets the version of the Power Fx core engine.
            /// </summary>
            public string FxCore => GetVer<Engine>();

            /// <summary>
            /// Gets the version of the Power Fx intermediate core engine.
            /// </summary>
            public string FxIntCore => GetVer<RecalcEngine>();

            /// <summary>
            /// Gets the version of the Power Fx Dataverse connector.
            /// </summary>
            public string FxDataverse => GetVer<DataverseConnection>();

            /// <summary>
            /// Gets the version of the Power Fx connector data source.
            /// </summary>
            public string FxConnector => GetVer<CdpDataSource>();

            /// <summary>
            /// Gets the version string for the specified type.
            /// </summary>
            /// <typeparam name="T">The type whose assembly version to retrieve.</typeparam>
            /// <returns>The informational version string of the assembly.</returns>
            private static string GetVer<T>()
            {
                var assembly = typeof(T).Assembly;
                var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

                var fullVerStr = attr?.InformationalVersion;
                return fullVerStr;
            }
        }
    }
}
