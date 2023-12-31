﻿using CvTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CvTracker.Services;

public interface IUserService
{
    Task<List<JobApplication>> GetJobApplicationsForUserAsync(string userId);
    Task<User> GetUserByIdAsync(string id);
    Task<UserOutputModel> CreateUserAsync(UserInputModel userInputModel);
    Task<(string AccessToken, string RefreshToken)> LoginAsync(UserLoginModel userLoginModel);
    Task<(string AccessToken, string RefreshToken)> RefreshAccessTokenAsync(string refreshToken);
}
