//http://localhost:7000/swagger/index.html
//http://localhost:7000/scalar/


using Microsoft.EntityFrameworkCore;

using Api_ArjSys_Tcc.Configurations;
using Api_ArjSys_Tcc.Data;

using Api_ArjSys_Tcc.Services.Engenharia;
using Api_ArjSys_Tcc.Services.Admin;
using Api_ArjSys_Tcc.Services.Comercial;
using Api_ArjSys_Tcc.Services.Producao;




var builder = WebApplication.CreateBuilder(args);



// ===== Serviços =====

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddOpenApiConfig();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Engenharia
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<BomService>();
builder.Services.AddScoped<GrupoProdutoService>();
builder.Services.AddScoped<GrupoVinculoService>();
builder.Services.AddScoped<ConfiguracaoEngenhariaService>();
builder.Services.AddScoped<PathDocumentosService>();


// Admin
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<FuncionarioService>();
builder.Services.AddScoped<PermissaoService>();
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddScoped<AuthService>();


// Comercial
builder.Services.AddScoped<PedidoVendaService>();
builder.Services.AddScoped<PedidoVendaItemService>();
builder.Services.AddScoped<NumeroSerieService>();


// Produção
builder.Services.AddScoped<OrdemProducaoService>();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwaggerConfig();
app.UseScalarConfig();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();
