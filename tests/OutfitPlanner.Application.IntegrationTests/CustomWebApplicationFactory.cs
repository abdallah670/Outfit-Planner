using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutfitPlanner.Api;
using OutfitPlanner.Persistence;
using System.Net.Http.Headers;

namespace OutfitPlanner.Application.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext and use InMemory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));

            // Override other services if needed (e.g. SearchService mocks)
        });
    }

    public static async Task<HttpClient> AuthenticatedClient(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        
        // Add test JWT token - assume simple bearer for tests
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "test-jwt-token-for-userid-test-user-id");

        return client;
    }
}

public static class HttpClientExtensions
{
    public static async Task<HttpClient> AuthenticateAsync(this HttpClient client)
    {
        // For compatibility with existing tests
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "test-jwt-token-for-userid-test-user-id");
        return client;
    }
}
