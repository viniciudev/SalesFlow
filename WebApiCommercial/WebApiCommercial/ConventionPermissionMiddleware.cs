// Middleware/ConventionPermissionMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.Enums;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

public class ConventionPermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConventionPermissionMiddleware> _logger;

    public ConventionPermissionMiddleware(RequestDelegate next, ILogger<ConventionPermissionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // Ignora endpoints sem Authorize ou AllowAnonymous
        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var authorizeAttribute = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
        if (authorizeAttribute == null)
        {
            await _next(context);
            return;
        }

        try
        {
            // 1. Verifica autenticação
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await ReturnJsonErrorResponse(context, 401, "Autenticação necessária",
                    "Você precisa fazer login para acessar este recurso.");
                return;
            }

            // 2. Obtém ID do usuário
            var userId = GetUserIdFromClaims(context.User);
            if (string.IsNullOrEmpty(userId))
            {
                await ReturnJsonErrorResponse(context, 401, "Informações do usuário incompletas",
                    "Não foi possível identificar o usuário. Faça login novamente.");
                return;
            }

            // 3. Determina a permissão necessária baseada na convenção
            var requiredPermission = DetermineRequiredPermission(context);
            if (requiredPermission == null)
            {
                // Se não conseguiu determinar a permissão, permite acesso
                await _next(context);
                return;
            }

            // 4. Verifica se usuário tem a permissão
            using var scope = context.RequestServices.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserPermissionRepository>();

            var hasPermission = await repository.UserPermissions(userId, requiredPermission.Value);

            if (!hasPermission)
            {
                var permissionName = GetPermissionFriendlyName(requiredPermission.Value);

                _logger.LogWarning($"Acesso negado para usuário {userId}. Permissão necessária: {requiredPermission}");

                await ReturnJsonErrorResponse(context, 403, "Acesso negado",
                    $"Você não tem permissão para executar esta ação. Permissão necessária: {permissionName}",
                    new
                    {
                        requiredPermission = requiredPermission.Value.ToString(),
                        requiredPermissionName = permissionName,
                        userId = userId,
                        endpointPath = context.Request.Path,
                        httpMethod = context.Request.Method,
                    });
                return;
            }

            _logger.LogDebug($"Permissão {requiredPermission} validada para usuário {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao validar permissão");
            await ReturnJsonErrorResponse(context, 500, "Erro interno",
                "Ocorreu um erro ao verificar suas permissões.");
            return;
        }

        await _next(context);
    }
    private string GetPermissionFriendlyName(PermissionEnum permission)
    {
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
            _ => permission.ToString().Replace("_", " ")
        };
    }
    private string GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var possibleClaimTypes = new[]
        {
            "UserId",
            ClaimTypes.NameIdentifier,
            "sub",
            "userId",
            "uid"
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
    private PermissionEnum? DetermineRequiredPermission(HttpContext context)
    {
        var route = context.GetRouteData();
        var controller = route.Values["controller"]?.ToString();
        var action = route.Values["action"]?.ToString();
        var httpMethod = context.Request.Method;

        // Mapeamento automático baseado em convenção
        return DeterminePermissionByConvention(controller, action, httpMethod);
    }

    private PermissionEnum? DeterminePermissionByConvention(string controller, string action, string httpMethod)
    {
        if (string.IsNullOrEmpty(controller)) return null;

        // Converte controller name para formato padronizado
        var controllerKey = controller.ToUpper();

        // Mapeamento principal de controllers para prefixos de permissão
        var controllerPrefixMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Cadastros
            { "Company", "CADASTRO_EMPRESA" },
            { "Product", "CADASTRO_PRODUTO" },
            { "Client", "CADASTRO_CLIENTE" },
            { "Usuario", "USUARIO" },
            { "User", "USUARIO" },
            { "FormaPagamento", "FORMA_PAGAMENTO" },
            { "PaymentMethod", "FORMA_PAGAMENTO" },
            
            // Operacionais
            { "Venda", "VENDA" },
            { "Sale", "VENDA" },
            { "Financeiro", "FINANCEIRO" },
            { "Financial", "FINANCEIRO" },
            { "Estoque", "ESTOQUE" },
            { "Stock", "ESTOQUE" },
            { "Renegociacao", "RENEGOCIACAO" },
            { "Renegotiation", "RENEGOCIACAO" },
            
            // Configurações
            { "Permission", "USUARIO_PERMISSION" },
            { "UserPermissions", "USUARIO_PERMISSION" },
            { "Configuracao", "CONFIGURACAO" },
            { "Configuration", "CONFIGURACAO" },
        };

        if (!controllerPrefixMap.TryGetValue(controller, out var prefix))
        {
            // Tenta encontrar por contém
            var matchingKey = controllerPrefixMap.Keys.FirstOrDefault(k =>
                controller.Contains(k, StringComparison.OrdinalIgnoreCase));

            if (matchingKey != null)
            {
                prefix = controllerPrefixMap[matchingKey];
            }
            else
            {
                // Se não encontrar, usa o nome do controller em uppercase
                prefix = controller.ToUpper();
            }
        }

        // Mapeamento de ações HTTP para sufixos de permissão
        var actionSuffix = DetermineActionSuffix(action, httpMethod);

        // Tenta encontrar a permissão correspondente no enum
        var permissionName = $"{prefix}_{actionSuffix}";

        if (Enum.TryParse<PermissionEnum>(permissionName, true, out var permission))
        {
            return permission;
        }

        // Se não encontrar, tenta alternativas
        return TryFindAlternativePermission(prefix, action, httpMethod);
    }

    private string DetermineActionSuffix(string action, string httpMethod)
    {
        // Se action for fornecida, usa ela
        if (!string.IsNullOrEmpty(action))
        {
            return action.ToUpper();
        }

        // Baseado no método HTTP
        return httpMethod.ToUpper() switch
        {
            "GET" => "VIEW",
            "POST" => "CREATE",
            "PUT" => "EDIT",
            "PATCH" => "EDIT",
            "DELETE" => "DELETE",
            _ => "ACCESS"
        };
    }

    private PermissionEnum? TryFindAlternativePermission(string prefix, string action, string httpMethod)
    {
        // Tenta combinações alternativas
        var possiblePermissions = new List<string>();

        // Baseado no método HTTP
        if (httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            possiblePermissions.Add($"{prefix}_VIEW");
            possiblePermissions.Add($"{prefix}_READ");
            possiblePermissions.Add($"{prefix}_ACCESS");
        }
        else if (httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            possiblePermissions.Add($"{prefix}_CREATE");
            possiblePermissions.Add($"{prefix}_ADD");
            possiblePermissions.Add($"{prefix}_INSERT");
        }
        else if (httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                 httpMethod.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
        {
            possiblePermissions.Add($"{prefix}_EDIT");
            possiblePermissions.Add($"{prefix}_UPDATE");
            possiblePermissions.Add($"{prefix}_MODIFY");
        }
        else if (httpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            possiblePermissions.Add($"{prefix}_DELETE");
            possiblePermissions.Add($"{prefix}_REMOVE");
        }

        // Se action for fornecida, tenta combinar com prefixo
        if (!string.IsNullOrEmpty(action))
        {
            possiblePermissions.Add($"{prefix}_{action.ToUpper()}");
        }

        // Tenta cada permissão possível
        foreach (var permissionName in possiblePermissions)
        {
            if (Enum.TryParse<PermissionEnum>(permissionName, true, out var permission))
            {
                return permission;
            }
        }

        return null;
    }
    private async Task ReturnJsonErrorResponse(HttpContext context, int statusCode, string title, string message, object details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            error = new
            {
                code = statusCode,
                type = GetErrorType(statusCode),
                title = title,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                path = context.Request.Path,
                method = context.Request.Method,
                traceId = context.TraceIdentifier
            }
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
    private string GetErrorType(int statusCode)
    {
        return statusCode switch
        {
            401 => "authentication_error",
            403 => "permission_error",
            404 => "not_found_error",
            422 => "validation_error",
            500 => "internal_server_error",
            _ => "http_error"
        };
    }
  
}