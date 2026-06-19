using Model;
using Model.DTO;
using Model.Enums;
using Model.Registrations;
using Repository;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service
{
    public class ServiceProvidedService : BaseService<ServiceProvided>, IServiceProvidedService
    {
        public ServiceProvidedService(IGenericRepository<ServiceProvided> repository) : base(repository)
        {
        }

        public async Task<PagedResult<ServiceProvided>> GetAllPaged(Filters filter)
        {
            return await (repository as IServiceProvidedRepository).GetAllPaged(filter);
        }

        public async Task<List<ServiceProvided>> GetListByName(Filters filter)
        {
            return await (repository as IServiceProvidedRepository).GetListByName(filter);
        }

        /// <summary>
        /// Cria um novo serviço prestado com validações NFS-e 2026
        /// </summary>
        public async Task<ServiceProvidedResponse> CreateAsync(ServiceProvidedCreateRequest request)
        {
            ValidateCreateRequest(request);

            var entity = MapCreateRequestToEntity(request);

            await repository.CreateAsync(entity);

            return MapToResponse(entity);
        }

        /// <summary>
        /// Atualiza um serviço prestado existente com validações NFS-e 2026
        /// </summary>
        public async Task<ServiceProvidedResponse> UpdateAsync(int id, ServiceProvidedUpdateRequest request)
        {
            ValidateUpdateRequest(request);

            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                throw new DomainException("Serviço prestado não encontrado.");

            MapUpdateRequestToEntity(request, existing);

            await base.Alter(existing);

            return MapToResponse(existing);
        }

        /// <summary>
        /// Obtém um serviço prestado por ID
        /// </summary>
        public async Task<ServiceProvidedResponse> GetByIdDtoAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        /// <summary>
        /// Exclui um serviço prestado
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                throw new DomainException("Serviço prestado não encontrado.");

            await base.DeleteAsync(id);
        }

        // ===== VALIDAÇÕES =====

        private void ValidateCreateRequest(ServiceProvidedCreateRequest request)
        {
            var errors = new List<string>();

            // Validações obrigatórias
            if (string.IsNullOrWhiteSpace(request.LocationCode) || request.LocationCode.Length != 7 || !request.LocationCode.All(char.IsDigit))
                errors.Add("O código IBGE do município (LocationCode) deve ter exatamente 7 dígitos numéricos.");

            if (string.IsNullOrWhiteSpace(request.NationalTaxCode) || request.NationalTaxCode.Length != 6 || !request.NationalTaxCode.All(char.IsDigit))
                errors.Add("O código de tributação nacional (NationalTaxCode) deve ter exatamente 6 dígitos numéricos.");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 2000)
                errors.Add("A descrição do serviço (Description) deve ter entre 10 e 2000 caracteres.");

            // Validações opcionais de formato
            if (!string.IsNullOrEmpty(request.MunicipalTaxCode) && (request.MunicipalTaxCode.Length != 3 || !request.MunicipalTaxCode.All(char.IsDigit)))
                errors.Add("O código de tributação municipal (MunicipalTaxCode) deve ter exatamente 3 dígitos numéricos.");

            if (!string.IsNullOrEmpty(request.NbsCode) && (request.NbsCode.Length != 9 || !request.NbsCode.All(char.IsDigit)))
                errors.Add("O código NBS (NbsCode) deve ter exatamente 9 dígitos numéricos.");

            if (!string.IsNullOrEmpty(request.InternalContributorCode) && !Regex.IsMatch(request.InternalContributorCode, @"^[a-zA-Z0-9]{1,20}$"))
                errors.Add("O código interno do contribuinte (InternalContributorCode) deve ser alfanumérico e ter entre 1 e 20 caracteres.");

            // Validações condicionais por tipo especial
            ValidateSpecialTypeFields(request.SpecialType, request, errors);

            if (errors.Count > 0)
                throw new DomainException(string.Join("; ", errors));
        }

        private void ValidateUpdateRequest(ServiceProvidedUpdateRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.LocationCode) || request.LocationCode.Length != 7 || !request.LocationCode.All(char.IsDigit))
                errors.Add("O código IBGE do município (LocationCode) deve ter exatamente 7 dígitos numéricos.");

            if (string.IsNullOrWhiteSpace(request.NationalTaxCode) || request.NationalTaxCode.Length != 6 || !request.NationalTaxCode.All(char.IsDigit))
                errors.Add("O código de tributação nacional (NationalTaxCode) deve ter exatamente 6 dígitos numéricos.");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 2000)
                errors.Add("A descrição do serviço (Description) deve ter entre 10 e 2000 caracteres.");

            if (!string.IsNullOrEmpty(request.MunicipalTaxCode) && (request.MunicipalTaxCode.Length != 3 || !request.MunicipalTaxCode.All(char.IsDigit)))
                errors.Add("O código de tributação municipal (MunicipalTaxCode) deve ter exatamente 3 dígitos numéricos.");

            if (!string.IsNullOrEmpty(request.NbsCode) && (request.NbsCode.Length != 9 || !request.NbsCode.All(char.IsDigit)))
                errors.Add("O código NBS (NbsCode) deve ter exatamente 9 dígitos numéricos.");

            if (!string.IsNullOrEmpty(request.InternalContributorCode) && !Regex.IsMatch(request.InternalContributorCode, @"^[a-zA-Z0-9]{1,20}$"))
                errors.Add("O código interno do contribuinte (InternalContributorCode) deve ser alfanumérico e ter entre 1 e 20 caracteres.");

            ValidateSpecialTypeFields(request.SpecialType, request, errors);

            if (errors.Count > 0)
                throw new DomainException(string.Join("; ", errors));
        }

        private void ValidateSpecialTypeFields(ServiceSpecialType specialType, ServiceProvidedCreateRequest req, List<string> errors)
        {
            ValidateConstructionFields(specialType, req.ConstructionCode, req.CibCode, req.PropertyRegistry, errors);
            ValidateEventFields(specialType, req.EventName, req.EventStartDate, req.EventEndDate, errors);
            ValidateForeignTradeFields(specialType, req.ServiceMode, req.ServiceLink, req.CurrencyCode, errors);
        }

        private void ValidateSpecialTypeFields(ServiceSpecialType specialType, ServiceProvidedUpdateRequest req, List<string> errors)
        {
            ValidateConstructionFields(specialType, req.ConstructionCode, req.CibCode, req.PropertyRegistry, errors);
            ValidateEventFields(specialType, req.EventName, req.EventStartDate, req.EventEndDate, errors);
            ValidateForeignTradeFields(specialType, req.ServiceMode, req.ServiceLink, req.CurrencyCode, errors);
        }

        private void ValidateConstructionFields(ServiceSpecialType type, string constructionCode, string cibCode, string propertyRegistry, List<string> errors)
        {
            if (type != ServiceSpecialType.Construction) return;

            bool hasConstructionCode = !string.IsNullOrWhiteSpace(constructionCode);
            bool hasCibCode = !string.IsNullOrWhiteSpace(cibCode);
            bool hasPropertyRegistry = !string.IsNullOrWhiteSpace(propertyRegistry);

            if (!hasConstructionCode && !hasCibCode && !hasPropertyRegistry)
                errors.Add("Para serviços de obra (SpecialType = Construction), é necessário informar ao menos um: ConstructionCode, CibCode ou PropertyRegistry.");

            if (hasCibCode && (cibCode.Length != 8 || !cibCode.All(char.IsDigit)))
                errors.Add("O código CIB deve ter exatamente 8 dígitos numéricos.");
        }

        private void ValidateEventFields(ServiceSpecialType type, string eventName, DateTime? eventStartDate, DateTime? eventEndDate, List<string> errors)
        {
            if (type != ServiceSpecialType.Event) return;

            if (string.IsNullOrWhiteSpace(eventName))
                errors.Add("Para serviços de evento (SpecialType = Event), o nome do evento (EventName) é obrigatório.");

            if (eventStartDate == null)
                errors.Add("Para serviços de evento (SpecialType = Event), a data de início (EventStartDate) é obrigatória.");

            if (eventEndDate == null)
                errors.Add("Para serviços de evento (SpecialType = Event), a data de fim (EventEndDate) é obrigatória.");
        }

        private void ValidateForeignTradeFields(ServiceSpecialType type, string serviceMode, string serviceLink, string currencyCode, List<string> errors)
        {
            if (type != ServiceSpecialType.ForeignTrade) return;

            if (string.IsNullOrWhiteSpace(serviceMode))
                errors.Add("Para comércio exterior (SpecialType = ForeignTrade), o modo de prestação (ServiceMode) é obrigatório.");

            if (string.IsNullOrWhiteSpace(serviceLink))
                errors.Add("Para comércio exterior (SpecialType = ForeignTrade), o vínculo entre as partes (ServiceLink) é obrigatório.");

            if (string.IsNullOrWhiteSpace(currencyCode))
                errors.Add("Para comércio exterior (SpecialType = ForeignTrade), o código da moeda (CurrencyCode) é obrigatório.");
        }

        // ===== MAPEAMENTOS =====

        private ServiceProvided MapCreateRequestToEntity(ServiceProvidedCreateRequest request)
        {
            return new ServiceProvided
            {
                IdCompany = request.IdCompany,
                Name = request.Name,
                Value = request.Value,
                LocationCode = request.LocationCode,
                NationalTaxCode = request.NationalTaxCode,
                Description = request.Description,
                MunicipalTaxCode = request.MunicipalTaxCode,
                NbsCode = request.NbsCode,
                InternalContributorCode = request.InternalContributorCode,
                SpecialType = request.SpecialType,
                PropertyRegistry = request.PropertyRegistry,
                ConstructionCode = request.ConstructionCode,
                CibCode = request.CibCode,
                EventName = request.EventName,
                EventStartDate = request.EventStartDate,
                EventEndDate = request.EventEndDate,
                EventIdentifier = request.EventIdentifier,
                ServiceMode = request.ServiceMode,
                ServiceLink = request.ServiceLink,
                CurrencyCode = request.CurrencyCode,
                ForeignValue = request.ForeignValue
            };
        }

        private void MapUpdateRequestToEntity(ServiceProvidedUpdateRequest request, ServiceProvided entity)
        {
            entity.Name = request.Name;
            entity.Value = request.Value;
            entity.LocationCode = request.LocationCode;
            entity.NationalTaxCode = request.NationalTaxCode;
            entity.Description = request.Description;
            entity.MunicipalTaxCode = request.MunicipalTaxCode;
            entity.NbsCode = request.NbsCode;
            entity.InternalContributorCode = request.InternalContributorCode;
            entity.SpecialType = request.SpecialType;
            entity.PropertyRegistry = request.PropertyRegistry;
            entity.ConstructionCode = request.ConstructionCode;
            entity.CibCode = request.CibCode;
            entity.EventName = request.EventName;
            entity.EventStartDate = request.EventStartDate;
            entity.EventEndDate = request.EventEndDate;
            entity.EventIdentifier = request.EventIdentifier;
            entity.ServiceMode = request.ServiceMode;
            entity.ServiceLink = request.ServiceLink;
            entity.CurrencyCode = request.CurrencyCode;
            entity.ForeignValue = request.ForeignValue;

            // Limpar campos não aplicáveis baseado no tipo
            if (entity.SpecialType != ServiceSpecialType.Construction)
            {
                entity.PropertyRegistry = null;
                entity.ConstructionCode = null;
                entity.CibCode = null;
            }
            if (entity.SpecialType != ServiceSpecialType.Event)
            {
                entity.EventName = null;
                entity.EventStartDate = null;
                entity.EventEndDate = null;
                entity.EventIdentifier = null;
            }
            if (entity.SpecialType != ServiceSpecialType.ForeignTrade)
            {
                entity.ServiceMode = null;
                entity.ServiceLink = null;
                entity.CurrencyCode = null;
                entity.ForeignValue = null;
            }
        }

        private ServiceProvidedResponse MapToResponse(ServiceProvided entity)
        {
            return new ServiceProvidedResponse
            {
                Id = entity.Id,
                IdCompany = entity.IdCompany,
                Name = entity.Name,
                Value = entity.Value,
                LocationCode = entity.LocationCode,
                NationalTaxCode = entity.NationalTaxCode,
                Description = entity.Description,
                MunicipalTaxCode = entity.MunicipalTaxCode,
                NbsCode = entity.NbsCode,
                InternalContributorCode = entity.InternalContributorCode,
                SpecialType = entity.SpecialType.ToString(),
                PropertyRegistry = entity.PropertyRegistry,
                ConstructionCode = entity.ConstructionCode,
                CibCode = entity.CibCode,
                EventName = entity.EventName,
                EventStartDate = entity.EventStartDate,
                EventEndDate = entity.EventEndDate,
                EventIdentifier = entity.EventIdentifier,
                ServiceMode = entity.ServiceMode,
                ServiceLink = entity.ServiceLink,
                CurrencyCode = entity.CurrencyCode,
                ForeignValue = entity.ForeignValue
            };
        }
    }

    public interface IServiceProvidedService : IBaseService<ServiceProvided>
    {
        Task<PagedResult<ServiceProvided>> GetAllPaged(Filters filter);
        Task<List<ServiceProvided>> GetListByName(Filters filter);
        Task<ServiceProvidedResponse> CreateAsync(ServiceProvidedCreateRequest request);
        Task<ServiceProvidedResponse> UpdateAsync(int id, ServiceProvidedUpdateRequest request);
        Task<ServiceProvidedResponse> GetByIdDtoAsync(int id);
        Task DeleteAsync(int id);
    }
}
