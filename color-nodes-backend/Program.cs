using color_nodes_backend.Data;
using color_nodes_backend.Hubs;
using color_nodes_backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=colornodes.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// Servicios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IGameService, GameService>();

// CORS
var MyCors = "_myCors";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(MyCors, p => p
        .WithOrigins(
            "http://localhost:3174",          // front local
            "http://26.233.244.31:7081"       // front radmin
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyCors);
app.UseAuthorization();

app.MapGet("/health", () => "OK");

app.MapControllers();
app.MapHub<GameHub>("/gameHub"); 

app.Run();
