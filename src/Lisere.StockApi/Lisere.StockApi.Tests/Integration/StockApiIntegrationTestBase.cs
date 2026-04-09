using System.Net.Http.Headers;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

public abstract class StockApiIntegrationTestBase : IClassFixture<StockApiWebApplicationFactory>
{
    protected readonly StockApiWebApplicationFactory Factory;

    protected StockApiIntegrationTestBase(StockApiWebApplicationFactory factory)
        => Factory = factory;

    protected HttpClient ClientWithRole(string role)
    {
        var token = JwtTokenHelper.GenerateToken(
            role,
            StockApiWebApplicationFactory.JwtSecret,
            StockApiWebApplicationFactory.JwtIssuer,
            StockApiWebApplicationFactory.JwtAudience);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
