using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using CvTracker.Helpers;
using CvTracker.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var filter = Builders<JobApplication>.Filter.Eq("UserId", userId);
        return await _jobApplicationCollection.Find(filter).ToListAsync();
    }
    public async Task<User> GetUserByIdAsync(string userId)
    {
        if (_mongoCollection == null)
        {
            throw new InvalidOperationException("MongoDB collection not initialized");
        }

        var objectId = new ObjectId(userId);

        var user = await _mongoCollection.Find(u => u.Id == objectId).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), $"User with ID {userId} not found");
        }

        return user;
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

    public async Task<string> LoginAsync(UserLoginModel userLoginModel)
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

        return accessTokenString;
    }
}