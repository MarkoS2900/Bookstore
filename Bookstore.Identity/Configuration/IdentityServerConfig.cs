using Duende.IdentityServer.Models;

namespace Bookstore.Identity.Configuration;

public static class IdentityServerConfig
{
    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope("bookstore.read", "Read books and authors"),
        new ApiScope("bookstore.write", "Create authors and create, update, and delete books")
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("bookstore-api", "Bookstore API")
        {
            Scopes = { "bookstore.read", "bookstore.write" }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "bookstore-client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets =
            {
                new Secret("bookstore-dev-secret".Sha256())
            },

            AllowedScopes =
            {
                "bookstore.read",
                "bookstore.write"
            }
        },
        new Client
        {
            ClientId = "bookstore-swagger",
            ClientName = "Bookstore Swagger UI",

            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,

            RedirectUris =
            {
                "https://localhost:7032/swagger/oauth2-redirect.html"
            },

            AllowedCorsOrigins =
            {
                "https://localhost:7032"
            },

            AllowedScopes =
            {
                "bookstore.read",
                "bookstore.write"
            }
        }
    ];
}