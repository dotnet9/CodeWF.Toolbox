namespace CodeWF.Core.ConfigDB;

public static class DBConst
{
    public static string DBConnectionString
    {
        get
        {
            Directory.CreateDirectory(ToolboxStoragePaths.Root);
            return new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = ToolboxStoragePaths.DatabasePath }.ToString();
        }
    }
}
