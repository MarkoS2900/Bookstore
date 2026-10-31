using System.Net;
using System.Net.Http.Json;
using Bookstore.Api.Data;
using Bookstore.Api.DTO.Books;
using Bookstore.Api.DTO.Common;
using Bookstore.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Api.Tests.IntegrationTests.Endpoints;

public sealed class BooksControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoBooksExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>();

        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetAll_ReturnsAllBooks()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        await SeedBookAsync(factory, authorId, "1984");
        await SeedBookAsync(factory, authorId, "Animal Farm");

        var response = await client.GetAsync("/api/v1/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>();

        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.Contains(books, b => b.Title == "1984");
        Assert.Contains(books, b => b.Title == "Animal Farm");
    }

    [Fact]
    public async Task GetById_ReturnsBook_WhenBookExists()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        var bookId = await SeedBookAsync(factory, authorId, "1984");

        var response = await client.GetAsync($"/api/v1/books/{bookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();

        Assert.NotNull(book);
        Assert.Equal(bookId, book.BookId);
        Assert.Equal(authorId, book.AuthorId);
        Assert.Equal("George Orwell", book.AuthorName);
        Assert.Equal("1984", book.Title);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/books/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreatedBook()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        var request = new BookRequest
        {
            AuthorId = authorId,
            Title = "1984",
            Subtitle = "A Novel"
        };

        var response = await client.PostAsJsonAsync("/api/v1/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();

        Assert.NotNull(book);
        Assert.True(book.BookId > 0);
        Assert.Equal(authorId, book.AuthorId);
        Assert.Equal("George Orwell", book.AuthorName);
        Assert.Equal("1984", book.Title);
        Assert.Equal("A Novel", book.Subtitle);

        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/v1/Books/{book.BookId}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenAuthorDoesNotExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var request = new BookRequest
        {
            AuthorId = 999,
            Title = "1984"
        };

        var response = await client.PostAsJsonAsync("/api/v1/books", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        Assert.Equal(0, await dbContext.Books.CountAsync());
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var request = new BookRequest
        {
            AuthorId = 1,
            Title = "AB"
        };

        var response = await client.PostAsJsonAsync("/api/v1/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedBook_WhenBookExists()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        var bookId = await SeedBookAsync(factory, authorId, "1984");

        var request = new BookRequest
        {
            AuthorId = authorId,
            Title = "Nineteen Eighty-Four",
            Subtitle = "Updated subtitle"
        };

        var response = await client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();

        Assert.NotNull(book);
        Assert.Equal(bookId, book.BookId);
        Assert.Equal("Nineteen Eighty-Four", book.Title);
        Assert.Equal("Updated subtitle", book.Subtitle);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");

        var request = new BookRequest
        {
            AuthorId = authorId,
            Title = "1984"
        };

        var response = await client.PutAsJsonAsync("/api/v1/books/999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenAuthorDoesNotExist_AndPreservesBook()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        var bookId = await SeedBookAsync(factory, authorId, "1984");

        var request = new BookRequest
        {
            AuthorId = 999,
            Title = "Updated title"
        };

        var response = await client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        var book = await dbContext.Books.SingleAsync(b => b.BookId == bookId);

        Assert.Equal("1984", book.Title);
        Assert.Equal(authorId, book.AuthorId);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_AndRemovesBook()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        var bookId = await SeedBookAsync(factory, authorId, "1984");

        var response = await client.DeleteAsync($"/api/v1/books/{bookId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        Assert.False(await dbContext.Books.AnyAsync(b => b.BookId == bookId));
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.DeleteAsync("/api/v1/books/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsBooksMatchingTitle()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");
        await SeedBookAsync(factory, authorId, "1984");
        await SeedBookAsync(factory, authorId, "Animal Farm");

        var response = await client.GetAsync("/api/v1/books/search?title=1984");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<BookResponse>>();

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("1984", result.Items[0].Title);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Search_ReturnsBooksMatchingAuthor()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var orwellId = await SeedAuthorAsync(factory, "George Orwell");
        var austenId = await SeedAuthorAsync(factory, "Jane Austen");

        await SeedBookAsync(factory, orwellId, "1984");
        await SeedBookAsync(factory, orwellId, "Animal Farm");
        await SeedBookAsync(factory, austenId, "Pride and Prejudice");

        var response = await client.GetAsync("/api/v1/books/search?author=Orwell");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<BookResponse>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, book => Assert.Equal("George Orwell", book.AuthorName));
    }

    [Fact]
    public async Task Search_ReturnsPaginatedResults()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var authorId = await SeedAuthorAsync(factory, "George Orwell");

        await SeedBookAsync(factory, authorId, "Book A");
        await SeedBookAsync(factory, authorId, "Book B");
        await SeedBookAsync(factory, authorId, "Book C");

        var response = await client.GetAsync("/api/v1/books/search?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<BookResponse>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("page=-1")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task Search_ReturnsBadRequest_WhenPaginationIsInvalid(string query)
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1/books/search?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Requests_ReturnUnauthorized_WhenNotAuthenticated()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenWriteScopeIsMissing()
    {
        await using var factory = new BookstoreApiFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-Test-Scopes", "bookstore.read");

        var request = new BookRequest
        {
            AuthorId = 1,
            Title = "1984"
        };

        var response = await client.PostAsJsonAsync("/api/v1/books", request);

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

    private static async Task<int> SeedBookAsync(
        BookstoreApiFactory factory, int authorId, string title)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        var book = new Book
        {
            AuthorId = authorId,
            Title = title
        };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        return book.BookId;
    }
}