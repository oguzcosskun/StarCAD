namespace StarCAD.ViewModel.Docs;

public static class DocSortKeys
{
    public const string FileName = "filename";
    public const string ReceivedAt = "receivedat";
}

public sealed class DocUploadDto
{
    // null = havuza yükleme (henüz bir projeye bağlanmamış).
    public int? ProjectId { get; init; }
    // Yalnızca ProjectId doluyken anlamlıdır — yüklerken tek bir birime ilk bağlantı kurar
    // (DocProjectUnit satırı). Ek/başka birimler POST /api/docs/{id}/units/{unitId} ile eklenir.
    public int? ProjectUnitId { get; init; }
    public int? DocTypeCodeId { get; init; }

    // Yalnızca ProjectId null iken (havuza yükleme) VE yükleyen Admin iken kullanılır — Admin'in
    // firma bağı olmadığı için hangi firmanın havuzuna yüklediğini belirtmesi gerekir. Firma
    // kullanıcısı gönderse bile yok sayılır (kendi firmasından başkasına yükleyemez).
    public int? ProjectFirmId { get; init; }

    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}

public sealed class DocAssignDto
{
    // null = havuza geri al. Proje değişince/havuza dönünce mevcut birim bağlantıları
    // (DocProjectUnit) temizlenir — eski projenin birimi yeni bağlamda anlamsız kalır.
    public int? ProjectId { get; init; }
}

public sealed class DocProjectUnitSummaryDto
{
    public int Id { get; init; }
    public string? UnitNumber { get; init; }
    public string? SubscriberNo { get; init; }
}

public sealed class DocListItemDto
{
    public int Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? ContentType { get; init; }
    public long? SizeBytes { get; init; }
    public int? DocTypeCodeId { get; init; }
    public string? DocTypeName { get; init; }
    public int? ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public List<DocProjectUnitSummaryDto> ProjectUnits { get; init; } = [];
    public int? ProjectFirmId { get; init; }
    public string? ProjectFirmName { get; init; }
    public DateTime? ReceivedAt { get; init; }
}

public sealed class DocDetailDto
{
    public int Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? ContentType { get; init; }
    public long? SizeBytes { get; init; }
    public int? DocTypeCodeId { get; init; }
    public string? DocTypeName { get; init; }
    public int? ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public List<DocProjectUnitSummaryDto> ProjectUnits { get; init; } = [];
    public int? ProjectFirmId { get; init; }
    public string? ProjectFirmName { get; init; }
    public int? UploadedByUserId { get; init; }
    public string? UploadedByUserFullName { get; init; }
    public DateTime? ReceivedAt { get; init; }
}

public sealed class DocDownloadDto
{
    public string Url { get; init; } = string.Empty;
}

public sealed class DocListQueryDto
{
    public int? ProjectId { get; init; }
    public int? ProjectUnitId { get; init; }
    public int? ProjectFirmId { get; init; }
    public int? DocTypeCodeId { get; init; }

    public DateTime? ReceivedFrom { get; init; }
    public DateTime? ReceivedTo { get; init; }

    // true: yalnızca havuzdaki (henüz bir projeye bağlanmamış) evrak listelenir.
    // false/verilmezse filtre uygulanmaz — kullanıcının görebildiği tüm evrak (havuz + projeye bağlı) listelenir.
    public bool PoolOnly { get; init; }

    public string SortBy { get; init; } = DocSortKeys.ReceivedAt;
    public string SortDir { get; init; } = "desc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}
