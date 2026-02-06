// Middleware/PermissionMiddleware.cs
using Microsoft.AspNetCore.Http;
using Model.Enums;
using Repository;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IUserPermissionRepository _repository;

    public PermissionMiddleware(RequestDelegate next, IUserPermissionRepository repository)
    {
        _next = next;
        _repository = repository;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var permissionAttribute = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (permissionAttribute != null)
        {
            var user = context.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Verifica se usuário tem a permissão
           
            var hasPermission = await _repository.UserPermissions(userId, permissionAttribute.PermissionCode);
                

            if (!hasPermission)
            {
                context.Response.StatusCode = 403;
                return;
            }
        }

        await _next(context);
    }
}

// Attributes/RequirePermissionAttribute.cs
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute
{
    public PermissionEnum PermissionCode { get; }

    public RequirePermissionAttribute(PermissionEnum permissionCode)
    {
        PermissionCode = permissionCode;
    }
}