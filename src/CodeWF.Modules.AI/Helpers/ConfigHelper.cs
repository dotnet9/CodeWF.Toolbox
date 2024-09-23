using CodeWF.Core.ConfigDB;
using CodeWF.Modules.AI.Entities;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CodeWF.Modules.AI.Helpers;

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
                CREATE TABLE IF NOT EXISTS {nameof(TranslateLanguageEntity)}(
                    {nameof(TranslateLanguageEntity.Languages)} VARCHAR(3072)
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
    public static List<string>? GetTranslateLanguages()
    {
        try
        {
            EnsureTableIsCreated();

            using var connection = new SqliteConnection(DBConst.DBConnectionString);
            connection.Open();

            const string sql =
                $@"SELECT {nameof(TranslateLanguageEntity.Languages)} FROM {nameof(TranslateLanguageEntity)}";
            var config = connection.QueryFirstOrDefault<TranslateLanguageEntity>(sql);

            return config?.Languages?.Split(',').ToList();
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    [DapperAot]
    public static bool UpdateTranslateLanguages(List<string>? languages)
    {
        try
        {
            EnsureTableIsCreated();

            using var connection = new SqliteConnection(DBConst.DBConnectionString);
            connection.Open();
            connection.Execute($"DELETE FROM {nameof(TranslateLanguageEntity)}");
            if (languages?.Any() != true)
            {
                return false;
            }

            var languageStr = string.Join(",", languages)!;
            var result = connection.Execute(
                @$"INSERT INTO {nameof(TranslateLanguageEntity)}({nameof(TranslateLanguageEntity.Languages)})
                        VALUES(@{nameof(TranslateLanguageEntity.Languages)})",
                new { Languages = languageStr });
            return result > 0;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}