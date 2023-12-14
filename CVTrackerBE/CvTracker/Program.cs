using System.Text;
using CvTracker.Models;
using CvTracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.MongoDB;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.MongoDB("mongodb+srv://user1:T3o6rLxvv7GHvPpJ@cv.y6ncygj.mongodb.net/JobTracker?authSource=admin", collectionName: "Logs")
    .CreateLogger();

builder.Host.UseSerilog();

var mongoClient = new MongoClient("mongodb+srv://user1:T3o6rLxvv7GHvPpJ@cv.y6ncygj.mongodb.net/?authSource=admin");
var database = mongoClient.GetDatabase("JobTracker");
var jobApplicationCollection = database.GetCollection<JobApplication>("JobApplications");
var userCollection = database.GetCollection<User>("User");

builder.Services.AddSingleton(jobApplicationCollection);
builder.Services.AddSingleton(userCollection);

builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("default", policyBuilder =>
    {
        policyBuilder.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policyBuilder.RequireAuthenticatedUser();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.Run();
