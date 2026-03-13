var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Web>("api");

builder.Build().Run();
