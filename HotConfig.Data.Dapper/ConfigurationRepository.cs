using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HotConfig.Data.Dapper.Abstract;
using HotConfig.Domain.Concrete.Entity;
using Dapper;
using MySqlConnector;

namespace HotConfig.Data.Dapper
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private string ConnectionString { get; }
        private IDbConnection DbConnection => new MySqlConnection(this.ConnectionString);

        public async Task<IEnumerable<Configuration>> GetAllAsync()
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"SELECT Id,
                                              Name,
                                              Type,
                                              Value,
                                              IsActive,
                                              ApplicationName
                                       FROM Configuration";

                return await dbConnection.QueryAsync<Configuration>(query);
            }
        }

        public async Task<IEnumerable<Configuration>> FilterAsync(string applicationName, bool isActive = true)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"SELECT Id,
                                              Name,
                                              Type,
                                              Value,
                                              IsActive,
                                              ApplicationName
                                       FROM Configuration
                                       WHERE ApplicationName = @ApplicationName AND 
                                             IsActive = @IsActive";

                return await dbConnection.QueryAsync<Configuration>(query, new {ApplicationName = applicationName, IsActive = isActive});
            }
        }

        public async Task<IEnumerable<Configuration>> SearchAsync(string applicationName, string name, bool? isActive)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"SELECT Id,
                                              Name,
                                              Type,
                                              Value,
                                              IsActive,
                                              ApplicationName
                                       FROM Configuration
                                       WHERE (@ApplicationName IS NULL OR ApplicationName = @ApplicationName) AND
                                             (@NamePattern IS NULL OR Name LIKE @NamePattern) AND
                                             (@IsActive IS NULL OR IsActive = @IsActive)
                                       ORDER BY ApplicationName, Name";

                var namePattern = string.IsNullOrEmpty(name)
                                          ? null
                                          : "%" + name.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
                var parameters = new
                                 {
                                     ApplicationName = string.IsNullOrEmpty(applicationName) ? null : applicationName,
                                     NamePattern = namePattern,
                                     IsActive = isActive
                                 };
                return await dbConnection.QueryAsync<Configuration>(query, parameters);
            }
        }

        public async Task<Configuration> GetAsync(int id)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"SELECT Id,
                                              Name,
                                              Type,
                                              Value,
                                              IsActive,
                                              ApplicationName
                                       FROM Configuration
                                       WHERE Id = @Id";

                return await dbConnection.QueryFirstOrDefaultAsync<Configuration>(query, new {Id = id});
            }
        }

        public async Task<Configuration> GetAsync(string applicationName, string name, bool isActive = true)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"SELECT Id,
                                              Name,
                                              Type,
                                              Value,
                                              IsActive,
                                              ApplicationName
                                       FROM Configuration
                                       WHERE ApplicationName = @ApplicationName AND
                                             Name = @Name AND 
                                             IsActive = @IsActive";

                var parameters = new
                                 {
                                     ApplicationName = applicationName,
                                     Name = name,
                                     IsActive = isActive
                                 };
                return await dbConnection.QueryFirstOrDefaultAsync<Configuration>(query, parameters);
            }
        }

        public async Task<Configuration> GetAsync(string name)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"SELECT Id,
                                              Name,
                                              Type,
                                              Value,
                                              IsActive,
                                              ApplicationName
                                       FROM Configuration
                                       WHERE Name = @Name";

                return await dbConnection.QueryFirstOrDefaultAsync<Configuration>(query, new {Name = name});
            }
        }

        public async Task AddAsync(Configuration entity)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"INSERT INTO Configuration (Name, Type, Value, IsActive, ApplicationName)
                                       VALUES (@Name,@Type,@Value,@IsActive,@ApplicationName)";

               await dbConnection.ExecuteAsync(query, entity);
            }
        }

        public async Task<int> InsertAsync(Configuration entity)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"INSERT INTO Configuration (Name, Type, Value, IsActive, ApplicationName)
                                       VALUES (@Name,@Type,@Value,@IsActive,@ApplicationName);
                                       SELECT LAST_INSERT_ID();";

                return Convert.ToInt32(await dbConnection.ExecuteScalarAsync<long>(query, entity));
            }
        }

        public async Task UpdateAsync(Configuration entity)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"UPDATE Configuration
                                       SET Name = @Name, 
                                           Type = @Type, 
                                           Value = @Value, 
                                           IsActive = @IsActive, 
                                           ApplicationName = @ApplicationName
                                       WHERE Id = @Id";

                await dbConnection.ExecuteAsync(query, entity);
            }
        }

        public async Task RemoveAsync(Configuration entity)
        {
            await this.RemoveAsync(entity?.Id ?? throw new ArgumentNullException(nameof(entity)));
        }

        public async Task RemoveAsync(int id)
        {
            using (var dbConnection = this.DbConnection)
            {
                const string query = @"DELETE FROM Configuration WHERE Id = @Id";

                await dbConnection.ExecuteAsync(query, new {Id = id});
            }
        }

        public ConfigurationRepository(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

    }
}