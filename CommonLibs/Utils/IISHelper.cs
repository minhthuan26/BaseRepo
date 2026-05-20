using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibs.Utils
{
    public static class IISHelper
    {
        /// <summary>
        /// Create IIS site with app pool (or update if exists)
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="physicalPath"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="appPoolName"></param>
        /// <param name="dotNetVersion"></param>
        /// <param name="integratedPipeline"></param>
        /// <param name="enable32Bit"></param>
        public static void CreateOrUpdateSite(
        string siteName,
        string physicalPath,
        string host,
        int port,
        string appPoolName,
        string dotNetVersion = "v4.0",
        bool integratedPipeline = true,
        bool enable32Bit = false)
        {
            using (var serverManager = new ServerManager())
            {
                //-----------------------------------
                // APP POOL
                //-----------------------------------
                var appPool = serverManager.ApplicationPools[appPoolName];

                if (appPool == null)
                {
                    appPool = serverManager.ApplicationPools.Add(appPoolName);
                }

                // .NET Version
                appPool.ManagedRuntimeVersion = dotNetVersion;
                // "" => No Managed Code
                // "v4.0"
                // "v2.0"

                // Pipeline
                appPool.ManagedPipelineMode =
                    integratedPipeline
                    ? ManagedPipelineMode.Integrated
                    : ManagedPipelineMode.Classic;

                // 32-bit
                appPool.Enable32BitAppOnWin64 = enable32Bit;

                // Auto start
                appPool.AutoStart = true;

                // Idle timeout
                appPool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes(20);

                // Recycle every 29 hours
                appPool.Recycling.PeriodicRestart.Time = TimeSpan.FromHours(29);

                //-----------------------------------
                // SITE
                //-----------------------------------
                var site = serverManager.Sites[siteName];

                if (site == null)
                {
                    site = serverManager.Sites.Add(
                        siteName,
                        "http",
                        $"*:{port}:{host}",
                        physicalPath);
                }
                else
                {
                    site.Applications["/"].VirtualDirectories["/"].PhysicalPath =
                        physicalPath;

                    site.Bindings.Clear();

                    site.Bindings.Add(
                        $"*:{port}:{host}",
                        "http");
                }

                // Assign app pool
                site.Applications["/"].ApplicationPoolName = appPoolName;

                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Get all IIS sites
        /// </summary>
        public static List<Site> GetAllSites()
        {
            using var serverManager = new ServerManager();

            return serverManager.Sites.ToList();
        }

        /// <summary>
        /// Get site by name
        /// </summary>
        public static Site GetSite(string siteName)
        {
            using var serverManager = new ServerManager();

            return serverManager.Sites
                .FirstOrDefault(x =>
                    x.Name.Equals(siteName,
                    StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Create new IIS site
        /// </summary>
        public static bool CreateSite(
            string siteName,
            string physicalPath,
            string host,
            int port,
            string appPool = null)
        {
            try
            {
                using var serverManager = new ServerManager();

                if (serverManager.Sites.Any(x =>
                    x.Name.Equals(siteName,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                string binding = $"*:{port}:{host}";

                Site site = serverManager.Sites.Add(
                    siteName,
                    "http",
                    binding,
                    physicalPath);

                if (!string.IsNullOrWhiteSpace(appPool))
                {
                    site.Applications["/"].ApplicationPoolName = appPool;
                }

                serverManager.CommitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Update IIS site binding/path
        /// </summary>
        public static bool UpdateSite(
            string siteName,
            string physicalPath,
            string host,
            int port)
        {
            try
            {
                using var serverManager = new ServerManager();

                Site site = serverManager.Sites[siteName];

                if (site == null)
                    return false;

                site.Applications["/"].VirtualDirectories["/"].PhysicalPath =
                    physicalPath;

                site.Bindings.Clear();

                site.Bindings.Add(
                    $"*:{port}:{host}",
                    "http");

                serverManager.CommitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete IIS site
        /// </summary>
        public static void DeleteSite(string siteName, bool deleteAppPool = false)
        {
            using (var serverManager = new ServerManager())
            {
                var site = serverManager.Sites[siteName];

                if (site != null)
                {
                    string appPoolName = site.Applications["/"].ApplicationPoolName;

                    serverManager.Sites.Remove(site);

                    if (deleteAppPool)
                    {
                        var appPool = serverManager.ApplicationPools[appPoolName];

                        if (appPool != null)
                        {
                            serverManager.ApplicationPools.Remove(appPool);
                        }
                    }

                    serverManager.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Start IIS site
        /// </summary>
        public static bool StartSite(string siteName)
        {
            try
            {
                using var serverManager = new ServerManager();

                Site site = serverManager.Sites[siteName];

                if (site == null)
                    return false;

                site.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stop IIS site
        /// </summary>
        public static bool StopSite(string siteName)
        {
            try
            {
                using var serverManager = new ServerManager();

                Site site = serverManager.Sites[siteName];

                if (site == null)
                    return false;

                site.Stop();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restart IIS site
        /// </summary>
        public static bool RestartSite(string siteName)
        {
            try
            {
                using var serverManager = new ServerManager();

                Site site = serverManager.Sites[siteName];

                if (site == null)
                    return false;

                site.Stop();
                site.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check site exists
        /// </summary>
        public static bool SiteExists(string siteName)
        {
            using var serverManager = new ServerManager();

            return serverManager.Sites.Any(x =>
                x.Name.Equals(siteName,
                StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get site state
        /// </summary>
        public static ObjectState GetSiteState(string siteName)
        {
            using var serverManager = new ServerManager();

            Site site = serverManager.Sites[siteName];

            return site?.State ?? ObjectState.Unknown;
        }
    }
}