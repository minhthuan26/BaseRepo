using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemManagement.Helpers
{
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Get all environment variables
        /// </summary>
        public static Dictionary<string, string> GetAll()
        {
            IDictionary envs = Environment.GetEnvironmentVariables();

            return envs.Keys
                .Cast<string>()
                .ToDictionary(
                    key => key,
                    key => envs[key]?.ToString() ?? string.Empty
                );
        }

        /// <summary>
        /// Get environment variable by key
        /// </summary>
        public static string Get(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        /// <summary>
        /// Create or update environment variable
        /// </summary>
        public static void Set(
            string key,
            string value,
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            Environment.SetEnvironmentVariable(key, value, target);
        }

        /// <summary>
        /// Remove environment variable
        /// </summary>
        public static void Delete(
            string key,
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            Environment.SetEnvironmentVariable(key, null, target);
        }

        /// <summary>
        /// Check environment variable exists
        /// </summary>
        public static bool Exists(string key)
        {
            return !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(key)
            );
        }

        /// <summary>
        /// Update environment variable only if exists
        /// </summary>
        public static bool Update(
            string key,
            string value,
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            if (!Exists(key))
                return false;

            Set(key, value, target);

            return true;
        }

        /// <summary>
        /// Create environment variable only if not exists
        /// </summary>
        public static bool Create(
            string key,
            string value,
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            if (Exists(key))
                return false;

            Set(key, value, target);

            return true;
        }
    }
}