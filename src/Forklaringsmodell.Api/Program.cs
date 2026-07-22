using Forklaringsmodell.Api.Middleware;
using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Options;
using Forklaringsmodell.Application.Repositories;
using Forklaringsmodell.Application.Services;
using Forklaringsmodell.Application.Validators;
using Forklaringsmodell.Infrastructure;
using Forklaringsmodell.Infrastructure.Repositories;
using Forklaringsmodell.Infrastructure.Seed;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core / SQLite
builder.Services.AddDbContext<ForklaringsmodellDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Forklaringsmodell")
        ?? "Data Source=forklaringsmodell.db"));

// Repository
builder.Services.AddScoped<IForklaringsmodellRepository, ForklaringsmodellRepository>();

// Konfidens-terskel-konfigurasjon (regel 3.3, se KonfidensTerskelOptions for dokumentasjon)
builder.Services.Configure<KonfidensTerskelOptions>(
    builder.Configuration.GetSection(KonfidensTerskelOptions.SectionName));

// FluentValidation-validatorer. Kun FluentValidation "core"-pakken er installert (ingen
// FluentValidation.DependencyInjectionExtensions/AspNetCore-adapter for automatisk
// scanning eller model-binding-validering), så hver validator registreres explisitt i
// DI og kalles manuelt fra service-laget (se f.eks. VurderingService).
builder.Services.AddScoped<IValidator<OpprettSakDto>, OpprettSakDtoValidator>();
builder.Services.AddScoped<IValidator<OppdaterSakDto>, OppdaterSakDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettKildeDto>, OpprettKildeDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettRettskildeDto>, OpprettRettskildeDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettRegelDto>, OpprettRegelDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettFaktumDto>, OpprettFaktumDtoValidator>();
builder.Services.AddScoped<IValidator<TransformerFaktumDto>, TransformerFaktumDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettVurderingDto>, OpprettVurderingDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettPartsmedvirkningDto>, OpprettPartsmedvirkningDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettVedtakDto>, OpprettVedtakDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettSakRelasjonDto>, OpprettSakRelasjonDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettVedtaksvirkningDto>, OpprettVedtaksvirkningDtoValidator>();
builder.Services.AddScoped<IValidator<OpprettVilkarDto>, OpprettVilkarDtoValidator>();

// Use-case/service-klasser (Application-laget)
builder.Services.AddScoped<SakService>();
builder.Services.AddScoped<SakRelasjonService>();
builder.Services.AddScoped<KildeService>();
builder.Services.AddScoped<RettskildeService>();
builder.Services.AddScoped<RegelService>();
builder.Services.AddScoped<VilkarService>();
builder.Services.AddScoped<FaktumService>();
builder.Services.AddScoped<VurderingService>();
builder.Services.AddScoped<PartsmedvirkningService>();
builder.Services.AddScoped<VedtakService>();

var app = builder.Build();

// Exception-handling middleware: mapper AppendOnlyViolationException -> 409,
// AppValidationException -> 400, NotFoundException -> 404.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Migrer og seed databasen automatisk i utviklingsmiljø hvis den er tom. Ikke noe
    // eget CLI-verktøy - holdes enkelt som instruert i oppgaven.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ForklaringsmodellDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Eksponert for WebApplicationFactory<Program> i integrasjonstester.
public partial class Program
{
}
