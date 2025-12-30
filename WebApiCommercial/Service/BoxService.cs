using Model;
using Model.DTO;
using Model.DTO.BoxDto;
using Model.Moves;
using Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{


    public class BoxService : BaseService<Box>, IBoxService
    {
        public BoxService(IGenericRepository<Box> repository) : base(repository)
        {
        }

        public async Task<ResponseGeneric> AbrirCaixaAsync( OpenBoxDto dto)
        {
            // Validar se já existe caixa aberto

            var caixaAberto = await (repository as IBoxRepository).GetByStatus(CaixaStatus.ABERTO);


            if (caixaAberto != null)
                return new ResponseGeneric { Message = "Já existe um caixa aberto!", Success = false };

            var caixa = new Box
            {
                IdCompany=dto.IdCompany,
                UsuarioId = null,
                ValorInicial = dto.ValorInicial,
                Observacoes = dto.Observacoes,
                Status = CaixaStatus.ABERTO,
            };

       
            await repository.CreateAsync(caixa);

            return new ResponseGeneric { Message = "Caixa aberto!", Success = true,Data= caixa };
        }
        public async Task<ResponseGeneric> FecharCaixaAsync(int caixaId, CloseBoxDto dto)
        {
            Box caixa = await (repository as IBoxRepository).GetByIdBox(caixaId);

            if (caixa == null || caixa.Status == CaixaStatus.FECHADO)
                return new ResponseGeneric { Message = "Caixa não encontrado ou já fechado", Success = false };

            // Calcular saldo total das movimentações
            var totalEntradas = caixa.Movimentacoes
                .Where(m => m.FinancialType == FinancialType.recipe)
                .Sum(x => x.Value);

            var totalSaidas = caixa.Movimentacoes
                .Where(m => m.FinancialType == FinancialType.expense)
                .Sum(m => m.Value);

            var saldoCalculado = caixa.ValorInicial + totalEntradas - totalSaidas;
            var diferenca = dto.ValorFinal - saldoCalculado;

            caixa.DataFechamento = DateTime.Now;
            caixa.ValorFinal = dto.ValorFinal;
            caixa.SaldoCalculado = saldoCalculado;
            caixa.Diferenca = diferenca;
            caixa.Status = CaixaStatus.FECHADO;
            caixa.Observacoes += $"\nFechamento: {dto.Observacoes}";

            await repository.UpdateAsync(caixaId, caixa);

            return new ResponseGeneric { Message = "Caixa fechado!", Success = true, Data = caixa };
        }
        public async Task<PagedResult<Box>> GetMovimentacoesAsync(Filters filters)
        {
           return await (repository as IBoxRepository).GetPaged(filters);
        }
        public async Task<BoxStatus> GetStatusByCompany(int tenantid)
        {
            return await (repository as IBoxRepository).GetStatusByCompany(tenantid);
        }
    }
    public interface IBoxService : IBaseService<Box>
    {
        Task<ResponseGeneric> AbrirCaixaAsync(OpenBoxDto dto);
        Task<ResponseGeneric> FecharCaixaAsync(int caixaId, CloseBoxDto dto);
        Task<PagedResult<Box>> GetMovimentacoesAsync(Filters filters);
        Task<BoxStatus> GetStatusByCompany(int tenantid);
    }
}
