using System;
using System.Data;
using System.Data.SQLite;

namespace AlfaSoftApp;

public static class Db
{
    private static readonly string ConnectionString =
        "Data Source=alfasoft.db3";

    public static SQLiteConnection Open()
    {
        var conn = new SQLiteConnection(ConnectionString);
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return conn;
    }

    public static DataTable Query(string sql,
        params (string name, object? value)[] parameters)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);

        var table = new DataTable();
        using var reader = cmd.ExecuteReader();
        table.Load(reader);
        return table;
    }

    public static int Execute(string sql,
        params (string name, object? value)[] parameters)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return cmd.ExecuteNonQuery();
    }
}