using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Extensions;
using StarCAD.Business.Security;
using StarCAD.Core.Search;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Users;
using StarCAD.Common.Constants;

namespace StarCAD.Business.Concrete;

public class UserManager : IUserService
{
    private readonly StarCADContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISearchService _searchService;

    public UserManager(StarCADContext db, ICurrentUserService currentUser, ISearchService searchService)
    {
        _db = db;
        _currentUser = currentUser;
        _searchService = searchService;
    }

    public async Task<IDataResult<PagedResultDto<UserListItemDto>>> GetListAsync(
        UserListQueryDto query, CancellationToken cancellationToken = default)
    {
        var users = _db.Users.AsNoTracking().WhereVisibleTo(_currentUser);

        if (!string.IsNullOrWhiteSpace(query.RoleCode))
        {
            var roleCode = query.RoleCode.Trim();
            users = users.Where(u => u.Role.Code == roleCode);
        }

        if (query.ProjectFirmId is { } projectFirmId)
            users = users.Where(u => u.ProjectFirmId == projectFirmId);

        if (query.GasDistributionFirmId is { } gasFirmId)
            users = users.Where(u => u.GasDistributionFirmId == gasFirmId);

        if (query.GasDistributionGroupId is { } groupId)
            users = users.Where(u =>
                (u.ProjectFirmId != null && u.ProjectFirm!.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirm.GroupId == groupId)) ||
                (u.GasDistributionFirmId != null && u.GasDistributionFirm!.GroupId == groupId));

        var (scopeField, scopeValue) = _currentUser.GetSearchScope();
        users = await users.ApplyTextSearchAsync(_searchService, "users", query.Q, scopeField, scopeValue, cancellationToken);

        var totalCount = await users.CountAsync(cancellationToken);

        users = ApplySort(users, query.SortBy, query.SortDir);

        const int maxPageSize = 100;
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Username = u.Username,
                Phone = u.Phone,
                RoleCode = u.Role.Code,
                RoleName = u.Role.Name,
                ProjectFirmId = u.ProjectFirmId,
                ProjectFirmName = u.ProjectFirm != null ? u.ProjectFirm.Title : null,
                GasDistributionFirmId = u.GasDistributionFirmId,
                GasDistributionFirmName = u.GasDistributionFirm != null ? u.GasDistributionFirm.Title : null,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<UserListItemDto>>(new PagedResultDto<UserListItemDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        });
    }

    private static IQueryable<User> ApplySort(IQueryable<User> users, string sortBy, string sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            UserSortKeys.FullName => descending
                ? users.OrderByDescending(u => u.FullName)
                : users.OrderBy(u => u.FullName),
            _ => descending
                ? users.OrderByDescending(u => u.CreatedAt)
                : users.OrderBy(u => u.CreatedAt)
        };
    }
    
    public async Task<IResult> ResetPasswordAsync(
        int userId, ResetPasswordRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.WhereVisibleTo(_currentUser)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return new ErrorResult("Kullanıcı bulunamadı.", StatusCodes.Status404NotFound);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.TokenVersion += 1;

        await _db.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Şifre sıfırlandı.");
    }
    
    public async Task<IDataResult<UserListItemDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().WhereVisibleTo(_currentUser)
            .Where(u => u.Id == id)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Username = u.Username,
                Phone = u.Phone,
                RoleCode = u.Role.Code,
                RoleName = u.Role.Name,
                ProjectFirmId = u.ProjectFirmId,
                ProjectFirmName = u.ProjectFirm != null ? u.ProjectFirm.Title : null,
                GasDistributionFirmId = u.GasDistributionFirmId,
                GasDistributionFirmName = u.GasDistributionFirm != null ? u.GasDistributionFirm.Title : null,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return new ErrorDataResult<UserListItemDto>("Kullanıcı bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<UserListItemDto>(user);
    }
    
    public async Task<IResult> UpdateAsync(int id, UserUpdateDto dto, CancellationToken cancellationToken = default)
{
    var isAdmin = _currentUser.RoleCode == RoleCodes.Admin;
    var isSelf = _currentUser.UserId == id;

    if (!isAdmin && !isSelf)
        return new ErrorResult("Bu kullanıcıyı düzenleme yetkiniz yok.", StatusCodes.Status403Forbidden);

    if (!isAdmin && (dto.RoleCode is not null || dto.ProjectFirmId is not null || dto.GasDistributionFirmId is not null))
        return new ErrorResult("Rol ve firma bilgisini yalnızca yönetici değiştirebilir.", StatusCodes.Status403Forbidden);

    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    if (user is null)
        return new ErrorResult("Kullanıcı bulunamadı.", StatusCodes.Status404NotFound);

    var email = dto.Email.Trim().ToLowerInvariant();

    var emailTaken = await _db.Users.IgnoreQueryFilters()
        .AnyAsync(u => u.Id != id && u.Email == email, cancellationToken);
    if (emailTaken)
        return new ErrorResult("Bu e-posta adresi zaten kullanılıyor.", StatusCodes.Status409Conflict);

    user.FullName = dto.FullName.Trim();
    user.Email = email;
    user.Phone = dto.Phone.Trim();

    if (isAdmin && dto.RoleCode is not null)
    {
        var role = await _db.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == dto.RoleCode, cancellationToken);

        if (role is null)
            return new ErrorResult("Seçilen rol bulunamadı.", StatusCodes.Status404NotFound);

        if (dto.ProjectFirmId.HasValue)
        {
            var projectFirmExists = await _db.ProjectFirms.AsNoTracking()
                .AnyAsync(f => f.Id == dto.ProjectFirmId.Value, cancellationToken);

            if (!projectFirmExists)
                return new ErrorResult("Seçilen proje firması bulunamadı.", StatusCodes.Status404NotFound);
        }

        if (dto.GasDistributionFirmId.HasValue)
        {
            var gasFirmExists = await _db.GasDistributionFirms.AsNoTracking()
                .AnyAsync(f => f.Id == dto.GasDistributionFirmId.Value, cancellationToken);

            if (!gasFirmExists)
                return new ErrorResult("Seçilen dağıtım firması bulunamadı.", StatusCodes.Status404NotFound);
        }

        user.RoleId = role.Id;
        user.ProjectFirmId = dto.ProjectFirmId;
        user.GasDistributionFirmId = dto.GasDistributionFirmId;
    }

    await _db.SaveChangesWithOutboxAsync(nameof(User), "Index", () => user.Id, cancellationToken);

    return new SuccessResult("Kullanıcı güncellendi.");
}
}