using System.Text;
using CvTracker.Models;
using CvTracker.Services;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
var mongoClient = new MongoClient("mongodb+srv://user1:T3o6rLxvv7GHvPpJ@cv.y6ncygj.mongodb.net/?authSource=admin");
var database = mongoClient.GetDatabase("JobTracker");
var jobApplicationCollection = database.GetCollection<JobApplication>("JobApplications");
var UserCollection = database.GetCollection<User>("User");

builder.Services.AddSingleton<IMongoCollection<JobApplication>>(jobApplicationCollection);
builder.Services.AddSingleton<IMongoCollection<User>>(UserCollection);

builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("default", policyBuilder =>
    {
        policyBuilder.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policyBuilder.RequireAuthenticatedUser();
    } );
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();