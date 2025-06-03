using System.Net.Http.Headers;
using Coursova.Core.Mapping;
using Coursova.Core;
using Coursova.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
var lichessToken = Environment.GetEnvironmentVariable("LICHESS_TOKEN")
                 ?? config["Lichess:Token"]; 

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPlayerInfoRepository, PlayerInfoRepository>();

builder.Services
    .AddHttpClient<ILichessService, LichessService>((sp, c) =>
    {
        c.BaseAddress = new Uri("https://lichess.org");

        c.DefaultRequestHeaders.Accept.Clear();
        c.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/x-ndjson"));

        c.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", lichessToken);

        c.DefaultRequestHeaders.UserAgent.ParseAdd(
            "CoursovaBot/1.0 (+mailto:you@example.com)");
    });

builder.Services.AddAutoMapper(typeof(LichessMappingProfile).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
