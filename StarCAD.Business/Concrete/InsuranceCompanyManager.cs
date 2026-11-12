using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.ViewModel.InsuranceCompanies;

namespace StarCAD.Business.Concrete;

public class InsuranceCompanyManager : IInsuranceCompanyService
{
    private readonly StarCADContext _db;

    public InsuranceCompanyManager(StarCADContext db)
    {
        _db = db;
    }

    public async Task<IDataResult<List<InsuranceCompanyListItemDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.InsuranceCompanies.AsNoTracking()
            .OrderBy(c => c.Title)
            .Select(c => new InsuranceCompanyListItemDto
            {
                Id = c.Id,
                Title = c.Title
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<List<InsuranceCompanyListItemDto>>(list);
    }
}
