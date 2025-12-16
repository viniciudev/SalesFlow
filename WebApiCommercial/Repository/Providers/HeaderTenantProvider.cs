using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Providers
{
    public interface ITenantProvider
    {
        int? GetCurrentTenantId();
    }

    public class HeaderTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentTenantId()
        {
            var context = _httpContextAccessor.HttpContext;

            // 1. Check if this is the login endpoint (e.g., /api/user/login)
            //if (context?.Request?.Path.StartsWithSegments("/api/user/login", StringComparison.OrdinalIgnoreCase))
            //{
            //    return null; // Skip tenant filtering for login
            //}

            // 2. Try to get TenantId from a custom header
            if (context?.Request.Headers.TryGetValue("tenantid", out var tenantIdHeader) == true)
            {
                return int.Parse( tenantIdHeader);
            }

            // 3. Optional: Add other resolution strategies (e.g., from JWT claim, subdomain)
            // For example, from a claim:
            // var tenantClaim = context.User.FindFirst("tenant_id");
            // return tenantClaim?.Value;

            // If no tenant is found, you might throw an exception or return a default.
            // For this example, we return null, and the DbContext will handle it.
            return null;
        }
    }
}
