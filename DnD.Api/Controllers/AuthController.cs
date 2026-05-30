using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOAuthService _oauthService;

    public AuthController(IAuthService authService, IOAuthService oauthService)
    {
        _authService = authService;
        _oauthService = oauthService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var result = await _authService.RegisterAsync(model);
        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { token = result.Token, message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var result = await _authService.LoginAsync(model);
        if (!result.IsSuccess)
            return Unauthorized(new { errors = result.Errors });

        return Ok(new { token = result.Token });
    }

    /// <summary>
    /// Get Google OAuth authorization URL
    /// </summary>
    [HttpGet("oauth/google/authorize")]
    public IActionResult GetGoogleAuthorizationUrl([FromQuery] string? redirectUri)
    {
        var state = Guid.NewGuid().ToString();
        var callbackUri = redirectUri ?? "http://localhost:3000/auth/google/callback";
        var authUrl = _oauthService.GetGoogleAuthorizationUrl(state, callbackUri);
        
        return Ok(new { authUrl, state });
    }

    /// <summary>
    /// Get Discord OAuth authorization URL
    /// </summary>
    [HttpGet("oauth/discord/authorize")]
    public IActionResult GetDiscordAuthorizationUrl([FromQuery] string? redirectUri)
    {
        var state = Guid.NewGuid().ToString();
        var callbackUri = redirectUri ?? "http://localhost:3000/auth/discord/callback";
        var authUrl = _oauthService.GetDiscordAuthorizationUrl(state, callbackUri);
        
        return Ok(new { authUrl, state });
    }

    /// <summary>
    /// Handle Google OAuth callback
    /// </summary>
    [HttpPost("oauth/google/callback")]
    public async Task<IActionResult> GoogleCallback([FromBody] OAuthRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.RedirectUri))
            return BadRequest(new { errors = new[] { "Code and redirect URI are required" } });

        var userInfo = await _oauthService.GetGoogleUserInfoAsync(request.Code, request.RedirectUri);
        if (userInfo == null)
            return BadRequest(new { errors = new[] { "Failed to retrieve user information from Google" } });

        var result = await _oauthService.HandleOAuthCallbackAsync(userInfo);
        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            token = result.Token,
            isNewUser = result.IsNewUser,
            email = result.Email,
            displayName = result.DisplayName,
            message = result.IsNewUser ? "User registered successfully" : "User logged in successfully"
        });
    }

    /// <summary>
    /// Handle Discord OAuth callback
    /// </summary>
    [HttpPost("oauth/discord/callback")]
    public async Task<IActionResult> DiscordCallback([FromBody] OAuthRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.RedirectUri))
            return BadRequest(new { errors = new[] { "Code and redirect URI are required" } });

        var userInfo = await _oauthService.GetDiscordUserInfoAsync(request.Code, request.RedirectUri);
        if (userInfo == null)
            return BadRequest(new { errors = new[] { "Failed to retrieve user information from Discord" } });

        var result = await _oauthService.HandleOAuthCallbackAsync(userInfo);
        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            token = result.Token,
            isNewUser = result.IsNewUser,
            email = result.Email,
            displayName = result.DisplayName,
            message = result.IsNewUser ? "User registered successfully" : "User logged in successfully"
        });
    }
}
