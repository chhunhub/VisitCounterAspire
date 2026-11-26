using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("cache")!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapGet("/hits", async (IConnectionMultiplexer redis) =>
{
    var value = await redis.GetDatabase().StringGetAsync("hits");
    return value.HasValue ? (long)value : 0L;
});

app.MapPost("/hit", async (IConnectionMultiplexer redis) =>
    await redis.GetDatabase().StringIncrementAsync("hits"));
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
