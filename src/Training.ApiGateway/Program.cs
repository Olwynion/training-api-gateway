using Training.ApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddGrpcClient<Training.Auth.AuthService.AuthServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Auth"] ?? "http://localhost:5002"));
builder.Services.AddGrpcClient<Training.Training.Proto.TrainingService.TrainingServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Training"] ?? "http://localhost:5003"));
builder.Services.AddGrpcClient<Training.AI.Proto.AiService.AiServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Ai"] ?? "http://localhost:5004"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Microsoft.AspNetCore.Server.Kestrel.Experimental.DisableHttp2Tls", true);
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<JwtMiddleware>();
app.MapControllers();
app.MapGet("/health", () => "OK");

app.Run();
