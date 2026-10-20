using System.Text.Json.Serialization;
using Flyrank.ImageRelevance.Api.Data;
using Flyrank.ImageRelevance.Api.Middleware;
using Flyrank.ImageRelevance.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddHttpClient<GeminiApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<GeminiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(90);
});
builder.Services.AddScoped<IImageAnalysisService>(serviceProvider =>
    serviceProvider.GetRequiredService<GeminiApiClient>());
builder.Services.AddScoped<IEmbeddingService>(serviceProvider =>
    serviceProvider.GetRequiredService<GeminiApiClient>());
builder.Services.AddSingleton<IImageRelevanceRepository, InMemoryImageRelevanceRepository>();
builder.Services.AddSingleton<IMismatchGuard, MismatchGuard>();
builder.Services.AddScoped<IImageRelevanceService, ImageRelevanceService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();
