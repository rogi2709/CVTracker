using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using CvTracker.Helpers;
using CvTracker.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace CvTracker.Services;

public class UserService:IUserService
{
    private readonly IMongoCollection<User> _mongoCollection;
    private readonly IConfiguration _configuration;
    private readonly IMongoCollection<JobApplication> _jobApplicationCollection;

    public UserService(IMongoCollection<User> mongoCollection, IConfiguration configuration, IMongoCollection<JobApplication> jobApplicationCollection)
    {
        _mongoCollection = mongoCollection;
        _configuration = configuration;
        _jobApplicationCollection = jobApplicationCollection;
    }
    
    public async Task<List<JobApplication>> GetJobApplicationsForUserAsync(string userId)
    {
        try
        {
            Log.Information("Getting job applications for user with ID {UserId}", userId);

            var filter = Builders<JobApplication>.Filter.Eq("UserId", userId);
            var jobApplications = await _jobApplicationCollection.Find(filter).ToListAsync();

            Log.Information("Returning {Count} job applications for user with ID {UserId}", jobApplications.Count, userId);

            return jobApplications;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting job applications for user with ID {UserId}", userId);
            throw;
        }
    }
    public async Task<User> GetUserByIdAsync(string userId)
    {
        try
        {
            Log.Information("Getting user by ID {UserId}", userId);

            if (_mongoCollection == null)
            {
                throw new InvalidOperationException("MongoDB collection not initialized");
            }

            var objectId = new ObjectId(userId);
            var user = await _mongoCollection.Find(u => u.Id == objectId).FirstOrDefaultAsync();

            if (user == null)
            {
                Log.Warning("User with ID {UserId} not found", userId);
                throw new ArgumentNullException(nameof(user), $"User with ID {userId} not found");
            }

            Log.Information("Returning user with ID {UserId}", userId);

            return user;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting user with ID {UserId}", userId);
            throw;
        }
    }
    public async Task<UserOutputModel> CreateUserAsync(UserInputModel userInput)
    {
        if (userInput == null)
        {
            throw new ArgumentNullException(nameof(userInput));
        }
        
        if (string.IsNullOrEmpty(userInput.InputPwd))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(userInput.InputPwd));
        }
        
        if (string.IsNullOrEmpty(userInput.Email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(userInput.Email));
        }
        var emailRegex = new Regex(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");
        if (!emailRegex.IsMatch(userInput.Email))
        {
            throw new ArgumentException("Email is not valid.", nameof(userInput.Email));
        }

        if (_mongoCollection == null)
        {
            throw new InvalidOperationException("Database not initialized.");
        }

        var existingUserCursor = await _mongoCollection.FindAsync(u => u.Username == userInput.Username || u.Email == userInput.Email);
        if (existingUserCursor.Any())
        {
            throw new InvalidOperationException("A user with the same username or email already exists.");
        }
        var salt = PasswordHelper.GenerateRandomSalt();
        User newUser = new User()
        {
            Username = userInput.Username,
            Email = userInput.Email,
            Salt = salt,
            PasswordHash = PasswordHelper.GenerateHashedPwd(userInput.InputPwd, salt)
        };

       await _mongoCollection.InsertOneAsync(newUser);

       UserOutputModel response = new UserOutputModel()
       {
           Username = newUser.Username,
           Email = newUser.Email
       };
       return response;
    }

    public async Task<(string AccessToken, string RefreshToken)> LoginAsync(UserLoginModel userLoginModel)
    {
        if (_mongoCollection == null)
        {
            throw new InvalidOperationException("MongoDB collection not initialized");
        }

        var user = await _mongoCollection.Find(u => u.Username == userLoginModel.Username).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User not found");
        }

        if (PasswordHelper.VerifyPassword(userLoginModel.InputPwd, user.PasswordHash) is false)
        {
            throw new InvalidOperationException("Password is wrong");
        }
        
        var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        
        await _mongoCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessTokenString = tokenHandler.WriteToken(accessToken);

        return (accessTokenString, refreshToken);
    }
    
    public async Task<(string AccessToken, string RefreshToken)> RefreshAccessTokenAsync(string refreshToken)
    {
        var user = await _mongoCollection.Find(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User not found or refresh token invalid");
        }
        
        var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
        
        user.RefreshToken = newRefreshToken;

        await _mongoCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(120), 
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessTokenString = tokenHandler.WriteToken(accessToken);

        return (accessTokenString, newRefreshToken);
    }


    
    

}