using System.IO.Compression;
using kxfthnkawdc.Hub;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Mvc;

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
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSingleton<ApplicationDbContext>();

var app = builder.Build();

app.UseSession();

app.MapControllers();

app.MapHub<ChatHub>("chatsignalr");

app.Use((context, next) =>
{
    if (app.Environment.IsDevelopment())
    {
        context.Session.SetInt32("id", 2);
        context.Session.SetString("username", "kxfthnkawdc");
        context.Session.CommitAsync();
    }
    
    if (context.Session.Keys.Contains("username"))
    {
        int? clientId = context.Session.GetInt32("id");
        if (clientId != null)
        {
            context.Response.Headers.Add("client_id", clientId.ToString());
        }

        return next.Invoke();
    }

    context.Response.Headers.Add("client_id", "0");

    if (context.Request.Path == "/chat.html")
    {
        context.Request.Path = "/login.html";
        context.Response.Redirect("/login.html", true);
    }

    return next.Invoke();
});

app.UseRouting();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseResponseCompression();

app.MapFallbackToFile("index.html");

app.Run();
