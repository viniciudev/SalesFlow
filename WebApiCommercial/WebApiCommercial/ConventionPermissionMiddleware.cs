// Middleware/ConventionPermissionMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.Enums;
using Model.Registrations;
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

    private static readonly Dictionary<string, string> _controllerPermissionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // === CADASTROS BÁSICOS ===
        { "Client", "CADASTRO_CLIENTE" },
        { "Product", "CADASTRO_PRODUTO" },
        { "Company", "CADASTRO_EMPRESA" },
        { "User", "USUARIO" },
        { "PaymentMethod", "FORMA_PAGAMENTO" },
         { "BankAccount", "CONTA_BANCARIA" },
        
        // === OPERACIONAIS ===
        { "Sale", "VENDA" },
        { "Financial", "FINANCEIRO" },
        { "Stock", "ESTOQUE" },
        { "Budget", "ORCAMENTO" }, // Se tiver no enum
        { "Commission", "COMISSAO" }, // Se tiver no enum
        
        // === PERMISSÕES ===
        { "Permission", "USUARIO_PERMISSION" },
        { "UserPermissions", "USUARIO_PERMISSION" },
        
        // === CAIXA ===
        { "Box", "CAIXA" }, // Se tiver no enum
        
        // === DASHBOARD ===
        { "Dashboard", "DASHBOARD" }, // Se tiver no enum
    };

    // 🔥 Controllers que NÃO exigem permissão (públicos)
    private static readonly HashSet<string> _publicControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "SearchZipCode",
        "Email",
        "Home",
     
    };

    // 🔥 Ações que NÃO exigem permissão (endpoints públicos)
    private static readonly HashSet<string> _publicActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "verify-email",
        "verifyEmail",
        "forgot-password",
        "forgotPassword",
        "reset-password",
        "resetPassword",
        "authenticate",
        ""
    };

    public ConventionPermissionMiddleware(RequestDelegate next, ILogger<ConventionPermissionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // === PASSO 1: IGNORA ENDPOINTS ALLOWANONYMOUS ===
        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var routeData = context.GetRouteData();
        var controller = routeData.Values["controller"]?.ToString();
        var action = routeData.Values["action"]?.ToString();
        var httpMethod = context.Request.Method;

        // === PASSO 2: DEBUG LOG ===
        _logger.LogDebug($"🔍 Request: Controller={controller}, Action={action}, Method={httpMethod}, Path={context.Request.Path}");

        // === PASSO 3: VERIFICA SE É CONTROLLER PÚBLICO ===
        if (!string.IsNullOrEmpty(controller) && _publicControllers.Contains(controller))
        {
            _logger.LogDebug($"✅ Controller público: {controller}");
            await _next(context);
            return;
        }

        // === PASSO 4: VERIFICA SE É AÇÃO PÚBLICA ===
        if (!string.IsNullOrEmpty(action) && _publicActions.Contains(action))
        {
            _logger.LogDebug($"✅ Ação pública: {action}");
            await _next(context);
            return;
        }

        // === PASSO 5: VERIFICA AUTENTICAÇÃO ===
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await ReturnJsonErrorResponse(context, 401, "Autenticação necessária",
                "Você precisa fazer login para acessar este recurso.");
            return;
        }

        // === PASSO 6: OBTÉM ID DO USUÁRIO ===
        var userId = GetUserIdFromClaims(context.User);
        if (string.IsNullOrEmpty(userId))
        {
            await ReturnJsonErrorResponse(context, 401, "Informações do usuário incompletas",
                "Não foi possível identificar o usuário. Faça login novamente.");
            return;
        }

        // === PASSO 7: DETERMINA PERMISSÃO NECESSÁRIA ===
        var requiredPermission = DetermineRequiredPermission(controller, action, httpMethod);

        if (requiredPermission == null)
        {
            // Se não encontrou permissão, NÃO PERMITE por segurança
            _logger.LogWarning($"⚠️ Permissão não mapeada para {controller}.{action} - BLOQUEADO!");
            await ReturnJsonErrorResponse(context, 403, "Acesso negado",
                $"Acesso negado. Recurso não configurado: {controller}.{action}");
            return;
        }

        // === PASSO 8: VERIFICA PERMISSÃO NO BANCO ===
        using var scope = context.RequestServices.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserPermissionRepository>();
        if(requiredPermission.Value== PermissionEnum.USUARIO_MANAGER)
        {

        }
        var hasPermission = await repository.UserPermissions(userId, requiredPermission.Value);

        if (!hasPermission)
        {
            var permissionName = GetPermissionFriendlyName(requiredPermission.Value);

            _logger.LogWarning($"❌ Acesso negado para usuário {userId}. Permissão necessária: {requiredPermission}");

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

        _logger.LogDebug($"✅ Permissão {requiredPermission} validada para usuário {userId}");
        await _next(context);
    }

    // 🔥 DETERMINA A PERMISSÃO BASEADA NO CONTROLLER E MÉTODO HTTP
    private PermissionEnum? DetermineRequiredPermission(string controller, string action, string httpMethod)
    {
        if (string.IsNullOrEmpty(controller)) return null;

        // === PASSO 1: OBTÉM O PREFIXO DO CONTROLLER ===
        if (!_controllerPermissionMap.TryGetValue(controller, out var prefix))
        {
            _logger.LogWarning($"⚠️ Controller não mapeado: {controller}");
            return null;
        }

        // === PASSO 2: DETERMINA O SUFIXO BASEADO NO MÉTODO HTTP ===
        // IMPORTANTE: IGNORA O NOME DA ACTION! Usa apenas o verbo HTTP
        var suffix = httpMethod.ToUpper() switch
        {
            "GET" => "VIEW",
            "POST" => "CREATE",
            "PUT" => "EDIT",
            "PATCH" => "EDIT",
            "DELETE" => "DELETE",
            _ => "ACCESS"
        };

        // === PASSO 3: TENTA A PERMISSÃO PRINCIPAL ===
        var permissionName = $"{prefix}_{suffix}";
        _logger.LogDebug($"🔍 Tentando permissão: {permissionName}");

        if (Enum.TryParse<PermissionEnum>(permissionName, true, out var permission))
        {
            return permission;
        }

        // === PASSO 4: TENTA ALTERNATIVAS COMUNS ===
        return TryFindAlternativePermission(prefix, httpMethod);
    }

    // 🔥 ALTERNATIVAS DE PERMISSÃO PARA O MESMO PREFIXO
    private PermissionEnum? TryFindAlternativePermission(string prefix, string httpMethod)
    {
        var alternatives = httpMethod.ToUpper() switch
        {
            "GET" => new[] { "VIEW", "READ", "ACCESS", "LIST", "GET", "MANAGER" },
            "POST" => new[] { "CREATE", "ADD", "INSERT", "NEW", "POST", "MANAGER" },
            "PUT" or "PATCH" => new[] { "EDIT", "UPDATE", "MODIFY", "ALTER", "PUT", "MANAGER" },
            "DELETE" => new[] { "DELETE", "REMOVE", "EXCLUDE", "DEL", "MANAGER" },
            _ => new[] { "ACCESS" }
        };

        foreach (var alt in alternatives)
        {
            var permissionName = $"{prefix}_{alt}";
            if (Enum.TryParse<PermissionEnum>(permissionName, true, out var permission))
            {
                _logger.LogDebug($"✅ Permissão alternativa encontrada: {permissionName}");
                return permission;
            }
        }

        return null;
    }

    // 🔥 MÉTODO PARA ADICIONAR NOVOS MAPEAMENTOS EM TEMPO DE EXECUÇÃO
    public static void RegisterControllerPermission(string controller, string permissionPrefix)
    {
        _controllerPermissionMap[controller] = permissionPrefix;
    }

    public static void RegisterPublicController(string controller)
    {
        _publicControllers.Add(controller);
    }

    public static void RegisterPublicAction(string action)
    {
        _publicActions.Add(action);
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
            PermissionEnum.CONTA_BANCARIA_CREATE=>"Criar conta bancária",
            PermissionEnum.CONTA_BANCARIA_EDIT=>"Editar conta bancária",
            PermissionEnum.CONTA_BANCARIA_VIEW=>"Visualizar conta bancária",
            PermissionEnum.CAIXA_CREATE=>"Abrir o caixa",
            PermissionEnum.CAIXA_VIEW=>"Ver caixa",
            PermissionEnum.FINANCEIRO_CREATE=>"Criar Financeiro",
            PermissionEnum.FINANCEIRO_DELETE=>"Deletar Financeiro",
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