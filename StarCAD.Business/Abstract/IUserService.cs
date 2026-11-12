using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Users;

namespace StarCAD.Business.Abstract;

public interface IUserService
{
    Task<IDataResult<PagedResultDto<UserListItemDto>>> GetListAsync(UserListQueryDto query, CancellationToken cancellationToken = default);
    Task<IResult> ResetPasswordAsync(int userId, ResetPasswordRequestDto dto, CancellationToken cancellationToken = default);
    Task<IDataResult<UserListItemDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, UserUpdateDto dto, CancellationToken cancellationToken = default);
}