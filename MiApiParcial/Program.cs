using Microsoft.EntityFrameworkCore;
using MiApiParcial.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de MySQL
var useProduction = builder.Configuration.GetValue<bool>("UseProductionDB");
var connectionString = useProduction
    ? builder.Configuration.GetConnectionString("ProductionConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar MySQL con Pomelo
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    // Para desarrollo, mostrar queries SQL en consola
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configurar para Railway/Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Crear base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Crear la base de datos si no existe
        await context.Database.EnsureCreatedAsync();

        // Verificar si ya hay datos
        if (!await context.Productos.AnyAsync())
        {
            // Insertar datos de ejemplo solo si la tabla está vacía
            Console.WriteLine("Insertando datos de ejemplo...");
        }

        Console.WriteLine("Base de datos MySQL conectada correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error con MySQL: {ex.Message}");
        Console.WriteLine("Verifica que MySQL esté ejecutándose y las credenciales sean correctas");
    }
}

// Habilitar Swagger siempre
app.UseSwagger();
app.UseSwaggerUI();

// Configurar CORS para frontend
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();
app.MapControllers();

// Ruta raíz
app.MapGet("/", () => Results.Redirect("/swagger"));

// Endpoint de salud para verificar conexión
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var productCount = await db.Productos.CountAsync();

        return Results.Ok(new
        {
            status = "healthy",
            database = "MySQL",
            connected = canConnect,
            totalProducts = productCount,
            timestamp = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});

app.Run();