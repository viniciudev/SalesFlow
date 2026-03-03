namespace Model.Enums
{
    // Models/Enums/PermissionEnum.cs
    public enum PermissionEnum
    {
        // Cadastros
        CADASTRO_EMPRESA_VIEW = 1,
        CADASTRO_EMPRESA_EDIT = 2,

        CADASTRO_PRODUTO_VIEW = 10,
        CADASTRO_PRODUTO_CREATE = 11,
        CADASTRO_PRODUTO_EDIT = 12,
        CADASTRO_PRODUTO_DELETE = 13,

        CADASTRO_CLIENTE_VIEW = 20,
        CADASTRO_CLIENTE_CREATE = 21,
        CADASTRO_CLIENTE_EDIT = 22,

        // Vendas
        VENDA_VIEW = 30,
        VENDA_CREATE = 31,
        VENDA_CANCELAR = 32,

        // Financeiro
        FINANCEIRO_VIEW = 40,
        FINANCEIRO_EDIT = 41,
        FINANCEIRO_CREATE=88,
        FINANCEIRO_DELETE=99,

        // Estoque
        ESTOQUE_VIEW = 50,
        ESTOQUE_EDIT = 51,
        ESTOQUE_CREATE=93,
        ESTOQUE_DELETE=94,

        // Renegociação
        RENEGOCIACAO_VIEW = 60,
        RENEGOCIACAO_CREATE = 61,

        // Usuários
        USUARIO_VIEW = 70,
        USUARIO_MANAGER = 71,
        USUARIO_PERMISSION_MANAGER = 72, // Permissão especial para gerenciar permissões

        // Forma de Pagamento
        FORMA_PAGAMENTO_VIEW = 80,
        FORMA_PAGAMENTO_MANAGER = 81,
        VENDA_ALTER = 82,

        //contas bancárias
        CONTA_BANCARIA_VIEW = 83,
        CONTA_BANCARIA_CREATE = 84,
        CONTA_BANCARIA_EDIT = 85,
        //abertura de caixa
        CAIXA_VIEW = 86,
        CAIXA_CREATE = 87,
//configuração emissão de nota fiscal
        CONF_NOTA_FISCAL_VIEW = 89,
        CONF_NOTA_FISCAL_CREATE = 90,
        CONF_NOTA_FISCAL_EDIT = 91,
        CONF_NOTA_FISCAL_DELETE = 92,
        // emissao de nota fiscal
        NOTA_FISCAL_VIEW = 95,
        NOTA_FISCAL_CREATE = 96,
        NOTA_FISCAL_CANCEL = 97,
        NOTA_FISCAL_EDIT = 98


    }
}
