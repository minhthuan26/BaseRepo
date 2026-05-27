using EncryptionLibrary;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
        bool TestConnection(out string message);
    }

    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
            string applicationId = EncryptionUtils.GetApplicationId();
            string applicationName = EncryptionUtils.GetApplicationName();
            string? host = _configuration["Host"];
            if (string.IsNullOrEmpty(host))
            {
                throw new InvalidOperationException("Host configuration is missing.");
            }
            string? database = _configuration["Database"];
            if (string.IsNullOrEmpty(database))
            {
                throw new InvalidOperationException("Database configuration is missing.");
            }
            string? userName = _configuration["Username"];
            if (string.IsNullOrEmpty(userName))
            {
                throw new InvalidOperationException("Username configuration is missing.");
            }
            string? password = _configuration["Password"];
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Password configuration is missing.");
            }
            string connectionString = "server=" + host + ";database=" + database +
                    ";uid=" + userName +
                    ";pwd=" + password +
                    ";MultipleActiveResultSets=true;Application Name=" + applicationName + ";";
            if (isEncrypt)
            {
                connectionString = "server=" + host + ";database=" + database +
                    ";uid=" + EncryptionUtils.Decrypt(userName, applicationName, applicationId) +
                    ";pwd=" + EncryptionUtils.Decrypt(password, applicationName, applicationId) +
                    ";MultipleActiveResultSets=true;Application Name=" + applicationName + ";";
            }

            return connectionString;
        }
    }
}
