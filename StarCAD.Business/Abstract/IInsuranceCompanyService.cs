using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.InsuranceCompanies;

namespace StarCAD.Business.Abstract;

// Salt-okunur: kayıtlar HasData ile seed edilir (bkz. InsuranceCompanyConfiguration), elle CRUD yok.
public interface IInsuranceCompanyService
{
    Task<IDataResult<List<InsuranceCompanyListItemDto>>> GetListAsync(CancellationToken cancellationToken = default);
}
