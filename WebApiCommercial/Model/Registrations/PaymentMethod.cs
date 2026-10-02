


using Model.Moves;
using System.Collections.Generic;

namespace Model.Registrations
{
	public class PaymentMethod : BaseEntity
	{
		public string Name { get; set; }
		public int IdCompany { get; set; }
		public Company Company { get; set; }

		public bool AllowInstallments { get; set; } = false; // Permite parcelamento?
		public bool IsImmediateSettlement { get; set; } = true; // 
		
		public virtual ICollection<FinancialPaymentMethod> FinancialPaymentMethods { get; set; } = new List<FinancialPaymentMethod>();
				
	}

	 public enum FormaPagamento
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
        /// 10 - Vale Alimentação
        /// </summary>
        fpValeAlimentacao = 10,

        /// <summary>
        /// 11 - Vale Refeição
        /// </summary>
        fpValeRefeicao = 11,

        /// <summary>
        /// 12  -Vale Presente
        /// </summary>
        fpValePresente = 12,

        /// <summary>
        /// 13 - Vale Combustível
        /// </summary>
        fpValeCombustivel = 13,

       

        /// <summary>
        /// 15 - Boleto Bancário
        /// </summary>
        fpBoletoBancario = 15,

        /// <summary>
        /// 16 - Depósito Bancário
        /// </summary>
        fpDepositoBancario = 16,

        /// <summary>
        /// 17 - Pagamento Instantâneo (PIX) dinâmico
        /// </summary>
        fpPagamentoInstantaneoPIXDinamico = 17,

        /// <summary>
        /// 18 - Transferência bancária, Carteira Digital
        /// </summary>
        fpTransferenciabancaria = 18,

        /// <summary>
        /// 19 - Programa de fidelidade, Cashback, Crédito Virtual
        /// </summary>
         fpProgramadefidelidade = 19,

        /// <summary>
        /// 20 - Pagamento Instantâneo (PIX) estático
        /// </summary>
       
        fpPagamentoInstantaneoPIXEstatico = 20,

        /// <summary>
        /// 21 - Crédito em loja
        /// </summary>
        fpCreditoEmLoja = 21,

     

        /// <summary>
        /// 90 - Sem pagamento
        /// </summary>
        [Description("Sem pagamento")]
        [XmlEnum("90")]
        fpSemPagamento = 90,

        /// <summary>
        /// 91 - Pagamento posterior
        /// </summary>
        [Description("Pagamento posterior")]
        [XmlEnum("91")]
        fpPagamentoPosterior = 91,

        /// <summary>
        /// 99 - Outros
        /// </summary>
        [Description("Outros")]
        [XmlEnum("99")]
        fpOutro = 99
    }
}
