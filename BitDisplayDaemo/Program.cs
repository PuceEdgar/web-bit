using BitDisplayDaemo;
using BitDisplayDaemo.SSNS_ENV;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<SSENV2Manager>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
