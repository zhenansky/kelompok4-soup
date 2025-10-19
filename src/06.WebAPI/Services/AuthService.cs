using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MyApp.WebAPI.Services.Interfaces;
using MyApp.WebAPI.Configuration;
using MyApp.WebAPI.Exceptions;
using MyApp.WebAPI.DTOs.Auth;
using MyApp.WebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace MyApp.WebAPI.Services
{
  public class AuthService : IAuthService
  {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;


    public AuthService(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      JwtSettings jwtSettings,
      ITokenService tokenService,
      IEmailService emailService,
      ILogger<AuthService> logger)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _jwtSettings = jwtSettings;
      _tokenService = tokenService;
      _emailService = emailService;
      _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
      _logger.LogInformation("Register attempt for {Email}", request.Email);

      var existingUser = await _userManager.FindByEmailAsync(request.Email);
      if (existingUser != null)
      {
        _logger.LogWarning("Registration failed: email {Email} already exists", request.Email);
        throw new ValidationException("Email already registered.");
      }


      if (request.Password != request.ConfirmPassword)
        throw new ValidationException("Passwords do not match.");

      var user = new User
      {
        UserName = request.Email,
        Email = request.Email,
        Name = request.Name,
        Status = UserStatus.Active,
        EmailConfirmed = false
      };

      var result = await _userManager.CreateAsync(user, request.Password);
      if (!result.Succeeded)
      {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("User registration failed for {Email}: {Errors}", request.Email, errors);
        throw new ValidationException($"Registration failed: {errors}");
      }

      await _userManager.AddToRoleAsync(user, "User");

      // Generate email confirmation token
      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      var encodedToken = Uri.EscapeDataString(token);

      // Create confirmation link (adjust to your frontend domain)
      var confirmLink = $"http://localhost:5099/api/Auth/confirm-email?email={request.Email}&token={encodedToken}";

      // Send confirmation email
      var subject = "Confirm Your Email - Soup";
      var body = $@"
        <p>Hello {user.Name},</p>
        <p>Thank you for registering! Please confirm your email by clicking the link below:</p>
        <p><a href='{confirmLink}' target='_blank'>Confirm Email</a></p>
        <p>If you did not register, please ignore this email.</p>
        <br/>
        <p>Regards,<br/>Soup Team</p>
      ";

      await _emailService.SendEmailAsync(user.Email!, subject, body);

      _logger.LogInformation("Registration successful for {Email}, confirmation email sent.", request.Email);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Registration successful! Please check your email to confirm your account."
      };
    }

    public async Task<string> ConfirmEmailAsync(string email, string token)
    {
      _logger.LogInformation("Email confirmation attempt for {Email}", email);

      var user = await _userManager.FindByEmailAsync(email);
      if (user == null)
      {
        _logger.LogWarning("Email confirmation failed: user not found ({Email})", email);
        return "http://localhost:5124/email-success?status=notfound";
      }

      var result = await _userManager.ConfirmEmailAsync(user, token);
      if (!result.Succeeded)
      {
        _logger.LogError("Email confirmation failed for {Email}", email);
        return "http://localhost:5124/email-success?status=failed";
      }

      await _userManager.UpdateAsync(user);

      _logger.LogInformation("Email confirmed successfully for {Email}", email);

      return "http://localhost:5124/email-success?status=success";
    }

    public async Task<AuthResponseDto> ResendConfirmationEmailAsync(string email)
    {
      _logger.LogInformation("Resend confirmation email to {Email}", email);

      var user = await _userManager.FindByEmailAsync(email);
      if (user == null)
      {
        _logger.LogWarning("Resend confirmation email failed: user not found ({Email})", email);
        throw new NotFoundException("User not found.");
      }

      if (user.EmailConfirmed)
      {
        _logger.LogWarning("Resend confirmation email failed: Email is already confirmed. ({Email})", email);
        throw new ValidationException("Email is already confirmed.");
      }

      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      var encodedToken = Uri.EscapeDataString(token);
      var confirmLink = $"http://localhost:5099/api/Auth/confirm-email?email={email}&token={encodedToken}";

      var subject = "Confirm Your Email - Soup";
      var body = $@"
        <p>Hello {user.Name},</p>
        <p>Please confirm your email by clicking the link below:</p>
        <p><a href='{confirmLink}' target='_blank'>Confirm Email</a></p>
        <p>Thank you!</p>
    ";

      await _emailService.SendEmailAsync(user.Email!, subject, body);

      _logger.LogInformation("Resend confirmation email successful, confirmation email sent to  {Email}", email);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Email confirmation send successful."
      };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
      _logger.LogInformation("Login attempt for {Email}", request.Email);

      var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
      if (user == null)
      {
        _logger.LogWarning("Login failed for {Email}: user not found", request.Email);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Invalid email or password"
        };
      }

      if (user.EmailConfirmed == false)
      {
        _logger.LogWarning("Login failed for {Email}: email not confirmed", request.Email);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Your email is not confirmed, please check your email or resend email confirmation."
        };
      }

      if (user.Status == UserStatus.Inactive)
      {
        _logger.LogWarning("Login failed for {Email}: email is inactive", request.Email);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Your account is inactive please contact admin at ptsoupco@gmail.com to activate."
        };
      }

      var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
      if (!result.Succeeded)
      {
        _logger.LogWarning("Login failed for {Email}: invalid password", request.Email);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Invalid email or password"
        };
      }

      var accessToken = await _tokenService.CreateTokenAsync(user);
      var refreshToken = _tokenService.GenerateRefreshToken();

      // Save refresh token
      user.RefreshToken = refreshToken;
      user.ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
      await _userManager.UpdateAsync(user);

      _logger.LogInformation("Login successful for {Email}", request.Email);

      var roles = await _userManager.GetRolesAsync(user);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Login successful",
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = user.ExpiresAt,
        Email = user.Email!,
        Name = user.Name,
        Role = roles.FirstOrDefault() ?? "User"
      };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
      _logger.LogInformation("Refresh token attempt.");

      // Extract claims dari expired access token
      var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
      if (principal == null)
      {
        _logger.LogWarning("Invalid access token during refresh.");
        return new AuthResponseDto
        {
          Success = false,
          Message = "Invalid access token"
        };
      }

      var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
      if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
      {
        _logger.LogWarning("Invalid token claims during refresh.");
        return new AuthResponseDto
        {
          Success = false,
          Message = "Invalid token claims"
        };
      }

      // Validasi refresh token
      var user = await _userManager.FindByIdAsync(userId.ToString());
      if (user == null || user.Status == UserStatus.Inactive || user.RefreshToken != request.RefreshToken ||
          user.ExpiresAt <= DateTime.UtcNow)
      {
        _logger.LogWarning("Invalid refresh token for user ID {UserId}", userId);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Invalid refresh token"
        };
      }

      // Generate tokens baru
      var accessToken = await _tokenService.CreateTokenAsync(user);
      var refreshToken = _tokenService.GenerateRefreshToken();

      // Update refresh token
      user.RefreshToken = refreshToken;
      user.ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
      await _userManager.UpdateAsync(user);

      _logger.LogInformation("Token refreshed successfully for {Email}", user.Email);

      var roles = await _userManager.GetRolesAsync(user);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Token refreshed successfully",
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = user.ExpiresAt,
        Email = user.Email!,
        Name = user.Name,
        Role = roles.FirstOrDefault() ?? "User"
      };
    }

    public async Task<bool> LogoutAsync(string email)
    {
      _logger.LogInformation("Logout attempt for {Email}", email);

      var user = await _userManager.FindByEmailAsync(email);
      if (user != null)
      {
        user.RefreshToken = null;
        user.ExpiresAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        _logger.LogInformation("Logout successful for {Email}", email);
        return true;
      }

      _logger.LogWarning("Logout failed: user not found ({Email})", email);
      return false;
    }

    public async Task<AuthResponseDto> SendResetPasswordEmailAsync(ForgotPasswordRequestDto request)
    {
      _logger.LogInformation("Password reset email request for {Email}", request.Email);

      var user = await _userManager.FindByEmailAsync(request.Email);
      if (user == null)
      {
        _logger.LogWarning("Password reset failed: user not found ({Email})", request.Email);
        throw new NotFoundException("Email not registered.");
      }

      // Generate reset password token
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      var encodedToken = Uri.EscapeDataString(token);

      var resetLink = $"http://localhost:5124/new-password?email={request.Email}&token={encodedToken}";

      var subject = "Reset Your Password - Soup";
      var body = $@"
        <p>Hello {user.Name},</p>
        <p>We received a request to reset your password. Click the link below to reset it:</p>
        <p><a href='{resetLink}' target='_blank'>Reset Password</a></p>
        <p>If you did not request this, please ignore this email.</p>
        <br />
        <p>Regards,<br/>Soup Team</p>
      ";

      await _emailService.SendEmailAsync(user.Email!, subject, body);
      _logger.LogInformation("Password reset email sent to {Email}", request.Email);

      return new AuthResponseDto
      {
        Success = true,
        Message = "A password reset link has been sent to your email."
      };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
      _logger.LogInformation("Reset password attempt for {Email}", request.Email);

      var user = await _userManager.FindByEmailAsync(request.Email);
      if (user == null)
      {
        _logger.LogWarning("Reset password failed: user not found ({Email})", request.Email);
        throw new NotFoundException("Email not registered.");
      }

      var isSamePassword = await _userManager.CheckPasswordAsync(user, request.NewPassword);
      if (isSamePassword)
        throw new ValidationException("New password cannot be the same as your old password.");

      if (request.NewPassword != request.ConfirmNewPassword)
        throw new ValidationException("Passwords do not match.");

      var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
      if (!result.Succeeded)
      {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Password reset failed for {Email}: {Errors}", request.Email, errors);
        throw new ValidationException("RESET_FAILED", errors);
      }

      user.RefreshToken = null;
      user.ExpiresAt = DateTime.UtcNow;
      await _userManager.UpdateAsync(user);

      _logger.LogInformation("Password reset successful for {Email}", request.Email);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Password has been reset successfully."
      };
    }
  }
}
