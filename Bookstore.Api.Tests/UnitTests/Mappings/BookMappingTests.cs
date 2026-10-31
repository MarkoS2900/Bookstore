using Bookstore.Api.DTO.Books;
using Bookstore.Api.Mappings;
using Bookstore.Api.Models;

namespace Bookstore.Api.Tests.UnitTests.Mappings;

public class BookMappingTests
{
    [Fact]
    public void ToResponse_ShouldMapBookToBookResponse()
    {
        var book = new Book
        {
            BookId = 1,
            AuthorId = 10,
            Author = new Author
            {
                AuthorId = 10,
                Name = "George Orwell"
            },
            Title = "1984",
            Subtitle = "A Novel"
        };

        var result = book.ToResponse();

        Assert.Equal(book.BookId, result.BookId);
        Assert.Equal(book.AuthorId, result.AuthorId);
        Assert.Equal(book.Author.Name, result.AuthorName);
        Assert.Equal(book.Title, result.Title);
        Assert.Equal(book.Subtitle, result.Subtitle);
    }

    [Fact]
    public void ToResponse_ShouldHandleNullSubtitle()
    {
        var book = new Book
        {
            BookId = 1,
            AuthorId = 10,
            Author = new Author
            {
                AuthorId = 10,
                Name = "George Orwell"
            },
            Title = "1984",
            Subtitle = null
        };

        var result = book.ToResponse();

        Assert.Null(result.Subtitle);
    }

    [Fact]
    public void ToEntity_ShouldMapBookRequestToBook()
    {
        var request = new BookRequest
        {
            AuthorId = 10,
            Title = "1984",
            Subtitle = "A Novel"
        };

        var result = request.ToEntity();

        Assert.Equal(request.AuthorId, result.AuthorId);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Subtitle, result.Subtitle);
        Assert.Equal(0, result.BookId);
        Assert.Null(result.Author);
    }

    [Fact]
    public void ToEntity_ShouldHandleNullSubtitle()
    {
        var request = new BookRequest
        {
            AuthorId = 10,
            Title = "1984",
            Subtitle = null
        };

        var result = request.ToEntity();

        Assert.Null(result.Subtitle);
    }
}