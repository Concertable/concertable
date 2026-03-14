var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .WithDataVolume("concertable-sql-data")
                 .AddDatabase("DefaultConnection");

var api = builder.AddProject<Projects.Web>("api")
       .WithReference(sql)
       .WaitFor(sql);

builder.AddNpmApp("frontend", "../ClientApp", "start")
       .WithReference(api)
       .WaitFor(api);

builder.Build().Run();
