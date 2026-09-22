using Microsoft.EntityFrameworkCore.Storage;
using Model;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Repository;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class ServiceInvoiceService : BaseService<ServiceInvoice>, IServiceInvoiceService
    {
        private readonly IServiceInvoiceRepository _invoiceRepo;
        private readonly IServiceOrderRepository _orderRepo;
        private readonly INfseService _nfseService;

        public ServiceInvoiceService(
            IGenericRepository<ServiceInvoice> repository,
            IServiceInvoiceRepository invoiceRepo,
            IServiceOrderRepository orderRepo,
            INfseService nfseService) : base(repository)
        {
            _invoiceRepo = invoiceRepo;
            _orderRepo = orderRepo;
            _nfseService = nfseService;
        }

        public async Task<PagedResult<ServiceInvoiceResponse>> GetAllPaged(Filters filter)
        {
            var paged = await _invoiceRepo.GetAllPaged(filter);
            var responses = new PagedResult<ServiceInvoiceResponse>
            {
                CurrentPage = paged.CurrentPage,
                PageCount = paged.PageCount,
                PageSize = paged.PageSize,
                RowCount = paged.RowCount,
                Results = paged.Results.Select(MapToResponse).ToList()
            };
            return responses;
        }

        public async Task<ServiceInvoiceResponse> GetByIdAsync(int id)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");
            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> CreateAsync(ServiceInvoiceCreateRequest request, Guid userId)
        {
            var order = await _orderRepo.GetByIdWithDetails(request.ServiceOrderId);
            if (order == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            if (order.Status == ServiceOrderStatus.Cancelada)
                throw new DomainException("Não é possível criar NFSe para uma ordem cancelada.");

            // Get CodMunIBGE from first service in order
            var firstService = order.ServiceOrderItems.FirstOrDefault();
            var codMunIBGE = firstService?.ServiceProvided?.LocationCode ?? "";

            // Check for duplicate services
            var orderServiceIds = order.ServiceOrderItems.Select(x => x.ServiceProvidedId).ToList();
            foreach (var serviceId in orderServiceIds)
            {
                var alreadyEmitted = await _invoiceRepo.HasEmittedInvoicesForService(request.ServiceOrderId, serviceId);
                if (alreadyEmitted)
                    throw new DomainException($"Serviço ID {serviceId} já foi emitido em outra NFSe desta ordem.");
            }

            var entity = new ServiceInvoice
            {
                ServiceOrderId = request.ServiceOrderId,
                TenantId = request.TenantId,
                ClientId = order.ClientId,

                // IdDPS fica NULO até a emissão. Antes era um Guid truncado em 20 chars,
                // que não é um id de DPS — o id real é montado na transmissão, com o
                // layout do padrão Nacional (cLocEmi + tipoInsc + inscrição + serie + numero).
                IdDPS = null,

                TipoAmbiente = request.TipoAmbiente,
                CodMunIBGE = codMunIBGE,
                Status = ServiceInvoiceStatus.Pendente,
                NumeroDPS = 0,
                DataCompetencia = request.DataCompetencia,
                TotalValue = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                DhEmissao = null,
                ServiceInvoiceItems = new List<ServiceInvoiceItem>()
            };

            // Copy all services from order to invoice — inclusive os campos fiscais, que
            // são a base de cálculo da DPS. Copiar só quantidade/valor perderia as
            // alíquotas e o desconto que o usuário configurou na OS.
            foreach (var orderItem in order.ServiceOrderItems)
            {
                entity.ServiceInvoiceItems.Add(new ServiceInvoiceItem
                {
                    ServiceProvidedId = orderItem.ServiceProvidedId,
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    Discount = orderItem.Discount,
                    Description = orderItem.Description,
                    TotalPrice = orderItem.TotalPrice,
                    IssqnRate = orderItem.IssqnRate,
                    IssqnRetido = orderItem.IssqnRetido,
                    PisRate = orderItem.PisRate,
                    CofinsRate = orderItem.CofinsRate,
                    IrRate = orderItem.IrRate,
                    CsllRate = orderItem.CsllRate,
                    InssRate = orderItem.InssRate,
                    CreatedAt = DateTime.UtcNow
                });
            }

            ServiceTotals.Apply(entity);

            await repository.CreateAsync(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> UpdateAsync(int id, ServiceInvoiceUpdateRequest request)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status != ServiceInvoiceStatus.Pendente)
                throw new DomainException("Apenas NFSe pendente pode ser editada.");

            entity.DataCompetencia = request.DataCompetencia;
            entity.TipoAmbiente = request.TipoAmbiente;
            entity.CodMunIBGE = request.CodMunIBGE ?? entity.CodMunIBGE;
            entity.UpdatedAt = DateTime.UtcNow;

            if (request.Items != null && request.Items.Count > 0)
            {
                var newItems = request.Items.Select(i =>
                {
                    var item = new ServiceInvoiceItem
                    {
                        ServiceInvoiceId = id,
                        ServiceProvidedId = i.ServiceProvidedId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Discount = i.Discount,
                        Description = i.Description,
                        IssqnRate = i.IssqnRate,
                        IssqnRetido = i.IssqnRetido,
                        PisRate = i.PisRate,
                        CofinsRate = i.CofinsRate,
                        IrRate = i.IrRate,
                        CsllRate = i.CsllRate,
                        InssRate = i.InssRate,
                        CreatedAt = DateTime.UtcNow
                    };
                    item.TotalPrice = ServiceTotals.ForItem(item).Base;
                    return item;
                }).ToList();

                await _invoiceRepo.UpdateInvoiceItems(id, newItems);
                entity.ServiceInvoiceItems = newItems;
                ServiceTotals.Apply(entity);
            }

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        /// <summary>
        /// Emite a NFS-e. Três desfechos possíveis, e a fatura sempre fica num estado
        /// coerente com o que de fato aconteceu:
        ///
        /// 1. <b>Transmitida e autorizada</b> — Status Emitido, chave de acesso e XML
        ///    gravados, Sent = true.
        /// 2. <b>Empresa sem certificado</b> — a nota é apenas REGISTRADA localmente
        ///    (Status Emitido, Sent = false) e o motivo fica em ErrorMessage. É o
        ///    comportamento que a UI anuncia antes de emitir.
        /// 3. <b>Transmissão falhou</b> — Status continua Pendente, Sent = false e o
        ///    motivo em ErrorMessage. Nada é perdido: a linha segue reemissível.
        ///
        /// Só os problemas de ENTRADA (fatura inexistente, status errado, sem itens)
        /// lançam DomainException. Falha de transmissão nunca vira exceção — o controller
        /// só captura DomainException, então uma exceção de rede viraria 500 com a fatura
        /// intacta e nenhum registro do que houve.
        /// </summary>
        public async Task<ServiceInvoiceResponse> EmitirAsync(int id, Guid userId)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status != ServiceInvoiceStatus.Pendente)
                throw new DomainException("Apenas NFSe pendente pode ser emitida.");

            if (!entity.ServiceInvoiceItems.Any())
                throw new DomainException("NFSe deve ter pelo menos um serviço.");

            var config = await _invoiceRepo.GetFiscalConfiguration(entity.TenantId);
            var podeTransmitir = _nfseService.TemCertificadoConfigurado(config);

            // Numeração SEM LACUNAS: o número é reservado agora, mas o contador só avança
            // quando a nota de fato existe (autorizada ou registrada localmente). Numa
            // falha de transmissão a fatura guarda o número e a retentativa reusa o MESMO
            // valor — é o que mantém o IdDPS estável entre tentativas. Efeito colateral
            // aceito: se outra nota for emitida no meio, ela toma o número e a
            // retentativa passa a usar um número novo (dois ids no SEFIN para a mesma
            // fatura) — por isso o registro de RequestPayloadJson/ResponseJson.
            var numeroDps = await _invoiceRepo.GetNextInvoiceNumber(entity.TenantId);

            entity.NumeroDPS = numeroDps;
            entity.TryCount++;
            entity.UpdatedAt = DateTime.UtcNow;

            // DhEmissao gravado UMA vez e mantido entre tentativas: o <dhEmissao> da DPS
            // sai daqui, e reenviar o mesmo IdDPS com data diferente pode ser recusado
            // como inconsistente caso a primeira tentativa tenha chegado ao SEFIN.
            if (!entity.DhEmissao.HasValue)
                entity.DhEmissao = DateTime.Now;

            var autorizada = false;

            if (!podeTransmitir)
            {
                // Sem certificado não há transmissão — prometer emissão aqui seria mentir
                // para o usuário. A nota fica registrada e o motivo fica explícito na
                // própria fatura (Sent = false mantém a distinção visível na listagem).
                entity.Sent = false;
                entity.ErrorMessage = config == null
                    ? "NFS-e registrada sem transmissão: a empresa não possui configuração fiscal ativa. "
                      + "Cadastre a configuração fiscal e o certificado digital para transmitir ao SEFIN."
                    : "NFS-e registrada sem transmissão: a configuração fiscal da empresa não possui "
                      + "certificado digital. Cadastre o certificado A1 para transmitir ao SEFIN.";
            }
            else
            {
                // O IdDPS é gravado ANTES de transmitir: se a resposta se perder, é este id
                // que permite consultar no SEFIN se a DPS chegou (ConsultaChaveDps) em vez
                // de emitir de novo e duplicar a nota.
                try
                {
                    entity.IdDPS = _nfseService.MontarIdDps(entity, config, numeroDps.ToString());
                }
                catch (Exception ex)
                {
                    // Dados do emitente/município incompletos: o id nem pode ser montado.
                    entity.ErrorMessage = $"Não foi possível montar a DPS: {ex.Message}";
                    entity.Sent = false;
                    await base.Alter(entity);
                    return MapToResponse(entity);
                }

                var resultado = await _nfseService.EmitirAsync(entity, config);

                entity.IdDPS = resultado.IdDps ?? entity.IdDPS;
                entity.RequestPayloadJson = resultado.RequestPayloadJson ?? resultado.XmlEnvio;
                entity.ResponseJson = resultado.ResponseJson;
                entity.Sent = resultado.Sucesso;
                autorizada = resultado.Sucesso;

                if (resultado.Sucesso)
                {
                    entity.ChaveAcesso = resultado.ChaveAcesso;
                    entity.XmlNfse = resultado.XmlNfse;
                    entity.ErrorMessage = null;
                    // Protocolo fica null de propósito: o padrão Nacional não devolve
                    // protocolo, só IdDps/chave/XML. A coluna existe por paridade com o
                    // lado NFe e não é usada aqui.
                }
                else
                {
                    entity.ErrorMessage = resultado.MensagemErro
                        ?? "A transmissão ao SEFIN falhou sem retorno de erro.";
                }
            }

            // A nota passa a Emitido em dois casos: autorizada pelo SEFIN, ou registrada
            // localmente (empresa sem certificado — a UI avisa antes, e concluir a OS
            // exige NFS-e emitida). Numa FALHA de transmissão a fatura continua Pendente e
            // o número reservado não é consumido, para a retentativa reusar o mesmo IdDPS.
            if (autorizada || !podeTransmitir)
            {
                entity.Status = ServiceInvoiceStatus.Emitido;
                entity.EmittedAt = DateTime.UtcNow;
                await _invoiceRepo.IncrementInvoiceNumber(entity.TenantId);
            }

            await base.Alter(entity);

            // Emitir conclui a OS quando TODAS as faturas ficam Emitido. Numa falha de
            // transmissão a fatura segue Pendente e a OS não é concluída.
            await ConcluirOrdemSeTudoEmitido(entity.ServiceOrderId, userId);

            return MapToResponse(entity);
        }

        private async Task ConcluirOrdemSeTudoEmitido(int serviceOrderId, Guid userId)
        {
            var order = await _orderRepo.GetByIdWithDetails(serviceOrderId);
            if (order == null || order.Status == ServiceOrderStatus.Concluida) return;

            // O Any() não é redundante: All() sobre uma coleção VAZIA devolve true, então
            // sem ele uma OS sem fatura carregada seria concluída por engano.
            if (order.ServiceInvoices.Any()
                && order.ServiceInvoices.All(x => x.Status == ServiceInvoiceStatus.Emitido))
            {
                order.Status = ServiceOrderStatus.Concluida;
                order.ConcludedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = userId;
                try
                {
                    // await _orderRepo.UpdateAsync(order.Id, order);
                    await _orderRepo.SaveChangesAsync(); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
               
            }
        }

        public async Task<ServiceInvoiceResponse> CancelarAsync(int id, string cancelReason, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(cancelReason) || cancelReason.Length < 15)
                throw new DomainException("Justificativa de cancelamento deve ter no mínimo 15 caracteres.");

            // 255 e o teto do SEFIN, nao uma escolha nossa: a justificativa vai em
            // <xMotivo>, cujo tipo (TSMotivo) tem maxLength 255. Aceitar 500 aqui so
            // adiava a rejeicao para a validacao de schema, depois de tudo montado.
            if (cancelReason.Length > 255)
                throw new DomainException("Justificativa de cancelamento deve ter no máximo 255 caracteres.");

            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status == ServiceInvoiceStatus.Cancelado)
                throw new DomainException("NFSe já está cancelada.");

            var config = await _invoiceRepo.GetFiscalConfiguration(entity.TenantId);
            if (config == null)
                throw new DomainException("Empresa sem configuração fiscal ativa.");

            try
            {
                var resp = await _nfseService.CancelarNfse(entity, config, cancelReason);

                // CancelarNfse devolve false quando o SEFIN NAO registrou o evento (HTTP de
                // erro, corpo nao desserializado ou `erros` preenchido). Sem esta guarda o
                // controller responderia "NFSe cancelada com sucesso" com a fatura ainda
                // em Emitido — pior do que falhar, porque mente sobre o estado fiscal.
                if (resp != true)
                    throw new DomainException(
                        "O SEFIN não registrou o cancelamento da NFS-e. A nota continua emitida; verifique a conexão e tente novamente.");
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                // Mesma razão do ObterDanfseAsync: o controller só captura DomainException,
                // então as guardas do CancelarNfse (sem certificado, sem chave de acesso) e
                // as falhas de rede virariam 500 sem mensagem nenhuma na tela.
                throw new DomainException("Não foi possível cancelar a NFS-e: " + ex.Message);
            }

            entity.Status = ServiceInvoiceStatus.Cancelado;
            entity.CancelReason = cancelReason;
            entity.CanceledBy = userId;
            entity.UpdatedAt = DateTime.UtcNow;

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> ResendAsync(int id)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status != ServiceInvoiceStatus.Emitido)
                throw new DomainException("Apenas NFSe emitida pode ser reenviada.");

            // Use current ServiceProvided data to update items
            foreach (var item in entity.ServiceInvoiceItems)
            {
                if (item.ServiceProvided != null)
                {
                    item.UnitPrice = item.ServiceProvided.Value;
                    item.TotalPrice = ServiceTotals.ForItem(item).Base;
                }
            }

            ServiceTotals.Apply(entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task<int> GetNextNumber(int tenantId)
        {
            return await _invoiceRepo.GetNextInvoiceNumber(tenantId);
        }

        /// <summary>
        /// XML autorizado da NFS-e. Não vai no corpo de <see cref="ServiceInvoiceResponse"/>
        /// porque é coluna <c>text</c> e essa mesma resposta serve à listagem paginada.
        /// </summary>
        public async Task<string> ObterXmlAsync(int id)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (string.IsNullOrWhiteSpace(entity.XmlNfse))
                throw new DomainException(
                    entity.Status == ServiceInvoiceStatus.Emitido && !entity.Sent
                        ? "NFS-e registrada sem transmissão ao SEFIN: não há XML autorizado."
                        : "NFS-e ainda não possui XML autorizado.");

            return entity.XmlNfse;
        }

        /// <summary>
        /// DANFSe (PDF) da NFS-e autorizada. Diferente do XML, exige rede: o pacote não
        /// tem gerador local, o PDF é baixado do ambiente nacional pela chave de acesso.
        /// </summary>
        public async Task<byte[]> ObterDanfseAsync(int id)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (string.IsNullOrWhiteSpace(entity.ChaveAcesso))
                throw new DomainException("NFS-e sem chave de acesso: não há DANFSe a baixar.");

            var config = await _invoiceRepo.GetFiscalConfiguration(entity.TenantId);
            if (config == null)
                throw new DomainException("Empresa sem configuração fiscal ativa.");

            try
            {
                return await _nfseService.ObterDanfseAsync(entity, config);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                // Diferente da emissão, aqui não há estado a preservar: basta dizer o que
                // houve. O controller só captura DomainException, então converte-se.
                throw new DomainException("Não foi possível obter o DANFSe: " + ex.Message);
            }
        }

        private ServiceInvoiceResponse MapToResponse(ServiceInvoice entity)
        {
            return new ServiceInvoiceResponse
            {
                Id = entity.Id,
                ServiceOrderId = entity.ServiceOrderId,
                TenantId = entity.TenantId,
                ClientId = entity.ClientId,
                ClientName = entity.Client?.Name ?? "",
                ClientDocument = entity.Client?.Document ?? "",
                IdDPS = entity.IdDPS,
                ChaveAcesso = entity.ChaveAcesso,
                TipoAmbiente = entity.TipoAmbiente.ToString(),
                DhEmissao = entity.DhEmissao,
                CodMunIBGE = entity.CodMunIBGE,
                Status = entity.Status.ToString(),
                NumeroDPS = entity.NumeroDPS,
                DataCompetencia = entity.DataCompetencia,
                TotalValue = entity.TotalValue,
                DiscountValue = entity.DiscountValue,
                IssqnValue = entity.IssqnValue,
                IssqnRetidoValue = entity.IssqnRetidoValue,
                RetentionValue = entity.RetentionValue,
                NetValue = entity.NetValue,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                EmittedAt = entity.EmittedAt,
                CancelReason = entity.CancelReason,
                CanceledBy = entity.CanceledBy,
                CreatedBy = entity.CreatedBy,

                // O XML nunca sai no corpo desta resposta (é coluna text e esta mesma
                // resposta serve a listagem paginada). Só o sinalizador de presença.
                HasXml = !string.IsNullOrEmpty(entity.XmlNfse),

                Sent = entity.Sent,
                TryCount = entity.TryCount,
                ErrorMessage = entity.ErrorMessage,
                Items = entity.ServiceInvoiceItems?.Select(i => new ServiceInvoiceItemResponse
                {
                    Id = i.Id,
                    ServiceProvidedId = i.ServiceProvidedId,
                    ServiceName = i.ServiceProvided?.Name ?? "",
                    ServiceDescription = i.ServiceProvided?.Description ?? "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    Description = i.Description,
                    TotalPrice = i.TotalPrice,
                    IssqnRate = i.IssqnRate,
                    IssqnRetido = i.IssqnRetido,
                    IssqnValue = ServiceTotals.ForItem(i).Issqn,
                    PisRate = i.PisRate,
                    CofinsRate = i.CofinsRate,
                    IrRate = i.IrRate,
                    CsllRate = i.CsllRate,
                    InssRate = i.InssRate,
                    LocationCode = i.ServiceProvided?.LocationCode ?? "",
                    NationalTaxCode = i.ServiceProvided?.NationalTaxCode ?? ""
                }).ToList() ?? new List<ServiceInvoiceItemResponse>()
            };
        }
    }

    public interface IServiceInvoiceService : IBaseService<ServiceInvoice>
    {
        Task<PagedResult<ServiceInvoiceResponse>> GetAllPaged(Filters filter);
        Task<ServiceInvoiceResponse> GetByIdAsync(int id);
        Task<ServiceInvoiceResponse> CreateAsync(ServiceInvoiceCreateRequest request, Guid userId);
        Task<ServiceInvoiceResponse> UpdateAsync(int id, ServiceInvoiceUpdateRequest request);
        Task<ServiceInvoiceResponse> EmitirAsync(int id, Guid userId);
        Task<ServiceInvoiceResponse> CancelarAsync(int id, string cancelReason, Guid userId);
        Task<ServiceInvoiceResponse> ResendAsync(int id);
        Task<int> GetNextNumber(int tenantId);
        Task<string> ObterXmlAsync(int id);
        Task<byte[]> ObterDanfseAsync(int id);
    }
}
