using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projApiMongoDB.Api.Repositories;
using projApiMongoDB.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration binding for MongoDB settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

// --- Register MongoClient as singleton (thread-safe)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});

// Register repository
builder.Services.AddScoped<IInfectadoRepository, InfectadoRepository>();

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging if necessary (default is fine for most cases)

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
