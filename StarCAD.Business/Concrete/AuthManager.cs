using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.Core.Security;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Auth;
using StarCAD.Common.Constants;

namespace StarCAD.Business.Concrete;

public class AuthManager : IAuthService
{
    private readonly StarCADContext _db;
    private readonly IConfiguration _config;
    private readonly ICurrentUserService _currentUser;

    public AuthManager(StarCADContext db, IConfiguration config, ICurrentUserService currentUser)
    {
        _db = db;
        _config = config;
        _currentUser = currentUser;
    }
    
    private LoginResponseDto BuildLoginResponse(
        int userId, string username, string email, string fullName,
        string roleCode, int tokenVersion, int? projectFirmId, int? gasDistributionFirmId)
    {
        var expireMinutes = int.Parse(_config["JWTAuth:Expire"]!);
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, roleCode),
            new(ClaimNames.TokenVersion, tokenVersion.ToString())
        };

        if (projectFirmId.HasValue)
            claims.Add(new Claim(ClaimNames.ProjectFirmId, projectFirmId.Value.ToString()));

        if (gasDistributionFirmId.HasValue)
            claims.Add(new Claim(ClaimNames.GasDistributionFirmId, gasDistributionFirmId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTAuth:SecretKey"]!));

        var token = new JwtSecurityToken(
            issuer: _config["JWTAuth:ValidIssuerURL"],
            audience: _config["JWTAuth:ValidAudienceURL"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            FullName = fullName,
            RoleCode = roleCode
        };
    }

    public async Task<IDataResult<LoginResponseDto>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Username == request.Username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FullName,
                u.PasswordHash,
                u.ProjectFirmId,
                u.TokenVersion,
                u.GasDistributionFirmId,
                RoleCode = u.Role.Code 
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new ErrorDataResult<LoginResponseDto>("Kullanıcı adı veya şifre hatalı.", StatusCodes.Status401Unauthorized);

        var response = BuildLoginResponse(
            user.Id, user.Username, user.Email, user.FullName,
            user.RoleCode, user.TokenVersion, user.ProjectFirmId, user.GasDistributionFirmId);

        return new SuccessDataResult<LoginResponseDto>(response, "Giriş başarılı.");
    }

    public async Task<IDataResult<RegisteredUserDto>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        var roleCode = request.RoleCode.Trim();

        var role = await _db.Roles
            .AsNoTracking()
            .Where(r => r.Code == roleCode)
            .Select(r => new { r.Id, r.Code })
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
            return new ErrorDataResult<RegisteredUserDto>("Seçilen rol bulunamadı.", StatusCodes.Status404NotFound);
        
        var usernameTaken = await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Username == username, cancellationToken);

        if (usernameTaken)
            return new ErrorDataResult<RegisteredUserDto>("Bu kullanıcı adı zaten kullanılıyor.", StatusCodes.Status409Conflict);

        var emailTaken = await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailTaken)
            return new ErrorDataResult<RegisteredUserDto>("Bu e-posta adresi zaten kullanılıyor.", StatusCodes.Status409Conflict);

        // 4) Firma gerçekten var mı?
        if (request.ProjectFirmId.HasValue)
        {
            var firmExists = await _db.ProjectFirms
                .AnyAsync(f => f.Id == request.ProjectFirmId.Value, cancellationToken);

            if (!firmExists)
                return new ErrorDataResult<RegisteredUserDto>("Seçilen proje firması bulunamadı.", StatusCodes.Status404NotFound);
        }

        if (request.GasDistributionFirmId.HasValue)
        {
            var firmExists = await _db.GasDistributionFirms
                .AnyAsync(f => f.Id == request.GasDistributionFirmId.Value, cancellationToken);

            if (!firmExists)
                return new ErrorDataResult<RegisteredUserDto>("Seçilen dağıtım firması bulunamadı.", StatusCodes.Status404NotFound);
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Username = username,
            Phone = request.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id,
            ProjectFirmId = request.ProjectFirmId,
            GasDistributionFirmId = request.GasDistributionFirmId,
            // SaveChanges override'ı yalnızca Project/ProjectVersion için CreatedAt yazıyor.
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesWithOutboxAsync(nameof(User), "Index", () => user.Id, cancellationToken);

        return new SuccessDataResult<RegisteredUserDto>(new RegisteredUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            RoleCode = role.Code
        }, "Kullanıcı oluşturuldu.");
    }
    
    
    public async Task<IDataResult<CurrentUserDto>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return new ErrorDataResult<CurrentUserDto>("Oturum bulunamadı.", StatusCodes.Status401Unauthorized);

        // Token'daki bilgi eskimiş olabilir (rol değişmiş olabilir) → DB'den taze oku.
        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == _currentUser.UserId.Value)
            .Select(u => new CurrentUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                RoleCode = u.Role.Code,
                RoleName = u.Role.Name,
                ProjectFirmId = u.ProjectFirmId,
                GasDistributionFirmId = u.GasDistributionFirmId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return new ErrorDataResult<CurrentUserDto>("Kullanıcı bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<CurrentUserDto>(user);
    }
    
    public async Task<IResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return new ErrorResult("Oturum bulunamadı.", StatusCodes.Status401Unauthorized);

        var affected = await _db.Users
            .Where(u => u.Id == _currentUser.UserId.Value)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.TokenVersion, u => u.TokenVersion + 1),
                cancellationToken);

        return affected == 0
            ? new ErrorResult("Kullanıcı bulunamadı.", StatusCodes.Status404NotFound)
            : new SuccessResult("Çıkış yapıldı.");
    }
    
    public async Task<IDataResult<LoginResponseDto>> ChangePasswordAsync(
        ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return new ErrorDataResult<LoginResponseDto>("Oturum bulunamadı.", StatusCodes.Status401Unauthorized);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

        if (user is null)
            return new ErrorDataResult<LoginResponseDto>("Kullanıcı bulunamadı.", StatusCodes.Status404NotFound);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return new ErrorDataResult<LoginResponseDto>("Mevcut şifre hatalı.", StatusCodes.Status400BadRequest);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.TokenVersion += 1;

        await _db.SaveChangesAsync(cancellationToken);

        var response = BuildLoginResponse(
            user.Id, user.Username, user.Email, user.FullName,
            user.Role.Code, user.TokenVersion, user.ProjectFirmId, user.GasDistributionFirmId);

        return new SuccessDataResult<LoginResponseDto>(response, "Şifre güncellendi.");
    }
}