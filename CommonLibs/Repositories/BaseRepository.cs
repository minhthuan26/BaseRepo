using AutoMapper;
using CommonLibs.Options;
using CommonLibs.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;

namespace CommonLibs.Repositories
{
    public abstract class BaseRepository<T>
    {
        protected IMapper _mapper;
        protected ILogger<T> _logger;
        protected IConfiguration _configuration;
        protected IHttpContextAccessor _httpContext;
        protected AppOptions _options;
        protected IAuditContext _auditContext;
        protected BaseRepository(IMapper mapper, ILogger<T> logger, IConfiguration configuration, IHttpContextAccessor httpContext, IOptions<AppOptions> options, IAuditContext auditContext)
        {
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _httpContext = httpContext;
            _options = options.Value;
            _auditContext = auditContext;
        }

        protected async Task<DataTable> ExecuteProcAsync(
            DbContext dbContext,
            string procName,
            params SqlParameter[] parameters)
        {
            var table = new DataTable();

            await using var connection = dbContext.Database.GetDbConnection();
            await using var command = connection.CreateCommand();

            command.CommandText = procName;
            command.CommandType = CommandType.StoredProcedure;

            if (parameters != null && parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();
            table.Load(reader);

            return table;
        }

        protected async Task<List<TModel>> ExecuteProcAsync<TModel>(
            DbContext dbContext,
            string procName,
            Func<SqlDataReader, TModel> map,
            params SqlParameter[] parameters)
        {
            var results = new List<TModel>();

            await using var connection = dbContext.Database.GetDbConnection();
            await using var command = connection.CreateCommand();

            command.CommandText = procName;
            command.CommandType = CommandType.StoredProcedure;

            if (parameters?.Length > 0)
                command.Parameters.AddRange(parameters);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(map((SqlDataReader)reader));
            }

            return results;
        }
    }
}
