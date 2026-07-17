using CommonLibs.Options;
using EncryptionLibrary;
using log4net.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
        bool TestConnection(out string message);
    }

    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlConnectionFactory> _logger;

        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates and returns a new, unopened SqlConnection instance.
        /// </summary>
        public IDbConnection CreateConnection()
        {
            string connectionString = GetConnectionString();
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Reuses your existing DatabaseHelper to test the configured connection.
        /// </summary>
        public bool TestConnection(out string message)
        {
            try
            {
                string connectionString = GetConnectionString();
                return DatabaseHelper.TestSqlConnection(connectionString, out message);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public string GetConnectionString(bool isEncrypt = true)
        {
            string applicationId = _configuration["Application:ApplicationId"] ?? throw new InvalidOperationException("ApplicationId is missing."); 
            string applicationName = _configuration["Application:ApplicationName"] ?? throw new InvalidOperationException("ApplicationName is missing.");
            string? host = _configuration["ConnectionInfo:Host"];
            if (string.IsNullOrEmpty(host))
            {
                throw new InvalidOperationException("Host configuration is missing.");
            }
            string? database = _configuration["ConnectionInfo:Database"];
            if (string.IsNullOrEmpty(database))
            {
                throw new InvalidOperationException("Database configuration is missing.");
            }
            string? userName = _configuration["ConnectionInfo:Username"];
            if (string.IsNullOrEmpty(userName))
            {
                throw new InvalidOperationException("Username configuration is missing.");
            }
            string? password = _configuration["ConnectionInfo:Password"];
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Password configuration is missing.");
            }

            string connectionString = string.Empty;
            if (isEncrypt)
            {
                _logger.LogInformation($"Building connection string from configuration with uid: {EncryptionUtils.EncryptData("lishn", applicationName, applicationId)} - pwd: {EncryptionUtils.EncryptData("lclliss1hn", applicationName, applicationId)}");
                connectionString = "server=" + host + ";database=" + database +
                    ";uid=" + EncryptionUtils.Decrypt(userName, applicationName, applicationId) +
                    ";pwd=" + EncryptionUtils.Decrypt(password, applicationName, applicationId) +
                    ";MultipleActiveResultSets=true;Application Name=" + applicationName + ";encrypt=true;TrustServerCertificate=true";
            } else
            {
                _logger.LogInformation($"Building connection string from configuration with uid: {userName} - pwd: {password}");
                connectionString = "server=" + host + ";database=" + database +
                        ";uid=" + userName +
                        ";pwd=" + password +
                        ";MultipleActiveResultSets=true;Application Name=" + applicationName + ";encrypt=true;TrustServerCertificate=true";
            }

            return connectionString;
        }
    }
}
