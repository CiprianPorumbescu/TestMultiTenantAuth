using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OpenIddict;

namespace TestMultiTenantAuth.OpenIddict
{
    public class OpenIddictOptionsInitializer : IConfigureNamedOptions<OpenIddictOptions>
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly TenantProvider _tenantProvider;

        public OpenIddictOptionsInitializer(
            IDataProtectionProvider dataProtectionProvider,
            TenantProvider tenantProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _tenantProvider = tenantProvider;
        }

        public void Configure(string name, OpenIddictOptions options) => Configure(options);

        public void Configure(OpenIddictOptions options)
        {
            var tenant = _tenantProvider.GetCurrentTenant();

            // Create a tenant-specific data protection provider to ensure authorization codes,
            // access tokens and refresh tokens can't be read/decrypted by the other tenants.
            options.DataProtectionProvider = _dataProtectionProvider.CreateProtector(tenant);

            // Other tenant-specific options can be registered here.
        }
    }
}
