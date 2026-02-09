// Middleware/PermissionMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.Enums;
using Repository;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionMiddleware> _logger;

    public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var permissionAttribute = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (permissionAttribute != null)
        {
            _logger.LogInformation($"Validando permissão: {permissionAttribute.PermissionCode} para rota: {context.Request.Path}");

            // Verifica se o usuário está autenticado
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Tentativa de acesso não autenticada");
                await ReturnErrorResponse(context, 401, "Autenticação necessária",
                    "Você precisa fazer login para acessar este recurso.");
                return;
            }

            // Tenta obter o ID do usuário de várias claims possíveis
            var userId = GetUserIdFromClaims(context.User);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("ID do usuário não encontrado nas claims");
                await ReturnErrorResponse(context, 401, "Informações do usuário incompletas",
                    "Não foi possível identificar o usuário. Faça login novamente.");
                return;
            }

            // Obtém o repositório via service provider
            using var scope = context.RequestServices.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserPermissionRepository>();

            try
            {
                // Verifica se usuário tem a permissão
                var hasPermission = await repository.UserPermissions(userId, permissionAttribute.PermissionCode);

                if (!hasPermission)
                {
                    var permissionName = GetPermissionFriendlyName(permissionAttribute.PermissionCode);

                    _logger.LogWarning($"Acesso negado para usuário {userId}. Permissão necessária: {permissionAttribute.PermissionCode}");

                    await ReturnErrorResponse(context, 403, "Acesso negado",
                        $"Você não tem permissão para executar esta ação. Permissão necessária: {permissionName}",
                        new
                        {
                            requiredPermission = permissionAttribute.PermissionCode.ToString(),
                            requiredPermissionName = permissionName,
                            userId = userId
                        });
                    return;
                }

                _logger.LogInformation($"Permissão validada com sucesso para usuário {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao validar permissão para usuário {userId}");
                await ReturnErrorResponse(context, 500, "Erro interno",
                    "Ocorreu um erro ao verificar suas permissões. Tente novamente mais tarde.");
                return;
            }
        }

        await _next(context);
    }

    private string GetUserIdFromClaims(ClaimsPrincipal user)
    {
        // Ordem de prioridade para buscar o ID do usuário
        var possibleClaimTypes = new[]
        {
            "UserId",                    // Claim personalizada
            ClaimTypes.NameIdentifier,   // Claim padrão do ASP.NET
            "sub",                       // Claim padrão JWT
            "userId",                    // Outra variação comum
            "uid"                        // Mais uma variação
        };

        foreach (var claimType in possibleClaimTypes)
        {
            var claimValue = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrEmpty(claimValue))
            {
                return claimValue;
            }
        }

        return null;
    }

    private string GetPermissionFriendlyName(PermissionEnum permission)
    {
        // Mapeia códigos de permissão para nomes amigáveis
        return permission switch
        {
            PermissionEnum.CADASTRO_EMPRESA_VIEW => "Visualizar Empresa",
            PermissionEnum.CADASTRO_EMPRESA_EDIT => "Editar Empresa",
            PermissionEnum.CADASTRO_PRODUTO_VIEW => "Visualizar Produtos",
            PermissionEnum.CADASTRO_PRODUTO_CREATE => "Criar Produto",
            PermissionEnum.CADASTRO_PRODUTO_EDIT => "Editar Produto",
            PermissionEnum.CADASTRO_PRODUTO_DELETE => "Excluir Produto",
            PermissionEnum.CADASTRO_CLIENTE_VIEW => "Visualizar Clientes",
            PermissionEnum.CADASTRO_CLIENTE_CREATE => "Criar Cliente",
            PermissionEnum.CADASTRO_CLIENTE_EDIT => "Editar Cliente",
            PermissionEnum.VENDA_VIEW => "Visualizar Vendas",
            PermissionEnum.VENDA_CREATE => "Criar Venda",
            PermissionEnum.VENDA_CANCELAR => "Cancelar Venda",
            PermissionEnum.VENDA_ALTER => "Alterar Venda",
            PermissionEnum.FINANCEIRO_VIEW => "Visualizar Financeiro",
            PermissionEnum.FINANCEIRO_EDIT => "Editar Financeiro",
            PermissionEnum.ESTOQUE_VIEW => "Visualizar Estoque",
            PermissionEnum.ESTOQUE_EDIT => "Ajustar Estoque",
            PermissionEnum.RENEGOCIACAO_VIEW => "Visualizar Renegociações",
            PermissionEnum.RENEGOCIACAO_CREATE => "Criar Renegociação",
            PermissionEnum.USUARIO_VIEW => "Visualizar Usuários",
            PermissionEnum.USUARIO_MANAGER => "Gerenciar Usuários",
            PermissionEnum.USUARIO_PERMISSION_MANAGER => "Gerenciar Permissões",
            PermissionEnum.FORMA_PAGAMENTO_VIEW => "Visualizar Formas de Pagamento",
            PermissionEnum.FORMA_PAGAMENTO_MANAGER => "Gerenciar Formas de Pagamento",
            _ => permission.ToString()
        };
    }

    private async Task ReturnErrorResponse(HttpContext context, int statusCode, string title, string message, object details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            error = new
            {
                code = statusCode,
                title = title,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path,
                traceId = context.TraceIdentifier
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

// Attributes/RequirePermissionAttribute.cs
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute
{
    public PermissionEnum PermissionCode { get; }

    public RequirePermissionAttribute(PermissionEnum permissionCode)
    {
        PermissionCode = permissionCode;
    }
}