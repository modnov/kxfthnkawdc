using System.IO.Compression;
using kxfthnkawdc.Hub;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        });
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSingleton<ApplicationDbContext>();

var app = builder.Build();

app.MapHub<ChatHub>("chatsignalr");

app.UseSession();

app.MapControllers();

app.Use((context, next) =>
{
    if (context.Session.Keys.Contains("username") ||
        context.Request.Path.Value.StartsWith("/login") ||
        context.Request.Path.Value.EndsWith(".jpeg") ||
        context.Request.Path.Value.EndsWith(".ico") ||
        context.Request.Path.Value == "/")
    {
        int? clientId = context.Session.GetInt32("id");
        if (clientId != null)
        {
            context.Response.Headers.Add("client_id", clientId.ToString());
        }

        return next.Invoke();
    }

    context.Request.Path = "/login.html";
    context.Response.Redirect("/login.html", true);

    return next.Invoke();
});

app.UseRouting();

app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseResponseCompression();

app.UseWebSockets();

app.Run();
