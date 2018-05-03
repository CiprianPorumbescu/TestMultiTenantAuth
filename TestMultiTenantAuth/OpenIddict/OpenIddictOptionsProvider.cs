using Microsoft.Extensions.Options;
using OpenIddict;
using System;
using System.Collections.Concurrent;

namespace TestMultiTenantAuth.OpenIddict
{
    public class OpenIddictOptionsProvider : IOptionsMonitor<OpenIddictOptions>
    {
        private readonly ConcurrentDictionary<(string name, string tenant), Lazy<OpenIddictOptions>> _cache;
        private readonly IOptionsFactory<OpenIddictOptions> _optionsFactory;
        private readonly TenantProvider _tenantProvider;

        public OpenIddictOptionsProvider(
            IOptionsFactory<OpenIddictOptions> optionsFactory,
            TenantProvider tenantProvider)
        {
            _cache = new ConcurrentDictionary<(string, string), Lazy<OpenIddictOptions>>();
            _optionsFactory = optionsFactory;
            _tenantProvider = tenantProvider;
        }

        public OpenIddictOptions CurrentValue => Get(Options.DefaultName);

        public OpenIddictOptions Get(string name)
        {
            var tenant = _tenantProvider.GetCurrentTenant();

            Lazy<OpenIddictOptions> Create() => new Lazy<OpenIddictOptions>(() => _optionsFactory.Create(name));
            return _cache.GetOrAdd((name, tenant), _ => Create()).Value;
        }

        public IDisposable OnChange(Action<OpenIddictOptions, string> listener) => null;
    }
}
