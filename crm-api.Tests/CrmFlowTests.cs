using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CrmApi.Tests;

public class CrmFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public CrmFlowTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("USE_INMEMORY", "true");
        _factory = factory.WithWebHostBuilder(b =>
            b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemory"] = "true",
                ["Jwt:Key"] = "test-only-key-0123456789abcdef-0123456789abcdef",
                ["Jwt:Issuer"] = "crm-lite",
                ["Jwt:Audience"] = "crm-lite"
            })));
    }

    private async Task<HttpClient> AuthedClientAsync(string email, string role = "Sales")
    {
        var client = _factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Pass123!", role });
        if (reg.StatusCode == HttpStatusCode.Conflict)
        {
            var login = await client.PostAsJsonAsync("/api/auth/login",
                new { email, password = "Pass123!" });
            login.EnsureSuccessStatusCode();
            var body = await login.Content.ReadFromJsonAsync<AuthResp>(JsonOpts);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", body!.Token);
            return client;
        }
        reg.EnsureSuccessStatusCode();
        var regBody = await reg.Content.ReadFromJsonAsync<AuthResp>(JsonOpts);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", regBody!.Token);
        return client;
    }

    private sealed record AuthResp(string Token, string Email, string Role);
    private sealed record IdResp(int Id);

    [Fact]
    public async Task Register_and_login_roundtrip()
    {
        var email = $"u1_{Guid.NewGuid():N}@x.com";
        var client = _factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Pass123!", role = "Admin" });
        Assert.Equal(HttpStatusCode.OK, reg.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Pass123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var body = await login.Content.ReadFromJsonAsync<AuthResp>(JsonOpts);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task Login_with_wrong_password_is_401()
    {
        var email = $"u2_{Guid.NewGuid():N}@x.com";
        var client = await AuthedClientAsync(email);
        var bad = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Wrong!" });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
    }

    [Fact]
    public async Task Customer_create_and_get_with_deals()
    {
        var client = await AuthedClientAsync($"u3_{Guid.NewGuid():N}@x.com");
        var created = await client.PostAsJsonAsync("/api/customers",
            new { name = "Acme", email = "a@acme.co", phone = "1", company = "Acme Inc" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var cust = await created.Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var deal = await client.PostAsJsonAsync("/api/deals",
            new { customerId = cust!.Id, title = "Big deal", value = 1000m, stage = "Lead" });
        Assert.Equal(HttpStatusCode.Created, deal.StatusCode);
        var got = await client.GetAsync($"/api/customers/{cust.Id}");
        Assert.Equal(HttpStatusCode.OK, got.StatusCode);
        var text = await got.Content.ReadAsStringAsync();
        Assert.Contains("Big deal", text);
    }

    [Fact]
    public async Task Closed_deal_stage_change_returns_400()
    {
        var client = await AuthedClientAsync($"u4_{Guid.NewGuid():N}@x.com");
        var cust = await (await client.PostAsJsonAsync("/api/customers",
            new { name = "C1" })).Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var deal = await (await client.PostAsJsonAsync("/api/deals",
            new { customerId = cust!.Id, title = "D1", value = 500m, stage = "Proposal" }))
            .Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var close = await client.PostAsJsonAsync($"/api/deals/{deal!.Id}/stage",
            new { stage = "Won" });
        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        var reopen = await client.PostAsJsonAsync($"/api/deals/{deal.Id}/stage",
            new { stage = "Lead" });
        Assert.Equal(HttpStatusCode.BadRequest, reopen.StatusCode);
    }

    [Fact]
    public async Task Lost_deal_stage_change_returns_400()
    {
        var client = await AuthedClientAsync($"u5_{Guid.NewGuid():N}@x.com");
        var cust = await (await client.PostAsJsonAsync("/api/customers",
            new { name = "C2" })).Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var deal = await (await client.PostAsJsonAsync("/api/deals",
            new { customerId = cust!.Id, title = "D2", value = 250m, stage = "Lead" }))
            .Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var close = await client.PostAsJsonAsync($"/api/deals/{deal!.Id}/stage",
            new { stage = "Lost" });
        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        var move = await client.PostAsJsonAsync($"/api/deals/{deal.Id}/stage",
            new { stage = "Won" });
        Assert.Equal(HttpStatusCode.BadRequest, move.StatusCode);
    }

    [Fact]
    public async Task Pipeline_summary_counts_and_totals()
    {
        var client = await AuthedClientAsync($"u6_{Guid.NewGuid():N}@x.com");
        var res = await client.GetAsync("/api/pipeline");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();
        Assert.Contains("stages", text);
        Assert.Contains("totalValue", text);
        Assert.Contains("totalCount", text);
    }

    [Fact]
    public async Task Activity_add_and_list()
    {
        var client = await AuthedClientAsync($"u7_{Guid.NewGuid():N}@x.com");
        var cust = await (await client.PostAsJsonAsync("/api/customers",
            new { name = "C3" })).Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var deal = await (await client.PostAsJsonAsync("/api/deals",
            new { customerId = cust!.Id, title = "D3", value = 100m, stage = "Lead" }))
            .Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        var add = await client.PostAsJsonAsync($"/api/deals/{deal!.Id}/activities",
            new { kind = "Call", text = "Called customer" });
        Assert.Equal(HttpStatusCode.Created, add.StatusCode);
        var list = await client.GetAsync($"/api/deals/{deal.Id}/activities");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains("Called customer", await list.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Deals_filter_by_stage()
    {
        var client = await AuthedClientAsync($"u8_{Guid.NewGuid():N}@x.com");
        var cust = await (await client.PostAsJsonAsync("/api/customers",
            new { name = "C4" })).Content.ReadFromJsonAsync<IdResp>(JsonOpts);
        await client.PostAsJsonAsync("/api/deals",
            new { customerId = cust!.Id, title = $"Q_{Guid.NewGuid():N}", value = 10m, stage = "Qualified" });
        var res = await client.GetAsync("/api/deals?stage=Qualified");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}

