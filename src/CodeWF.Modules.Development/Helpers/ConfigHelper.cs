using CodeWF.Core.ConfigDB;
using CodeWF.Modules.Development.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CodeWF.Modules.Development.Helpers;

public static class ConfigHelper
{
    [DapperAot]
    public static bool EnsureTableIsCreated()
    {
        try
        {
            using var connection = new SqliteConnection(DBConst.DBConnectionString);
            connection.Open();

            const string sql = $@"
                CREATE TABLE IF NOT EXISTS {nameof(JsonPrettifyEntity)}(
                    {nameof(JsonPrettifyEntity.IsSortKey)} Bool,
                    {nameof(JsonPrettifyEntity.IndentSize)} INTEGER
            )";

            using var command = new SqliteCommand(sql, connection);
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    [DapperAot]
    public static JsonPrettifyEntity GetJsonPrettifyConfig()
    {
        try
        {
            EnsureTableIsCreated();

            using var connection = new SqliteConnection(DBConst.DBConnectionString);
            connection.Open();

            const string sql =
                $@"SELECT {nameof(JsonPrettifyEntity.IsSortKey)},{nameof(JsonPrettifyEntity.IndentSize)} FROM {nameof(JsonPrettifyEntity)}";
            var config = connection.QueryFirstOrDefault<JsonPrettifyEntity>(sql);
            config ??= new JsonPrettifyEntity() { IsSortKey = false, IndentSize = 2 };

            return config;
        }
        catch (Exception ex)
        {
            return new JsonPrettifyEntity() { IsSortKey = false, IndentSize = 2 };
        }
    }
    
    [DapperAot]
    public static bool UpdateJsonPrettifyConfig(JsonPrettifyEntity config)
    {
        try
        {
            EnsureTableIsCreated();

            using var connection = new SqliteConnection(DBConst.DBConnectionString);
            connection.Open();
            connection.Execute($"DELETE FROM {nameof(JsonPrettifyEntity)}");
            var result = connection.Execute(
                @$"INSERT INTO {nameof(JsonPrettifyEntity)}({nameof(JsonPrettifyEntity.IsSortKey)},{nameof(JsonPrettifyEntity.IndentSize)})
                        VALUES(@{nameof(JsonPrettifyEntity.IsSortKey)},@{nameof(JsonPrettifyEntity.IndentSize)})",
                config);
            return result > 0;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}