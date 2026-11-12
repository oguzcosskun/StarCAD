namespace StarCAD.ViewModel.Policies;

public sealed class PolicyAddDto
{
    public int ProjectUnitId { get; init; }
    public int? InsuranceCompanyId { get; init; }
    public string? PolicyNumber { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed class PolicyUpdateDto
{
    public int? InsuranceCompanyId { get; init; }
    public string? PolicyNumber { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed class PolicyListQueryDto
{
    // null = proje-bağımsız "tüm poliçeler" listesi (WhereVisibleTo kapsamında).
    public int? ProjectId { get; init; }
    public int? InsuranceCompanyId { get; init; }
    public bool ExcludeUnitDeleted { get; init; }

    // Poliçe numarası, birim numarası veya abone numarasında geçen metne göre arama (contains).
    public string? Search { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}

// Liste ve tekil detay için tek DTO — iki görünüm de aynı alanları taşıyor, ayrı tutmak
// yalnızca birebir kopya iki sınıf üretirdi.
public sealed class PolicyDto
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public int? ProjectUnitId { get; init; }
    public string? UnitNumber { get; init; }
    public int? InsuranceCompanyId { get; init; }
    public string? InsuranceCompanyTitle { get; init; }
    public string? PolicyNumber { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsActive { get; init; }
    public bool IsUnitDeleted { get; init; }
}
