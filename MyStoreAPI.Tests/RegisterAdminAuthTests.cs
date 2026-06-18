using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MyStoreAPI.Tests;

public class RegisterAdminAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RegisterAdminAuthTests(WebApplicationFactory<Program> factory)
    {
        // Boot the API in memory. Inject a valid dummy Base64 JWT:Secret (and the
        // related JWT settings) so authentication can configure its signing key and
        // startup succeeds without real secrets.
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // 32 raw bytes, Base64-encoded — valid for HMAC-SHA256.
                    ["JWT:Secret"] = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=",
                    ["JWT:ValidIssuer"] = "https://test.local",
                    ["JWT:ValidAudience"] = "https://test.local",
                    ["JWT:TokenExpiryTimeInHour"] = "1",
                });
            });
        });
    }

    [Fact]
    public async Task RegisterAdmin_WithoutAuthentication_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register-admin", new
        {
            username = "adminuser",
            email = "admin@example.com",
            password = "P@ssw0rd!",
            firstName = "Admin",
            lastName = "User"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
