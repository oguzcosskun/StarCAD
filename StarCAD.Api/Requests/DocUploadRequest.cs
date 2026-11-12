namespace StarCAD.Api.Requests;

// multipart/form-data ile bind edilir. IFormFile burada, ViewModel katmanında değil — ViewModel
// ASP.NET Core'dan bağımsız kalmalı (bkz. StarCAD.ViewModel/Docs/DocDtos.cs'teki DocUploadDto).
public sealed class DocUploadRequest
{
    public IFormFile File { get; init; } = null!;
    public int? ProjectId { get; init; }
    public int? ProjectUnitId { get; init; }
    public int? DocTypeCodeId { get; init; }
    public int? ProjectFirmId { get; init; }
}
