using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.WebAPI.DTOs.Auth;
using MyApp.WebAPI.Services.Interfaces;
using MyApp.WebAPI.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace MyApp.WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
      _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto dto)
    {
      var result = await _authService.RegisterAsync(dto);
      if (result.Success)
      {
        return Ok(new ApiResponse<AuthResponseDto>
        {
          Success = true,
          Data = result,
          Message = result.Message
        });
      }

      return BadRequest(new ApiResponse<AuthResponseDto>
      {
        Success = false,
        Data = result,
        Message = result.Message
      });
    }

    [HttpPost("resend-confirmation-email")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ResendConfirmationEmail([FromBody] string email)
    {
      var result = await _authService.ResendConfirmationEmailAsync(email);
      if (result.Success)
      {
        return Ok(new ApiResponse<AuthResponseDto>
        {
          Success = true,
          Data = result,
          Message = result.Message
        });
      }

      return BadRequest(new ApiResponse<AuthResponseDto>
      {
        Success = false,
        Data = result,
        Message = result.Message
      });
    }

    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
      var redirectUrl = await _authService.ConfirmEmailAsync(email, token);
      return Redirect(redirectUrl);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto dto)
    {
      var result = await _authService.LoginAsync(dto);
      if (result.Success)
      {
        return Ok(new ApiResponse<AuthResponseDto>
        {
          Success = true,
          Data = result,
          Message = "Login successful"
        });
      }

      return BadRequest(new ApiResponse<AuthResponseDto>
      {
        Success = false,
        Data = result,
        Message = result.Message
      });
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
      var result = await _authService.RefreshTokenAsync(dto);

      if (result.Success)
      {
        return Ok(new ApiResponse<AuthResponseDto>
        {
          Success = true,
          Data = result,
          Message = "Token refreshed successfully"
        });
      }

      return BadRequest(new ApiResponse<AuthResponseDto>
      {
        Success = false,
        Data = result,
        Message = result.Message
      });

    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
      var email = User.FindFirst(ClaimTypes.Email)?.Value
      ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value; ;

      if (string.IsNullOrEmpty(email))
      {
        return BadRequest(new ApiResponse<bool>
        {
          Success = false,
          Data = false,
          Message = "Invalid token: email not found."
        });
      }

      var result = await _authService.LogoutAsync(email);
      return Ok(new ApiResponse<bool>
      {
        Success = result,
        Data = result,
        Message = result ? "Logout successful" : "Logout failed"
      });
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
      var result = await _authService.SendResetPasswordEmailAsync(dto);
      if (result.Success)
      {
        return Ok(new ApiResponse<AuthResponseDto>
        {
          Success = true,
          Data = result,
          Message = result.Message
        });
      }

      return BadRequest(new ApiResponse<AuthResponseDto>
      {
        Success = false,
        Data = result,
        Message = result.Message
      });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
      var result = await _authService.ResetPasswordAsync(request);
      if (result.Success)
      {
        return Ok(new ApiResponse<AuthResponseDto>
        {
          Success = true,
          Data = result,
          Message = result.Message
        });
      }

      return BadRequest(new ApiResponse<AuthResponseDto>
      {
        Success = false,
        Data = result,
        Message = result.Message
      });
    }
  }
}
