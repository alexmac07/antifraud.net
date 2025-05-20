using Antifraud.Common.Settings;
using Antifraud.Core.Repository.Interface;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System.Data;

namespace Antifraud.Core.Repository.Implementation
{
    public class DapperRepository<T> : IDapperRepository<T> where T : class
    {
        private readonly string _connection;
        private readonly ILogger<DapperRepository<T>> _logger;

        public DapperRepository(IOptions<AppSettings> config, ILogger<DapperRepository<T>> logger)
        {
            _connection = config.Value.DBConnection;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            _logger = logger;
        }

        internal IDbConnection Connection
        {
            get { return new NpgsqlConnection(_connection); }
        }

        public async Task<int> ExecuteAsync(string query, object parameters = null)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    return await conn.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred on {nameof(ExecuteAsync)} while using {query} with parameters {JsonConvert.SerializeObject(parameters)}");
                throw;
            }

        }

        public async Task<IEnumerable<T>> QueryAsync(string query, object parameters = null)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    return await conn.QueryAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred on {nameof(QueryAsync)} while using {query} with parameters {JsonConvert.SerializeObject(parameters)}");
                throw;
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync(string query, object parameters = null)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    return await conn.QueryFirstOrDefaultAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred on {nameof(QueryFirstOrDefaultAsync)} while using {query} with parameters {JsonConvert.SerializeObject(parameters)}");
                throw;
            }
        }

    }
}
