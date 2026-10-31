using System.Net;
using System.Net.Http.Json;
using Bookstore.Api.Data;
using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Api.Tests.IntegrationTests.Endpoints;

public sealed class AuthorsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoAuthorsExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authors = await response.Content.ReadFromJsonAsync<List<AuthorResponse>>();

        Assert.NotNull(authors);
        Assert.Empty(authors);
    }

    [Fact]
    public async Task GetAll_ReturnsAllAuthors()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        await SeedAuthorAsync(factory, "George Orwell");
        await SeedAuthorAsync(factory, "Jane Austen");

        var response = await client.GetAsync("/api/v1/authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authors = await response.Content.ReadFromJsonAsync<List<AuthorResponse>>();

        Assert.NotNull(authors);
        Assert.Equal(2, authors.Count);
        Assert.Contains(authors, a => a.Name == "George Orwell");
        Assert.Contains(authors, a => a.Name == "Jane Austen");
    }

    [Fact]
    public async Task GetById_ReturnsAuthor_WhenAuthorExists()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");

        var response = await client.GetAsync($"/api/v1/authors/{authorId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var author = await response.Content.ReadFromJsonAsync<AuthorResponse>();

        Assert.NotNull(author);
        Assert.Equal(authorId, author.AuthorId);
        Assert.Equal("George Orwell", author.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenAuthorDoesNotExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/authors/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAuthor()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var request = new CreateAuthorRequest { Name = "George Orwell" };

        var response = await client.PostAsJsonAsync("/api/v1/authors", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var author = await response.Content.ReadFromJsonAsync<AuthorResponse>();

        Assert.NotNull(author);
        Assert.True(author.AuthorId > 0);
        Assert.Equal("George Orwell", author.Name);

        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/v1/Authors/{author.AuthorId}", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("A")]
    public async Task Create_ReturnsBadRequest_WhenNameIsInvalid(string name)
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var request = new CreateAuthorRequest { Name = name };

        var response = await client.PostAsJsonAsync("/api/v1/authors", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        Assert.Equal(0, await dbContext.Authors.CountAsync());
    }

    [Fact]
    public async Task Requests_ReturnUnauthorized_WhenNotAuthenticated()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/authors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenWriteScopeIsMissing()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-Test-Scopes", "bookstore.read");

        var request = new CreateAuthorRequest { Name = "George Orwell" };

        var response = await client.PostAsJsonAsync("/api/v1/authors", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<int> SeedAuthorAsync(BookstoreApiFactory factory, string name)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        var author = new Author { Name = name };
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        return author.AuthorId;
    }
}