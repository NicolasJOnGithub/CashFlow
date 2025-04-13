using Microsoft.Data.Sqlite;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.DataProvider.Sqlite;
public sealed class GilsightQueryFactory : IDisposable
{
    public static string DatabaseFile => Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "DefaultDbV2.sqlite");
    private static Compiler Compiler = new SqliteCompiler();

    public SqliteConnection Connection { get; init; }
    public QueryFactory Factory { get; init; }
    public GilsightQueryFactory()
    {
        Connection = new SqliteConnection($"Data Source={DatabaseFile}");
        Connection.Open();
        Factory = new QueryFactory(Connection, Compiler);
    }

    public void Dispose()
    {
        Factory.Dispose();
        Connection.Dispose();
    }

    public Query Query(string table) => Factory.Query(table);
    public Query Query() => Factory.Query();
}
