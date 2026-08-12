using System;
using System.Collections.Generic;

namespace Model.DTO
{
    /// <summary>
    /// Request para criação de lançamentos financeiros com suporte a parcelamento.
    /// Mantém compatibilidade com o FinancialRequest (registro simples),
    /// herdando todos os dados base e adicionando as informações de parcelas.
    /// </summary>
    public class FinancialInstallmentRequest : FinancialRequest
    {
        /// <summary>
        /// Número de parcelas. Quando maior que 1, o sistema gera múltiplos
        /// registros financeiros dividindo o valor total em partes iguais.
        /// Padrão: 1 (registro simples, sem parcelamento).
        /// </summary>
        public int NumberOfInstallments { get; set; } = 1;

        /// <summary>
        /// Intervalo em dias entre os vencimentos das parcelas.
        /// Usado somente quando InstallmentDueDates não é informado.
        /// Ex.: valor total R$ 1.000,00, 2 parcelas, intervalo 30 dias
        /// => parcela 1: hoje, parcela 2: hoje + 30 dias.
        /// </summary>
        public int InstallmentIntervalDays { get; set; } = 30;

        /// <summary>
        /// Datas de vencimento manuais para cada parcela (opcional).
        /// Quando informado e com o mesmo tamanho de NumberOfInstallments,
        /// substitui o cálculo automático por intervalo de dias.
        /// </summary>
        public List<DateTime>? InstallmentDueDates { get; set; }
    }
}
