using Npgsql;

namespace boardomapi.Database;

public static class DbConfig
{
  public static NpgsqlDataSource CreateDataSource(string connectionString)
  {
    var builder = new NpgsqlDataSourceBuilder(connectionString);
    builder.ConnectionStringBuilder.MaxPoolSize = 100;
    builder.ConnectionStringBuilder.MinPoolSize = 10;
    builder.ConnectionStringBuilder.ConnectionLifetime = 300;
    builder.ConnectionStringBuilder.ConnectionIdleLifetime = 60;
    builder.ConnectionStringBuilder.Timeout = 30;
    return builder.Build();
  }
}
