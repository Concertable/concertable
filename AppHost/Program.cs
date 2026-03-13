var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .AddDatabase("DefaultConnection");

builder.AddProject<Projects.Web>("api")
       .WithReference(sql)
       .WaitFor(sql);

builder.Build().Run();
