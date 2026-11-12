using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Users;
using StarCAD.Common.Constants;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Kullanıcılar")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("api/users")]
    [EndpointSummary("Görünürlük kapsamındaki kullanıcıları listeler (rol/firma filtreli, sayfalı)")]
    [ProducesResponseType(typeof(PagedResultDto<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] UserListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _userService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }
    
    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPost("api/users/{id:int}/reset-password")]
    [EndpointSummary("Bir kullanıcının şifresini sıfırlar (yalnızca yönetici); kullanıcının tüm oturumları sonlanır")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.ResetPasswordAsync(id, dto, cancellationToken);
        return FromResult(result);
    }
    
    [HttpGet("api/users/{id:int}")]
    [EndpointSummary("Görünürlük kapsamındaki bir kullanıcının detayını getirir")]
    [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPut("api/users/{id:int}")]
    [EndpointSummary("Kullanıcıyı günceller (Admin herkesi, diğerleri yalnızca kendini; rol/firma yalnızca Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }
}