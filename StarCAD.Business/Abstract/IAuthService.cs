using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Auth;

namespace StarCAD.Business.Abstract;

public interface IAuthService
{
    Task<IDataResult<LoginResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<IDataResult<RegisteredUserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<IDataResult<CurrentUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<IResult> LogoutAsync(CancellationToken cancellationToken = default);
    Task<IDataResult<LoginResponseDto>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}