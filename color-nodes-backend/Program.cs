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
            "http://26.166.216.244:5197"       // front abierto por tu IP Radmin
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

var app = builder.Build();

// Swagger solo en dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usa la MISMA política que definiste arriba
app.UseCors(MyCors);

// ?? Trabajando en HTTP ? quita la redirección a HTTPS
// app.UseHttpsRedirection();

app.UseAuthorization();

// Opcional: endpoint de salud para probar conectividad
app.MapGet("/health", () => "OK");

app.MapControllers();
app.MapHub<GameHub>("/gameHub"); // puedes añadir .RequireCors(MyCors) si quieres

app.Run();
