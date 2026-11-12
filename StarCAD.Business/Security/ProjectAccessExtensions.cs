using StarCAD.Common.Constants;
using StarCAD.Core.Security;
using StarCAD.Entity.Concrete;

namespace StarCAD.Business.Security;

/// <summary>
/// Firma bazlı görünürlük filtresi (ABAC / kaynak seviyesi yetkilendirme).
///
/// Filtre SORGUYA girer (WHERE), veri yüklendikten sonra ayıklanmaz. Sebep: biri
/// filtreyi uygulamayı unutursa sonuç "boş liste" olur, "başka firmanın verisi" değil —
/// yani hatanın yönü güvenli tarafa bakar.
///
/// Kullanıcının firma bağı için bkz. knowledge/single-role.md: bir kullanıcının ya
/// ProjectFirmId'si ya GasDistributionFirmId'si dolu olur, ikisi birden asla.
/// </summary>
public static class ProjectAccessExtensions
{
    public static IQueryable<Project> WhereVisibleTo(
        this IQueryable<Project> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(p => p.ProjectFirmAuthorization.ProjectFirmId == projectFirmId);
        }

        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(p => p.ProjectFirmAuthorization.GasDistributionFirmId == gasFirmId);
        }

        return query.Where(_ => false);
    }

    /// <summary>
    /// Kullanıcının kendi firma/rol scoping'ini (Admin/ProjectFirmId/GasDistributionFirmId)
    /// bir Elasticsearch term filter'ına çevirir — arama evrenini SQL'deki
    /// <see cref="WhereVisibleTo(IQueryable{Project}, ICurrentUserService)"/> ve benzerlerine
    /// paralel olarak daraltmak için (bkz. ProjectManager/UserManager.GetListAsync). Entity'ye
    /// özgü değil — yalnızca mevcut kullanıcının kendi durumuna bakar, bu yüzden birden fazla
    /// Manager'da paylaşılabilir. SQL tarafındaki filtre kaldırılmıyor, bu sadece ek bir katman.
    /// </summary>
    public static (string? Field, string? Value) GetSearchScope(this ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return (null, null);

        if (currentUser.ProjectFirmId.HasValue)
            return ("projectFirmId", currentUser.ProjectFirmId.Value.ToString());

        if (currentUser.GasDistributionFirmId.HasValue)
            return ("gasDistributionFirmId", currentUser.GasDistributionFirmId.Value.ToString());

        return (null, null);
    }

    public static IQueryable<ProjectVersion> WhereVisibleTo(
        this IQueryable<ProjectVersion> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(v => v.Project.ProjectFirmAuthorization.ProjectFirmId == projectFirmId);
        }

        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(v => v.Project.ProjectFirmAuthorization.GasDistributionFirmId == gasFirmId);
        }

        return query.Where(_ => false);
    }

    public static IQueryable<ProjectFirm> WhereVisibleTo(
        this IQueryable<ProjectFirm> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        // Proje firması kullanıcısı yalnızca kendi firmasını görür.
        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(f => f.Id == projectFirmId);
        }

        // Gaz dağıtım kullanıcısı, kendi firmasının bölgelerinde yetkili proje firmalarını görür
        // (Project görünürlüğüyle aynı mantık).
        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(f => f.ProjectFirmAuthorizations
                .Any(a => a.GasDistributionFirmId == gasFirmId));
        }

        return query.Where(_ => false);
    }
    
    public static IQueryable<User> WhereVisibleTo(
        this IQueryable<User> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(u => u.ProjectFirmId == projectFirmId);
        }

        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(u => u.GasDistributionFirmId == gasFirmId);
        }

        return query.Where(_ => false);
    }
    
    public static IQueryable<ProjectFirmAuthorization> WhereVisibleTo(
        this IQueryable<ProjectFirmAuthorization> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(a => a.ProjectFirmId == projectFirmId);
        }

        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(a => a.GasDistributionFirmId == gasFirmId);
        }

        return query.Where(_ => false);
    }

    public static IQueryable<Doc> WhereVisibleTo(
        this IQueryable<Doc> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        // Proje firması kullanıcısı: kendi firmasına ait tüm evrak (havuzdaki + projeye bağlı).
        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(d => d.ProjectFirmId == projectFirmId);
        }

        // Gaz dağıtım kullanıcısı: yalnızca kendi bölgesindeki projelere bağlı evrakı görür;
        // havuzdaki (ProjectId=null) evrak bir proje firmasının iç dosyalaması, onu görmez.
        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(d =>
                d.ProjectId != null && d.Project!.ProjectFirmAuthorization.GasDistributionFirmId == gasFirmId);
        }

        return query.Where(_ => false);
    }

    public static IQueryable<Policy> WhereVisibleTo(
        this IQueryable<Policy> query, ICurrentUserService currentUser)
    {
        if (currentUser.RoleCode == RoleCodes.Admin)
            return query;

        if (currentUser.ProjectFirmId.HasValue)
        {
            int projectFirmId = currentUser.ProjectFirmId.Value;
            return query.Where(p => p.Project.ProjectFirmAuthorization.ProjectFirmId == projectFirmId);
        }

        if (currentUser.GasDistributionFirmId.HasValue)
        {
            int gasFirmId = currentUser.GasDistributionFirmId.Value;
            return query.Where(p => p.Project.ProjectFirmAuthorization.GasDistributionFirmId == gasFirmId);
        }

        return query.Where(_ => false);
    }
}