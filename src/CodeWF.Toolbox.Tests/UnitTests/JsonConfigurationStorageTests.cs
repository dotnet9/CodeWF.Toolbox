using CodeWF.Core.ConfigDB;
using CodeWF.Modules.Development.Entities;
using CodeWF.Modules.Development.Helpers;
using Microsoft.Data.Sqlite;

namespace CodeWF.Toolbox.Tests.UnitTests;

[CollectionDefinition("Toolbox storage", DisableParallelization = true)]
public sealed class ToolboxStorageCollection { }

[Collection("Toolbox storage")]
public class JsonConfigurationStorageTests
{
    [Fact]
    public void JsonConfigurationUsesIndependentDataDirectoriesWithoutCreatingDatabaseInWorkingDirectory()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var originalData = Environment.GetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY");
        var root = Path.Combine(Path.GetTempPath(), "CodeWF.Toolbox.Tests", Guid.NewGuid().ToString("N"));
        var working = Path.Combine(root, "working");
        var first = Path.Combine(root, "first;data", "nested");
        var second = Path.Combine(root, "second", "nested");
        Directory.CreateDirectory(working);
        try
        {
            Environment.SetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY", null);
            Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodeWF.Toolbox"), ToolboxStoragePaths.Root);
            Environment.CurrentDirectory = working;
            Environment.SetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY", first);
            Assert.Equal(first, CodeWF.Toolbox.Services.ToolboxDataPaths.Root);
            Assert.True(Path.IsPathFullyQualified(new SqliteConnectionStringBuilder(DBConst.DBConnectionString).DataSource));
            Assert.True(ConfigHelper.EnsureTableIsCreated());
            Assert.True(ConfigHelper.UpdateJsonPrettifyConfig(new JsonPrettifyEntity { IsSortKey = true, IndentSize = 4 }));
            var saved = ConfigHelper.GetJsonPrettifyConfig();
            Assert.True(saved.IsSortKey);
            Assert.Equal(4, saved.IndentSize);
            Assert.True(File.Exists(Path.Combine(first, "CodeWF.Toolbox.db")));

            Environment.SetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY", second);
            var independent = ConfigHelper.GetJsonPrettifyConfig();
            Assert.False(independent.IsSortKey);
            Assert.Equal(2, independent.IndentSize);
            Assert.True(ConfigHelper.UpdateJsonPrettifyConfig(new JsonPrettifyEntity { IsSortKey = false, IndentSize = 6 }));
            Assert.Equal(6, ConfigHelper.GetJsonPrettifyConfig().IndentSize);
            Assert.True(File.Exists(Path.Combine(second, "CodeWF.Toolbox.db")));

            Environment.SetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY", first);
            Assert.True(ConfigHelper.GetJsonPrettifyConfig().IsSortKey);
            Assert.Equal(4, ConfigHelper.GetJsonPrettifyConfig().IndentSize);
            Assert.False(File.Exists(Path.Combine(working, "CodeWF.Toolbox.db")));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Environment.CurrentDirectory = originalDirectory;
            Environment.SetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY", originalData);
        }
    }
}
