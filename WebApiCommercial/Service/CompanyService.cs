using GoogleApi.Entities.Search.Video.Common;
using Microsoft.AspNetCore.Components.Forms;
using Model.DTO;
using Model.Registrations;
using Repository;
using System.Threading.Tasks;

namespace Service
{
    public class CompanyService : BaseService<Company>, ICompanyService
    {
        public CompanyService(IGenericRepository<Company> repository) : base(repository)
        {
        }

        public async Task<Company> GetById(int id)
        {
            var resp=await  ( repository as ICompanyRepository).GetByIdAsync(id);
            return resp;
        }
        public async Task<bool> UpdateCompany(int id, CompanyDto value)
        {
            var existingCompany = await (repository as ICompanyRepository).GetByIdAsync(id);
            if (existingCompany == null)
            {
                return false;
            }

            // Atualiza os campos
            existingCompany.CorporateName = value.CorporateName;
            existingCompany.Name = value.Name;
            existingCompany.Cnpj = value.Cnpj;
            existingCompany.ZipCode = value.ZipCode;
            existingCompany.Address = value.Address;
            existingCompany.State = value.State;
            existingCompany.City = value.City;
            existingCompany.CommercialPhone = value.CommercialPhone;
            existingCompany.Cellphone = value.Cellphone;

            // Limpa os campos numéricos
            //existingCompany.CleanNumericFields();

            //// Validações
            //if (existingCompany.Cnpj.Length != 14)
            //{
            //    return BadRequest("CNPJ deve conter 14 dígitos");
            //}

            //if (existingCompany.ZipCode.Length != 8)
            //{
            //    return BadRequest("CEP deve conter 8 dígitos");
            //}

           await base.Alter(existingCompany);
            return true;
        }
    }
    public interface ICompanyService : IBaseService<Company>
    {
        Task<Company> GetById(int id);
        Task<bool> UpdateCompany(int id, CompanyDto value);
    }
}
