using Training.ApiGateway.Middleware;
using Training.ApiGateway.Converters;
using Training.ApiGateway.Repositories;
using Training.ApiGateway.Services;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required");
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is required");

builder.Services.AddSingleton(new JwtTokenService(jwtSecret));
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connStr));
builder.Services.AddScoped<AuthService>();

builder.Services.AddGrpcClient<Training.Training.Proto.TrainingService.TrainingServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Training"] ?? "http://localhost:5003"));
builder.Services.AddGrpcClient<Training.AI.Proto.AiService.AiServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Ai"] ?? "http://localhost:5004"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new MuscleGroupJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Microsoft.AspNetCore.Server.Kestrel.Experimental.DisableHttp2Tls", true);
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseMiddleware<JwtMiddleware>();
app.MapControllers();
app.MapGet("/health", () => "OK");

app.Run();
