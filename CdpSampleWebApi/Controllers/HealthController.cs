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
    // For Power Apps, routes must be hosted directly at /, and not have any additional prefix.
    // Webapp can have the extra prefix and things. It's just that the connector needs to have extra logic in Policies.xml
    // which is a runtime logic where we can update the backend url with prefixes.
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        public HealthController()
        {
        }

        [HttpGet]
        [Route("$health")]
        public HealthStats GetHealth()
        {
            var stats = new HealthStats();
            return stats;
        }

        // Return various health / versioning statistics.
        // This is all public information.
        public class HealthStats
        {
            public int ProcessId { get; set; } = Environment.ProcessId;

            public string DotNetVer { get; set; } = typeof(object).Assembly.ImageRuntimeVersion;

            public DateTime StartupUtctime { get; set; } = Program.StartupTime;

            public DateTime CurrentUtctime { get; set; } = DateTime.UtcNow;

            public TimeSpan Uptime => this.CurrentUtctime - StartupUtctime;

            public string OsVersion => Environment.OSVersion.VersionString;

            public FxVersions FxVersions { get; set; } = new FxVersions();
        }

        // Capture versions of various core Fx components.
        public class FxVersions
        {
            public string FxCore => GetVer<Engine>();

            public string FxIntCore => GetVer<RecalcEngine>();

            public string FxDataverse => GetVer<DataverseConnection>();

            public string FxConnector => GetVer<CdpDataSource>();

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
