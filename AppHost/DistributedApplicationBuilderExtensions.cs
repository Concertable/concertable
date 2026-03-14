internal static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<SqlServerDatabaseResource> AddSqlServer(this IDistributedApplicationBuilder builder)
    {
        return builder.AddSqlServer("sql")
                      .WithDataVolume("concertable-sql-data")
                      .AddDatabase("DefaultConnection");
    }

    public static IResourceBuilder<ProjectResource> AddApi(this IDistributedApplicationBuilder builder, IResourceBuilder<SqlServerDatabaseResource> sql)
    {
        return builder.AddProject<Projects.Web>("api")
                      .WithReference(sql)
                      .WaitFor(sql);
    }

    public static IResourceBuilder<NodeAppResource> AddFrontend(this IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> api)
    {
        return builder.AddNpmApp("frontend", "../ClientApp", "start")
                      .WithReference(api)
                      .WaitFor(api);
    }
}
