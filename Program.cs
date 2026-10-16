var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure session services.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Title shown on the browser tab
        options.DocumentTitle = "RaceDay API";

        // Hide the Schemas section at the bottom of the page
        options.DefaultModelsExpandDepth(-1);

        // Also hide it with a style rule, in case the setting above is ignored
        options.HeadContent = "<style>.swagger-ui section.models { display: none; }</style>";
    });
}

app.UseHttpsRedirection();

// Enable session before the controllers handle requests.
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();