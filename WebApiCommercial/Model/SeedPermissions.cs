// Data/SeedPermissions.cs
using Model.Enums;
using Model.Registrations;
using System.Collections.Generic;

namespace Model
{
    public static class SeedPermissions
    {
        public static List<Permission> GetDefaultPermissions()
        {
            return new List<Permission>
            {
                // Cadastros - Empresa
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_EMPRESA_VIEW, // Valor do Enum: 1
                    Code = PermissionEnum.CADASTRO_EMPRESA_VIEW,
                    Name = "Visualizar Empresa",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_EMPRESA_EDIT, // Valor do Enum: 2
                    Code = PermissionEnum.CADASTRO_EMPRESA_EDIT,
                    Name = "Editar Empresa",
                    Category = "Cadastros"
                },
                
                // Cadastros - Produto
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_PRODUTO_VIEW, // Valor do Enum: 10
                    Code = PermissionEnum.CADASTRO_PRODUTO_VIEW,
                    Name = "Visualizar Produtos",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_PRODUTO_CREATE, // Valor do Enum: 11
                    Code = PermissionEnum.CADASTRO_PRODUTO_CREATE,
                    Name = "Criar Produto",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_PRODUTO_EDIT, // Valor do Enum: 12
                    Code = PermissionEnum.CADASTRO_PRODUTO_EDIT,
                    Name = "Editar Produto",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_PRODUTO_DELETE, // Valor do Enum: 13
                    Code = PermissionEnum.CADASTRO_PRODUTO_DELETE,
                    Name = "Excluir Produto",
                    Category = "Cadastros"
                },
                
                // Cadastros - Cliente
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_CLIENTE_VIEW, // Valor do Enum: 20
                    Code = PermissionEnum.CADASTRO_CLIENTE_VIEW,
                    Name = "Visualizar Clientes",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_CLIENTE_CREATE, // Valor do Enum: 21
                    Code = PermissionEnum.CADASTRO_CLIENTE_CREATE,
                    Name = "Criar Cliente",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.CADASTRO_CLIENTE_EDIT, // Valor do Enum: 22
                    Code = PermissionEnum.CADASTRO_CLIENTE_EDIT,
                    Name = "Editar Cliente",
                    Category = "Cadastros"
                },
                
                // Vendas
                new Permission
                {
                    Id = (int)PermissionEnum.VENDA_VIEW, // Valor do Enum: 30
                    Code = PermissionEnum.VENDA_VIEW,
                    Name = "Visualizar Vendas",
                    Category = "Vendas"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.VENDA_CREATE, // Valor do Enum: 31
                    Code = PermissionEnum.VENDA_CREATE,
                    Name = "Criar Venda",
                    Category = "Vendas"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.VENDA_CANCELAR, // Valor do Enum: 32
                    Code = PermissionEnum.VENDA_CANCELAR,
                    Name = "Cancelar Venda",
                    Category = "Vendas"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.VENDA_ALTER, // Valor do Enum: 82
                    Code = PermissionEnum.VENDA_ALTER,
                    Name = "Alterar Venda",
                    Category = "Vendas"
                },
                
                // Financeiro
                new Permission
                {
                    Id = (int)PermissionEnum.FINANCEIRO_VIEW, // Valor do Enum: 40
                    Code = PermissionEnum.FINANCEIRO_VIEW,
                    Name = "Visualizar Financeiro",
                    Category = "Financeiro"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.FINANCEIRO_EDIT, // Valor do Enum: 41
                    Code = PermissionEnum.FINANCEIRO_EDIT,
                    Name = "Editar Financeiro",
                    Category = "Financeiro"
                },
                 new Permission
                {
                    Id = (int)PermissionEnum.FINANCEIRO_CREATE,
                    Code = PermissionEnum.FINANCEIRO_CREATE,
                    Name = "Criar financeiro",
                    Category = "Financeiro"
                },
                
                // Estoque
                new Permission
                {
                    Id = (int)PermissionEnum.ESTOQUE_VIEW, // Valor do Enum: 50
                    Code = PermissionEnum.ESTOQUE_VIEW,
                    Name = "Visualizar Estoque",
                    Category = "Estoque"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.ESTOQUE_EDIT, // Valor do Enum: 51
                    Code = PermissionEnum.ESTOQUE_EDIT,
                    Name = "Ajustar Estoque",
                    Category = "Estoque"
                },
                 new Permission
                {
                    Id = (int)PermissionEnum.ESTOQUE_CREATE, 
                    Code = PermissionEnum.ESTOQUE_CREATE,
                    Name = "Criar Estoque",
                    Category = "Estoque"
                },
                                  new Permission
                {
                    Id = (int)PermissionEnum.ESTOQUE_DELETE,
                    Code = PermissionEnum.ESTOQUE_DELETE,
                    Name = "Deletar Estoque",
                    Category = "Estoque"
                },
                
                // Renegociação
                new Permission
                {
                    Id = (int)PermissionEnum.RENEGOCIACAO_VIEW, // Valor do Enum: 60
                    Code = PermissionEnum.RENEGOCIACAO_VIEW,
                    Name = "Visualizar Renegociações",
                    Category = "Renegociação"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.RENEGOCIACAO_CREATE, // Valor do Enum: 61
                    Code = PermissionEnum.RENEGOCIACAO_CREATE,
                    Name = "Criar Renegociação",
                    Category = "Renegociação"
                },
                
                // Usuários
                new Permission
                {
                    Id = (int)PermissionEnum.USUARIO_VIEW, // Valor do Enum: 70
                    Code = PermissionEnum.USUARIO_VIEW,
                    Name = "Visualizar Usuários",
                    Category = "Usuários"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.USUARIO_MANAGER, // Valor do Enum: 71
                    Code = PermissionEnum.USUARIO_MANAGER,
                    Name = "Gerenciar Usuários",
                    Category = "Usuários"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.USUARIO_PERMISSION_MANAGER, // Valor do Enum: 72
                    Code = PermissionEnum.USUARIO_PERMISSION_MANAGER,
                    Name = "Gerenciar Permissões",
                    Description = "Pode atribuir permissões a outros usuários",
                    Category = "Usuários"
                },
                
                // Forma de Pagamento
                new Permission
                {
                    Id = (int)PermissionEnum.FORMA_PAGAMENTO_VIEW, // Valor do Enum: 80
                    Code = PermissionEnum.FORMA_PAGAMENTO_VIEW,
                    Name = "Visualizar Formas de Pagamento",
                    Category = "Cadastros"
                },
                new Permission
                {
                    Id = (int)PermissionEnum.FORMA_PAGAMENTO_MANAGER, // Valor do Enum: 81
                    Code = PermissionEnum.FORMA_PAGAMENTO_MANAGER,
                    Name = "Gerenciar Formas de Pagamento",
                    Category = "Cadastros"
                },
                // Contas Bancárias
                new Permission
                {
                    Id = (int)PermissionEnum.CONTA_BANCARIA_CREATE,
                    Code = PermissionEnum.CONTA_BANCARIA_CREATE,
                    Name = "Criar conta bancária",
                    Category = "Cadastros"
                },
                 new Permission
                {
                    Id = (int)PermissionEnum.CONTA_BANCARIA_EDIT,
                    Code = PermissionEnum.CONTA_BANCARIA_EDIT,
                    Name = "editar conta bancária",
                    Category = "Cadastros"
                },
                  new Permission
                {
                    Id = (int)PermissionEnum.CONTA_BANCARIA_VIEW,
                    Code = PermissionEnum.CONTA_BANCARIA_VIEW,
                    Name = "visualizar conta bancária",
                    Category = "Cadastros"
                },
                  //CAIXA
                    new Permission
                {
                    Id = (int)PermissionEnum.CAIXA_VIEW,
                    Code = PermissionEnum.CAIXA_VIEW,
                    Name = "Ver caixa",
                    Category = "Financeiro"
                },
                  new Permission
                {
                    Id = (int)PermissionEnum.CAIXA_CREATE,
                    Code = PermissionEnum.CAIXA_CREATE,
                    Name = "Abrir caixa",
                    Category = "Financeiro"
                },
                  new Permission
                  {
                      Id=(int)PermissionEnum.FINANCEIRO_DELETE,
                      Code = PermissionEnum.FINANCEIRO_DELETE,
                      Name = "Deletar Financeiro",
                      Category="Financeiro"
                  }
                 
                  new Permission
                  {
                      Id = (int)PermissionEnum.CONF_NOTA_FISCAL_VIEW,
                    Code = PermissionEnum.CONF_NOTA_FISCAL_VIEW,
                    Name = "Visualizar Configurações de Nota Fiscal",
                    Category = "Cadastros"
                  }
                  ,                  new Permission
                  {
                        Id = (int)PermissionEnum.CONF_NOTA_FISCAL_CREATE,
                        Code = PermissionEnum.CONF_NOTA_FISCAL_CREATE,
                        Name = "Criar Configurações de Nota Fiscal",
                        Category = "Cadastros"
                  },
                  new Permission
                  {
                        Id = (int)PermissionEnum.CONF_NOTA_FISCAL_EDIT,
                        Code = PermissionEnum.CONF_NOTA_FISCAL_EDIT,
                        Name = "Editar Configurações de Nota Fiscal",
                        Category = "Cadastros"
                  },
                    new Permission
                    {
                            Id = (int)PermissionEnum.CONF_NOTA_FISCAL_DELETE,
                            Code = PermissionEnum.CONF_NOTA_FISCAL_DELETE,
                            Name = "Cancelar Configurações de Nota Fiscal",
                            Category = "Cadastros"
                    }
                    ,new Permission
                    {
                            Id = (int)PermissionEnum.NOTA_FISCAL_VIEW,
                            Code = PermissionEnum.NOTA_FISCAL_VIEW,
                            Name = "Visualizar Notas Fiscais",
                            Category = "Notas"
                    },
                    new Permission
                    {
                            Id = (int)PermissionEnum.NOTA_FISCAL_CREATE,
                            Code = PermissionEnum.NOTA_FISCAL_CREATE,
                            Name = "Criar Notas Fiscais",
                            Category = "Notas"
                    },
                    new Permission
                    {
                            Id = (int)PermissionEnum.NOTA_FISCAL_CANCEL,
                            Code = PermissionEnum.NOTA_FISCAL_CANCEL,
                            Name = "Cancelar Notas Fiscais",
                            Category = "Notas"
                    },
                    new Permission
                    {
                            Id = (int)PermissionEnum.NOTA_FISCAL_EDIT,
                            Code = PermissionEnum.NOTA_FISCAL_EDIT,
                            Name = "Editar Notas Fiscais",
                            Category = "Notas"
                    }

            };
        }
    }
}