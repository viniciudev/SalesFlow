using Model;
using Model.DTO;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public class BankAccountService : BaseService<BankAccount>, IBankAccountService
    {
        private readonly IBankAccountRepository _repository;
        public BankAccountService(IGenericRepository<BankAccount> repository,
            IBankAccountRepository bankAccountRepository) : base(repository)
        {
            _repository = bankAccountRepository;
        }

        public async Task<PagedResult<BankAccountDto>> GetPagedAsync(BankAccountFilterDto filter)
        {
            PagedResult<BankAccountDto> paged = await _repository.GetPagedAsync(filter);
            return paged;

        }

        public async Task<IEnumerable<BankAccountDto>> GetByFilterAsync(int idcompany, string searchTerm)
        {
            IEnumerable<BankAccountDto> query = await _repository.GetByFilterAsync(idcompany, searchTerm);

            return query;
        }

        public async Task<BankAccountDto> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<BankAccountDto> CreateAsync(CreateBankAccountDto dto)
        {
            var entity = new BankAccount
            {
                BankCode = dto.BankCode,
                BankName = dto.BankName,
                AgencyNumber = dto.AgencyNumber,
                AgencyDigit = dto.AgencyDigit,
                AccountNumber = dto.AccountNumber,
                AccountDigit = dto.AccountDigit,
                AccountType = Enum.Parse<AccountType>(dto.AccountType),
                HolderName = dto.HolderName,
                HolderDocument = dto.HolderDocument,
                HolderType = Enum.Parse<HolderType>(dto.HolderType),
                PixKey = dto.PixKey,
                IsActive = dto.IsActive,
                IdCompany = dto.idCompany
            };

            await _repository.CreateAsync(entity);


            return MapToDto(entity);
        }

        public async Task<BankAccountDto> UpdateAsync(UpdateBankAccountDto dto)
        {
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity == null)
                return null;

            entity.BankCode = dto.BankCode;
            entity.BankName = dto.BankName;
            entity.AgencyNumber = dto.AgencyNumber;
            entity.AgencyDigit = dto.AgencyDigit;
            entity.AccountNumber = dto.AccountNumber;
            entity.AccountDigit = dto.AccountDigit;
            entity.AccountType = Enum.Parse<AccountType>(dto.AccountType);
            entity.HolderName = dto.HolderName;
            entity.HolderDocument = dto.HolderDocument;
            entity.HolderType = Enum.Parse<HolderType>(dto.HolderType);
            entity.PixKey = dto.PixKey;
            entity.IsActive = dto.IsActive;


            await _repository.UpdateAsync(entity.Id, entity);


            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }

        public async Task<BankAccountDto> ToggleStatusAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            entity.IsActive = !entity.IsActive;


            await _repository.UpdateAsync(id, entity);


            return MapToDto(entity);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.Exists(id);
        }

        private BankAccountDto MapToDto(BankAccount entity)
        {
            return new BankAccountDto
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                AgencyNumber = entity.AgencyNumber,
                AgencyDigit = entity.AgencyDigit,
                AccountNumber = entity.AccountNumber,
                AccountDigit = entity.AccountDigit,
                AccountType = entity.AccountType.ToString(),
                HolderName = entity.HolderName,
                HolderDocument = entity.HolderDocument,
                HolderType = entity.HolderType.ToString(),
                PixKey = entity.PixKey,
                IsActive = entity.IsActive,

            };
        }
    }

    public interface IBankAccountService : IBaseService<BankAccount>
    {
        Task<PagedResult<BankAccountDto>> GetPagedAsync(BankAccountFilterDto filter);
        Task<IEnumerable<BankAccountDto>> GetByFilterAsync(int idcompany, string searchTerm);
        Task<BankAccountDto> GetByIdAsync(int id);
        Task<BankAccountDto> CreateAsync(CreateBankAccountDto dto);
        Task<BankAccountDto> UpdateAsync(UpdateBankAccountDto dto);
        Task<bool> DeleteAsync(int id);
        Task<BankAccountDto> ToggleStatusAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
