using Data.Models;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.Extensions.FileProviders;
using DBS.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddAutoMapper();
builder.Services.ConfigIdentityService();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddBussinessService(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.ConfigHangFire(builder.Configuration);
builder.Services.AddJWTAuthentication(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"]);

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin()
        .WithOrigins("http://localhost:3000", "https://ims.hisoft.vn"
        );
}));

builder.Services.AddSignalR();

    var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory() + "/" + "wwwroot")
});

app.MapControllers();

app.Services.ApplyPendingMigrations();

// config hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    AppPath = null,
    DashboardTitle = "Hangfire CMS",
    Authorization = new[]
    {
        new HangfireCustomBasicAuthenticationFilter
        {
            User = "admin",
            Pass = "password123@"
        }
    }
});

app.UseForwardedHeaders();

app.Run();
