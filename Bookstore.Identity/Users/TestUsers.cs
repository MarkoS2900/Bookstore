using Duende.IdentityServer.Test;
using System.Security.Claims;

namespace Bookstore.Identity.Users;

public static class TestUsers
{
    public static List<TestUser> Users =>
    [
        new TestUser
        {
            SubjectId = "1",
            Username = "testuser",
            Password = "Password123!",
            Claims =
            [
                new Claim("name", "Test User"),
                new Claim("role", "User")
            ]
        }
    ];
}