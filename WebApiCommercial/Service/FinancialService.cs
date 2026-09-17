using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.Registrations;
using YourNamespace.DTOs;

namespace Service
{
    public class FinancialService : BaseService<Financial>, IFinancialService
    {
        private readonly ICostCenterRepository _costCenterRepository;
        private readonly IFinancialResourceRepository _financialResourceRepository;
        private readonly IFinancialPaymentMethodRepository _financialPaymentMethodRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly IBoxService _boxService;

        public FinancialService(IGenericRepository<Financial> repository,
            ICostCenterRepository costCenterRepository,
            IFinancialResourceRepository financialResourceRepository,
            IFinancialPaymentMethodRepository financialPaymentMethodRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IBoxRepository boxRepository,
            IBoxService boxService) : base(repository)
        {
            _costCenterRepository = costCenterRepository;
            _financialResourceRepository = financialResourceRepository;
            _financialPaymentMethodRepository = financialPaymentMethodRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _boxRepository = boxRepository;
            _boxService = boxService;
        }
        public async Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem)
        {
            return await (repository as IFinancialRepository).SearchBySaleItemsId(id, typeItem, idItem);
        }
        public async Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters)
        {
            return await (repository as IFinancialRepository).GetPagedByFilter(filters);
        }
        public async Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters)
        {
            return await (repository as IFinancialRepository).GetByMonthAllCommission(filters);
        }
        public async Task DeleteFinancial(int id)
        {
            try
            {
                Financial financialData = await (repository as IFinancialRepository).GetById(id);
                if (financialData != null)
                {
                    if (financialData.FinancialPaymentMethods != null)
                    {
                        foreach (var item in financialData.FinancialPaymentMethods)
                        {
                            await _financialPaymentMethodRepository.DeleteAsync(item.Id);
                        }

                    }
                }
                await base.DeleteAsync(id);
            }
            catch (System.Exception ex)
            {

                throw;
            }

        }
        public async Task<List<Financial>> GetByIdCompany(Filters filters)
        {
            return await (repository as IFinancialRepository).GetByIdCompany(filters);
        }
        public async Task AlterFinancial(FinancialRequest financial)
        {
            Financial financialData = await (repository as IFinancialRepository).GetById(financial.Id);
            if (financialData != null)
            {
                if (financialData.FinancialPaymentMethods != null)
                {
                    foreach (var item in financialData.FinancialPaymentMethods)
                    {
                        await _financialPaymentMethodRepository.DeleteAsync(item.Id);
                    }

                }
                List<FinancialPaymentMethod> financialPaymentMethod = new();
                foreach (var item in financial.PaymentMethods)
                {
                    financialPaymentMethod.Add(new FinancialPaymentMethod
                    {
                        PaymentMethodId = item.PaymentMethodId,
                        FinancialId = financialData.Id,
                        Amount = item.Value,
                        //      Installments = item.Installments
                    });
                }
                ;
                financialData.FinancialPaymentMethods = financialPaymentMethod;
                financialData.Value = financial.Value;
                financialData.FinancialType = financial.FinancialType;
                financialData.Description = financial.Description;
                financialData.DueDate = financial.DueDate;
                financialData.FinancialStatus = financial.FinancialStatus;
                financialData.BankAccountId = financial.BankAccountId;
                financialData.SettlementDate = financial.SettlementDate; // Novo
                financialData.InterestValue = financial.InterestValue;     // Novo
                financialData.FineValue = financial.FineValue;             // Novo
                financialData.SettledValue = financial.SettledValue;       // Novo
                await base.Alter(financialData);
            }
        }
        public async Task<List<int>> CreateFinancial(FinancialInstallmentRequest financial)
        {
            try
            {
                ValidateFinancialRequest(financial);

                var listCostCenter = await _costCenterRepository.GetByIdCompany(financial.IdCompany);
                int? costCenterId = listCostCenter.FirstOrDefault()?.Id;

                int installments = financial.NumberOfInstallments <= 0 ? 1 : financial.NumberOfInstallments;
                var ids = new List<int>();

                // Registro simples (sem parcelamento) - comportamento existente
                if (installments == 1)
                {
                    Financial fin = BuildFinancialRecord(financial, financial.Value, financial.DueDate,
                        financial.Description, costCenterId, installmentIndex: -1, installments: 1);
                    await base.Save(fin);
                    ids.Add(fin.Id);
                    return ids;
                }

                // Parcelamento: divide o valor em partes iguais e gera um registro por parcela
                decimal installmentValue = Math.Round(financial.Value / installments, 2);
                decimal remainingValue = financial.Value;
                bool hasManualDueDates = financial.InstallmentDueDates != null
                                         && financial.InstallmentDueDates.Count == installments;

                for (int i = 0; i < installments; i++)
                {
                    // A última parcela absorve a diferença de arredondamento
                    decimal currentValue = (i == installments - 1)
                        ? Math.Round(remainingValue, 2)
                        : installmentValue;
                    remainingValue -= currentValue;

                    // Vencimento manual (se informado) ou sequencial por intervalo de dias
                    DateTime dueDate = hasManualDueDates
                        ? financial.InstallmentDueDates[i]
                        : financial.DueDate.AddDays(i * financial.InstallmentIntervalDays);

                    string description = $"Parcela {i + 1}/{installments} - {financial.Description}";

                    Financial fin = BuildFinancialRecord(financial, currentValue, dueDate,
                        description, costCenterId, installmentIndex: i, installments: installments);
                    await base.Save(fin);
                    ids.Add(fin.Id);
                }

                return ids;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Valida os dados do request de financeiro/parcelamento.
        /// </summary>
        private void ValidateFinancialRequest(FinancialInstallmentRequest financial)
        {
            if (financial.Value <= 0)
                throw new Exception("O valor total deve ser maior que zero.");
            if (financial.NumberOfInstallments < 1)
                throw new Exception("O número de parcelas deve ser maior que zero.");
            if (financial.InstallmentIntervalDays < 1)
                throw new Exception("O intervalo entre parcelas deve ser maior que zero.");
            if (financial.InstallmentDueDates != null &&
                financial.InstallmentDueDates.Count >0 &&
                financial.InstallmentDueDates.Count != financial.NumberOfInstallments)
                throw new Exception("O número de datas de vencimento deve ser igual ao número de parcelas.");
            if (financial.PaymentMethods == null || !financial.PaymentMethods.Any())
                throw new Exception("Adicione pelo menos uma forma de pagamento.");
        }

        /// <summary>
        /// Monta um registro financeiro a partir do request, aplicando o valor e a
        /// data de vencimento da parcela. Quando parcelado, cada parcela recebe
        /// uma FinancialPaymentMethod com o valor rateado (a última parcela absorve
        /// a diferença de arredondamento).
        /// </summary>
        private Financial BuildFinancialRecord(
            FinancialInstallmentRequest financial,
            decimal value,
            DateTime dueDate,
            string description,
            int? costCenterId,
            int installmentIndex,
            int installments)
        {
            Financial fin = new Financial
            {
                BankAccountId = financial.BankAccountId,
                FinancialStatus = financial.FinancialStatus,
                FinancialType = financial.FinancialType,
                CreationDate = financial.CreationDate,
                DueDate = dueDate,
                Description = description,
                Origin = financial.Origin,
                IdCompany = (int)financial.IdCompany,
                IdCostCenter = costCenterId,
                Value = value,
                IdClient = financial.ClientId,
                SettlementDate = financial.SettlementDate,
                InterestValue = financial.InterestValue,
                FineValue = financial.FineValue,
                SettledValue = financial.SettledValue,
            };

            List<FinancialPaymentMethod> financialPaymentMethod = new();
            foreach (var item in financial.PaymentMethods)
            {
                decimal amount;
                if (installments > 1)
                {
                    decimal pmInstallment = Math.Round(item.Value / installments, 2);
                    amount = (installmentIndex == installments - 1)
                        ? Math.Round(item.Value - (pmInstallment * (installments - 1)), 2)
                        : pmInstallment;
                }
                else
                {
                    amount = item.Value;
                }

                financialPaymentMethod.Add(new FinancialPaymentMethod
                {
                    PaymentMethodId = item.PaymentMethodId,
                    FinancialId = fin.Id,
                    Amount = amount,
                    Installments = 1
                });
            }
            fin.FinancialPaymentMethods = financialPaymentMethod;

            return fin;
        }
        public async Task<List<Financial>> GetByIdSaleAsync(int id)
        {
            return await (repository as IFinancialRepository).GetByIdSaleAsync(id);
        }
        public async Task<PagedResultWithTotals> GetPaged(Filters filters)
        {
            return await (repository as IFinancialRepository).GetPaged(filters);
        }
        public async Task<PagedResult<FinancialResponse>> GetPagedByIdClient(Filters filters)
        {
            return await (repository as IFinancialRepository).GetPagedByIdClient(filters);
        }
        public async Task AlterFinancialStatus(Financial financial)
        {
            try
            {
                Financial financialData = await base.GetByIdAsync(financial.Id);
                financialData.FinancialStatus = financial.FinancialStatus;
                financialData.SettlementDate = DateTime.Now.ToString("yyyy-MM-dd");
                await base.Alter(financialData);
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
        public async Task CreateRenegotiationAsync(RenegotiationRequestDto request)
        {
            await GenerateFinancial(request);
        }
        private async Task GenerateFinancial(RenegotiationRequestDto request)
        {
            try
            {
                //cria nova parcela
                var listCostCenter = await _costCenterRepository.GetByIdCompany(request.IdCompany);
                for (int i = 0; i < request.NumberOfInstallments; i++)
                {
                    Financial financial = new Financial();
                    financial.Id = 0;
                    financial.FinancialStatus =  FinancialStatus.pending;
                    financial.FinancialType = FinancialType.recipe;
                    financial.Origin = OriginFinancial.renegotiation;

                    financial.CreationDate = DateTime.Now;
                    financial.DueDate = i == 0 ? request.NewDueDate : request.NewDueDate.AddMonths(i);
                    financial.IdCompany = request.IdCompany;
                    financial.Description = request.Description;
                    financial.IdCostCenter = listCostCenter.FirstOrDefault()?.Id;
                    financial.IdClient = request.ClientId;

                    financial.Value = (request.NewValue / request.NumberOfInstallments);
                    List<FinancialPaymentMethod> financialPaymentMethod = new();
                    foreach (var item in request.PaymentMethods)
                    {
                        financialPaymentMethod.Add(new FinancialPaymentMethod
                        {
                            PaymentMethodId = item.Id,
                            FinancialId = financial.Id,
                            //Amount = item.Amount,
                            //      Installments = item.Installments
                        });
                    }
                    financial.FinancialPaymentMethods = financialPaymentMethod;
                    await base.Create(financial);

                    foreach (var id in request.OriginalInstallments)
                    {
                        await _financialResourceRepository.CreateAsync(
                            new FinancialResources
                            {
                                IdRefOrigin = id,
                                IdNewFinancial = financial.Id
                            });
                    }
                }

                //muda o status pra renegociado
                foreach (var id in request.OriginalInstallments)
                {
                    await AlterFinancialStatus(new Financial
                    {
                        Id = id,
                        FinancialStatus = FinancialStatus.renegotiated,
                    });
                }


            }
            catch (Exception ex)
            {

                throw;
            }
        }
        /// <summary>
        /// Gera os lançamentos financeiros (uma parcela por <see cref="Financial"/>) a partir das
        /// formas de pagamento informadas. Serve tanto para vendas (<paramref name="idSale"/>) quanto
        /// para ordens de serviço (<paramref name="idServiceOrder"/>) — sempre exatamente um dos dois.
        /// Não faz nada quando a coleção vem vazia.
        /// </summary>
        public async Task GenerateFinancialCentral(ICollection<FormPaymentSale> formPaymentSales, int idCompany,
            int? idSale = null, int? idServiceOrder = null, int? idClient = null, int? bankAccountId = null,
            string? descricaoOrigem = null)
        {
            if (formPaymentSales == null || !formPaymentSales.Any())
                return;

            var caixaAberto = await _boxRepository.GetByStatus(CaixaStatus.ABERTO, idCompany);

            var listCostCenter = await _costCenterRepository.GetByIdCompany(idCompany);
            var costCenterId = listCostCenter.FirstOrDefault()?.Id;

            var paymentMethods = new Dictionary<int, PaymentMethod>();

            foreach (var m in formPaymentSales)
            {
                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(m.PaymentMethodId);
                if (paymentMethod == null)
                    throw new Exception($"Método de pagamento ID {m.PaymentMethodId} não encontrado");

                paymentMethods[m.PaymentMethodId] = paymentMethod;

                // Sem parcelamento informado, normaliza para 1 (o valor é mutado no DTO de entrada).
                if (!m.Installments.HasValue || m.Installments.Value <= 1)
                    m.Installments = 1;
            }

            // Usado no texto da descrição: "Venda #12" / "Ordem de Serviço #34".
            string referencia = descricaoOrigem ?? "Venda";
            int? referenciaId = idSale ?? idServiceOrder;

            foreach (var m in formPaymentSales)
            {
                var paymentMethod = paymentMethods[m.PaymentMethodId];
                int installments = m.Installments ?? 1;
                decimal installmentValue = Math.Round(m.Value / installments, 2);
                decimal remainingValue = m.Value;

                DateTime firstDueDate = GetFirstDueDate();
                bool hasManualDueDates = m.InstallmentDueDates != null
                                         && m.InstallmentDueDates.Count == installments;

                for (int i = 0; i < installments; i++)
                {
                    decimal currentValue = (i == installments - 1)
                        ? Math.Round(remainingValue, 2)
                        : installmentValue;
                    remainingValue -= currentValue;

                    DateTime dueDate;
                    if (hasManualDueDates)
                    {
                        // Usa a data informada manualmente pelo usuário
                        dueDate = m.InstallmentDueDates[i];
                    }
                    else
                    {
                        //sem parcelamento e liquidação pendente=data de vencimento hoje
                        dueDate = !paymentMethod.AllowInstallments
                            ? DateTime.Now
                            : AdjustToBusinessDay(firstDueDate.AddMonths(i));
                    }

                    bool isPaid = paymentMethod.IsImmediateSettlement && installments == 1;
                    FinancialStatus status = isPaid ? FinancialStatus.paid : FinancialStatus.pending;

                    var financial = new Financial
                    {
                        Id = 0,
                        FinancialStatus = status,
                        FinancialType = FinancialType.recipe,
                        Origin = OriginFinancial.financial,
                        IdSale = idSale,
                        IdServiceOrder = idServiceOrder,
                        CreationDate = DateTime.Now,
                        DueDate = dueDate,
                        SettlementDate = isPaid ? DateTime.Now.ToString() : null,
                        IdCompany = idCompany,
                        BoxId = caixaAberto?.Id,
                        Description = installments > 1
                            ? $"Parcela {i + 1}/{installments} - {paymentMethod.Name} - {referencia} #{referenciaId}"
                            : $"{paymentMethod.Name} - {referencia} #{referenciaId}",
                        IdCostCenter = costCenterId,
                        IdClient = idClient,
                        Value = currentValue,
                        Troco = null,
                        BankAccountId = bankAccountId,
                        FinancialPaymentMethods = new List<FinancialPaymentMethod>
                        {
                            new FinancialPaymentMethod
                            {
                                PaymentMethodId = m.PaymentMethodId,
                                FinancialId = 0,
                                Amount = currentValue,
                                Installments = 1
                            }
                        }
                    };

                    await base.Create(financial);
                }
            }
        }
        /// <summary>
        /// Reconcilia o financeiro de uma ordem de serviço com as formas de pagamento informadas.
        /// A estratégia é regenerar: as parcelas pendentes são removidas (com reajuste do caixa,
        /// quando lançadas nele) e as já pagas viram <see cref="FinancialStatus.Canceled"/>, para
        /// preservar o histórico em vez de apagar um recebimento que de fato ocorreu.
        /// Ao final, gera as parcelas novas a partir de <paramref name="payments"/>.
        /// </summary>
        public async Task ReplaceServiceOrderFinancials(int idServiceOrder, int idCompany, int? idClient,
            ICollection<FormPaymentSale> payments)
        {
            var existing = await (repository as IFinancialRepository).GetByIdServiceOrderAsync(idServiceOrder);

            foreach (var financial in existing)
            {
                try
                {
                    if (financial.FinancialStatus == FinancialStatus.pending)
                    {
                        foreach (var fpm in financial.FinancialPaymentMethods)
                            await _financialPaymentMethodRepository.DeleteAsync(fpm.Id);

                        await base.DeleteAsync(financial.Id);
                    }
                    else if (financial.FinancialStatus == FinancialStatus.paid)
                    {
                        // Parcela paga não pode ser deletada: mantém a coleção de FPMs intacta
                        // para não deixar o dependente órfão no tracker do EF.
                        financial.FinancialStatus = FinancialStatus.Canceled;
                        await base.Alter(financial);
                    }

                    // O caixa é recalculado DEPOIS de a parcela sair do saldo:
                    // AjustarCaixaEdicaoVendaAsync relê o que está gravado e descarta as
                    // canceladas, então chamá-lo antes somaria ao SaldoCalculado justamente
                    // a parcela que está sendo anulada.
                    if (financial.BoxId != null)
                        await _boxService.AjustarCaixaEdicaoVendaAsync((int)financial.BoxId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            await GenerateFinancialCentral(payments, idCompany,
                idServiceOrder: idServiceOrder, idClient: idClient,
                descricaoOrigem: "Ordem de Serviço");
        }

        /// <summary>
        /// Anula os recebíveis de uma ordem de serviço cancelada. Sem isto a OS ficaria
        /// cancelada mas com parcelas em aberto no financeiro.
        /// Diferente da reconciliação de edição (que apaga as pendentes porque elas serão
        /// regeradas), aqui todas as parcelas viram <see cref="FinancialStatus.Canceled"/>:
        /// a dívida existiu e está sendo anulada, então o registro precisa continuar
        /// auditável em vez de desaparecer do banco. Parcelas renegociadas ficam intactas —
        /// elas já foram substituídas por outras sem vínculo com esta OS, e cancelá-las
        /// quebraria a cadeia registrada em <see cref="FinancialResources"/>.
        /// Idempotente: parcelas já canceladas são ignoradas.
        /// </summary>
        public async Task CancelServiceOrderFinancials(int idServiceOrder)
        {
            var existing = await (repository as IFinancialRepository).GetByIdServiceOrderAsync(idServiceOrder);

            foreach (var financial in existing)
            {
                if (financial.FinancialStatus == FinancialStatus.Canceled ||
                    financial.FinancialStatus == FinancialStatus.renegotiated)
                    continue;

                financial.FinancialStatus = FinancialStatus.Canceled;
                await base.Alter(financial);

                // Recalculado depois da anulação, pelo mesmo motivo da reconciliação:
                // o saldo do caixa não pode continuar contando uma parcela já cancelada.
                if (financial.BoxId != null)
                    await _boxService.AjustarCaixaEdicaoVendaAsync((int)financial.BoxId);
            }
        }

        public async Task<List<Financial>> GetByIdServiceOrderAsync(int id)
        {
            return await (repository as IFinancialRepository).GetByIdServiceOrderAsync(id);
        }

        private DateTime GetFirstDueDate()
        {
            return AdjustToBusinessDay(DateTime.Now.AddDays(30));
        }

        private DateTime AdjustToBusinessDay(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
                return date.AddDays(2);
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return date.AddDays(1);
            return date;
        }
        public async Task<List<Financial>> GetByIdPurchaseAsync(int id)
        {
            return await (repository as IFinancialRepository).GetByIdPurchaseAsync(id);
        }
    }
    public interface IFinancialService : IBaseService<Financial>
    {
        Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem);
        Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters);
        Task DeleteFinancial(int id);
        Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters);
        Task<List<Financial>> GetByIdCompany(Filters filters);
        Task AlterFinancial(FinancialRequest financial);
        Task<List<Financial>> GetByIdSaleAsync(int id);
        Task<List<int>> CreateFinancial(FinancialInstallmentRequest financial);
        Task<PagedResultWithTotals> GetPaged(Filters filters);
        Task AlterFinancialStatus(Financial financial);
        Task<PagedResult<FinancialResponse>> GetPagedByIdClient(Filters filters);
        Task CreateRenegotiationAsync(RenegotiationRequestDto request);
        Task<List<Financial>> GetByIdPurchaseAsync(int id);

        Task GenerateFinancialCentral(ICollection<FormPaymentSale> formPaymentSales, int idCompany,
            int? idSale = null, int? idServiceOrder = null, int? idClient = null, int? bankAccountId = null,
            string? descricaoOrigem = null);

        Task ReplaceServiceOrderFinancials(int idServiceOrder, int idCompany, int? idClient,
            ICollection<FormPaymentSale> payments);

        Task CancelServiceOrderFinancials(int idServiceOrder);

        Task<List<Financial>> GetByIdServiceOrderAsync(int id);
    }
}