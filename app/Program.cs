//http://localhost:7000/swagger/index.html
//http://localhost:7000/scalar/

using Api_ArjSys_Tcc.Configurations;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Services.Engenharia;
using Microsoft.EntityFrameworkCore;







var builder = WebApplication.CreateBuilder(args);

// ===== Serviços =====

// Controllers — habilita o uso de controllers na API
// JsonStringEnumConverter — permite enviar/receber enums como texto ("PC", "KG") em vez de números (0, 1)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// OpenAPI — gera o documento JSON que descreve todos os endpoints da API
builder.Services.AddOpenApi();

// Entity Framework + SQLite — ORM para acesso ao banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS — permite o frontend (React/Vite) fazer requisições para esta API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Services — registro dos serviços de negócio (injeção de dependência)
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<BomService>();






var app = builder.Build();

// ===== Pipeline de Middleware =====

// OpenAPI — expõe o documento JSON em /openapi/v1.json (apenas em Development)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Swagger UI — visualizador interativo da API em /swagger
app.UseSwaggerConfig();

// Scalar — visualizador moderno da API em /scalar/v1
app.UseScalarConfig();

// HTTPS — redireciona requisições HTTP para HTTPS
app.UseHttpsRedirection();

// CORS — aplica a política de acesso do frontend
app.UseCors("AllowFrontend");

// Authorization — habilita autenticação/autorização (preparado para JWT futuro)
app.UseAuthorization();

// Controllers — mapeia as rotas dos controllers
app.MapControllers();

// Inicia a aplicação
app.Run();