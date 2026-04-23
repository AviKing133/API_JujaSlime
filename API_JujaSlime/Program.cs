var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 1. AÑADIR ESTO: Configurar la política de CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirJuego", policy =>
    {
        policy.AllowAnyOrigin() // O usa .WithOrigins("https://aviking133.github.io") para más seguridad
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- 2. AÑADIR ESTO: Activar CORS ---
// MUY IMPORTANTE: Debe ir después de HttpsRedirection y ANTES de Authorization
app.UseHttpsRedirection();

app.UseCors("PermitirJuego");

app.UseAuthorization();

app.MapControllers();

app.Run();