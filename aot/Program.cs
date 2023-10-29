using aot;
using Microsoft.AspNetCore.Connections;

var builder = WebApplication.CreateSlimBuilder(args);

//var section = builder.Configuration.GetSection("Kestrel");

builder.WebHost.UseSockets(o =>
{
    o.Backlog = 65535;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080,c=>c.UseConnectionHandler<RpcConnectionHandler>());
    //options.Configure(section).Endpoint("Echo", endpoint => endpoint.ListenOptions.UseConnectionHandler<RpcConnectionHandler>());
});

var app = builder.Build();

app.Run();