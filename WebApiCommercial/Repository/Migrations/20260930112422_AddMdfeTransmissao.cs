using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <summary>
    /// MDF-e — fase 2: o que a SEFAZ devolve na transmissão e o que os eventos
    /// de encerramento e cancelamento produzem.
    ///
    /// <b>Esta migration foi escrita à mão.</b> O scaffold do EF gerou, além das
    /// 9 colunas abaixo, 45 <c>AlterColumn</c> de data espalhados por 30 tabelas
    /// de outros módulos (<c>tb_client</c>, <c>tb_box</c>, <c>tb_nfeEmission</c>,
    /// <c>tb_driver_license</c>…). Foram todos REMOVIDOS.
    ///
    /// É o mesmo drift já documentado no topo da <c>AddMdfe</c>: o modelo de
    /// design time mapeia <c>DateTime</c> para <c>timestamp without time zone</c>
    /// e o banco tem <c>timestamp with time zone</c> em todas as colunas de data,
    /// então todo <c>migrations add</c> propõe converter o schema inteiro. Nada
    /// disso tem relação com o MDF-e, e converter mexeria em dado real de outros
    /// módulos. A decisão de não zerar o drift continua a mesma — ver a nota da
    /// <c>AddMdfe</c>, que explica por que declarar o store type não é inócuo.
    ///
    /// <b>Atenção ao tipo das 3 colunas novas de data:</b> o scaffold as gerou
    /// como <c>timestamp without time zone</c>, e aqui elas foram trocadas para
    /// <c>timestamp with time zone</c>. Não é cosmético. As 3 colunas de data que
    /// já existem em <c>tb_mdfe</c> (<c>DataEmissao</c>, <c>CreatedAt</c>,
    /// <c>UpdatedAt</c>) foram criadas como <c>timestamp with time zone</c> pela
    /// <c>AddMdfe</c>; deixar as novas como "without" faria a MESMA tabela ter os
    /// dois tipos, e o Npgsql trata os dois de forma diferente na hora de enviar
    /// o parâmetro — o valor gravado passaria a depender do TimeZone da sessão em
    /// metade das colunas de data do manifesto. Uniformidade aqui é o que mantém
    /// "DataAutorizacao" comparável com "DataEmissao".
    ///
    /// Conteúdo real: 9 colunas novas em <c>tb_mdfe</c>, todas anuláveis exceto
    /// <c>SequenciaEvento</c> (que começa em 0).
    /// </summary>
    public partial class AddMdfeTransmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CStat",
                table: "tb_mdfe",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAutorizacao",
                table: "tb_mdfe",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCancelamento",
                table: "tb_mdfe",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEncerramento",
                table: "tb_mdfe",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JustificativaCancelamento",
                table: "tb_mdfe",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocoloEncerramento",
                table: "tb_mdfe",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recibo",
                table: "tb_mdfe",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequenciaEvento",
                table: "tb_mdfe",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "XMotivo",
                table: "tb_mdfe",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CStat",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "DataAutorizacao",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "DataCancelamento",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "DataEncerramento",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "JustificativaCancelamento",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "ProtocoloEncerramento",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "Recibo",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "SequenciaEvento",
                table: "tb_mdfe");

            migrationBuilder.DropColumn(
                name: "XMotivo",
                table: "tb_mdfe");
        }
    }
}
