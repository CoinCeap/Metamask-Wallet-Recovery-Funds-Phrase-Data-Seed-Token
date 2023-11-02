using Microsoft.VisualBasic;
using metamask_mini_api.Services;
using Serilog;
using Serilog.Events;

// var connection = new Seq.Api.SeqConnection("http://localhost:18080", "ZHJOkr6jFfLrcGBZs2ZU");
// await connection.Events.DeleteAsync();

Console.WriteLine("json-rpc api listening on " + metamask_mini_api.Shared.Constants.Url);
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(metamask_mini_api.Shared.Constants.Url);
var myLogger = new LoggerConfiguration()
    .Destructure.ByTransforming<System.Numerics.BigInteger>(x => x.ToString())
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.With()
    .WriteTo.Console()//outputTemplate:"{HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
    // .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
Serilog.Log.Logger = myLogger;
Serilog.SerilogHostBuilderExtensions.UseSerilog(builder.Host);
builder.Services.AddControllers();
builder.Services.AddTransient<MinimalEthApi>();
builder.Services.AddSingleton(SimpleBlockChain.Initialize());
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyOrigin().WithMethods("GET", "POST"));
app.MapControllers();
app.Run();