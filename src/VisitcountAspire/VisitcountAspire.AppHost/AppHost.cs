var builder = DistributedApplication.CreateBuilder(args);
var redis = builder.AddRedis("cache");

var api = builder.AddDotnetProject("api", "../../VisitcountAPI/VisitcountAPI.csproj")
                 .WithReference(redis)
                 .WaitFor(redis);

builder.AddDotnetProject("web", "../../VisitcountWeb/VisitcountWeb.csproj")
       .WithReference(api)
       .WaitFor(api)
       .WithExternalHttpEndpoints();
builder.Build().Run();
