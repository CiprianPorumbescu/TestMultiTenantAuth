using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestMultiTenantAuth.OpenIddict
{
    public class TenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentTenant()
        {
            // This sample uses the path base as the tenant.
            // You can replace that by your own logic.
            string tenant = _httpContextAccessor.HttpContext.Request.PathBase;
            if (string.IsNullOrEmpty(tenant))
            {
                tenant = "default";
            }

            return tenant;
        }
    }
}
