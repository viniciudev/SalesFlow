


using Model.Moves;
using System.Collections.Generic;

namespace Model.Registrations
{
	public class PaymentMethod : BaseEntity
	{
		public string Name { get; set; }
		public int IdCompany { get; set; }
		public Company Company { get; set; }
		public TipoFormaPagamento PaymentType { get; set; }
		public bool AllowInstallments { get; set; } = false; // Permite parcelamento?
		public bool IsImmediateSettlement { get; set; } = true; // 
		
		public virtual ICollection<FinancialPaymentMethod> FinancialPaymentMethods { get; set; } = new List<FinancialPaymentMethod>();
				
	}

	 public enum TipoFormaPagamento
    {
        /// <summary>
        /// 01 - Dinheiro
        /// </summary>
        fpDinheiro = 01,

        /// <summary>
        /// 02 - Cheque
        /// </summary>
        fpCheque = 02,

        /// <summary>
        /// 03 - Cartão de Crédito
        /// </summary>
        fpCartaoCredito = 03,

        /// <summary>
        /// 04 - Cartão de Débito
        /// </summary>
        fpCartaoDebito = 04,
        

        /// <summary>
        /// 5 - Vale Alimentação
        /// </summary>
        fpValeAlimentacao = 5,

        /// <summary>
        /// 6 - Vale Refeição
        /// </summary>
        fpValeRefeicao = 6,

        /// <summary>
        /// 7  -Vale Presente
        /// </summary>
        fpValePresente = 7,

        /// <summary>
        /// 8 - Vale Combustível
        /// </summary>
        fpValeCombustivel = 8,

       

        /// <summary>
        /// 9 - Boleto Bancário
        /// </summary>
        fpBoletoBancario = 9,

        /// <summary>
        /// 10 - Depósito Bancário
        /// </summary>
        fpDepositoBancario = 10,

        /// <summary>
        /// 11 - Pagamento Instantâneo (PIX) dinâmico
        /// </summary>
        fpPagamentoInstantaneoPIXDinamico = 11,

        /// <summary>
        /// 12 - Transferência bancária, Carteira Digital
        /// </summary>
        fpTransferenciabancaria = 12,

        /// <summary>
        /// 13 - Programa de fidelidade, Cashback, Crédito Virtual
        /// </summary>
         fpProgramadefidelidade = 13,

        /// <summary>
        /// 14 - Pagamento Instantâneo (PIX) estático
        /// </summary>
        fpPagamentoInstantaneoPIXEstatico = 14,
        /// <summary>
        /// 99 - Outros
        /// </summary>
        fpOutro = 99
    }
}
