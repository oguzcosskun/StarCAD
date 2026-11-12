using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Auth;

namespace StarCAD.Api.Controllers;

[Tags("Kimlik Doğrulama")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("api/auth/login")]
    [EndpointSummary("Kullanıcı adı ve şifre ile giriş yapar, JWT token döner")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPost("api/auth/register")]
    [EndpointSummary("Yeni kullanıcı oluşturur (yalnızca yönetici)")]
    [ProducesResponseType(typeof(RegisteredUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpGet("api/auth/me")]
    [EndpointSummary("Giriş yapmış kullanıcının bilgilerini döner")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(cancellationToken);
        return FromResult(result);
    }
    
    [Authorize]
    [HttpPost("api/auth/change-password")]
    [EndpointSummary("Giriş yapmış kullanıcının kendi şifresini değiştirir, yeni token döner")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ChangePasswordAsync(request, cancellationToken);
        return FromResult(result);
    }
    
    [Authorize]
    [HttpPost("api/auth/logout")]
    [EndpointSummary("Oturumu sonlandırır; kullanıcının tüm mevcut token'ları geçersiz olur")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(cancellationToken);
        return FromResult(result);
    }

}