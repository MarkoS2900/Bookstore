using Bookstore.Api.DTO.Books;
using Bookstore.Api.Exceptions;
using Bookstore.Api.Models;
using Bookstore.Api.Repositories.Interfaces;
using Bookstore.Api.Services;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.Tests.UnitTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IAuthorRepository> _authorRepositoryMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _authorRepositoryMock = new Mock<IAuthorRepository>();

        _bookService = new BookService(
            _bookRepositoryMock.Object,
            _authorRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        var books = new List<Book>
        {
            CreateBook(1, "1984", 1, "George Orwell"),
            CreateBook(2, "Animal Farm", 1, "George Orwell")
        };

        _bookRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var result = await _bookService.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("1984", result[0].Title);
        Assert.Equal("Animal Farm", result[1].Title);
        Assert.Equal("George Orwell", result[0].AuthorName);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBooksExist_ShouldReturnEmptyList()
    {
        _bookRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        var result = await _bookService.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ShouldReturnBook()
    {
        var book = CreateBook(1, "1984", 1, "George Orwell");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var result = await _bookService.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.BookId);
        Assert.Equal("1984", result.Title);
        Assert.Equal("George Orwell", result.AuthorName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var result = await _bookService.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidAuthor_ShouldCreateBook()
    {
        var request = new BookRequest
        {
            AuthorId = 1,
            Title = "1984",
            Subtitle = "A Novel"
        };

        var author = new Author
        {
            AuthorId = 1,
            Name = "George Orwell"
        };

        _authorRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _bookRepositoryMock
            .Setup(r => r.CreateAsync(
                It.IsAny<Book>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book book, CancellationToken _) =>
            {
                book.BookId = 1;
                book.Author = author;
                return book;
            });

        var result = await _bookService.CreateAsync(request);

        Assert.Equal(1, result.BookId);
        Assert.Equal(1, result.AuthorId);
        Assert.Equal("1984", result.Title);
        Assert.Equal("A Novel", result.Subtitle);
        Assert.Equal("George Orwell", result.AuthorName);

        _bookRepositoryMock.Verify(
            r => r.CreateAsync(
                It.Is<Book>(b =>
                    b.AuthorId == 1 &&
                    b.Title == "1984" &&
                    b.Subtitle == "A Novel"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAuthorId_ShouldThrowValidationException()
    {
        var request = new BookRequest
        {
            AuthorId = 0,
            Title = "1984"
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookService.CreateAsync(request));

        Assert.Equal("AuthorId must be greater than 0.", exception.Message);

        _authorRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _bookRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNonexistentAuthor_ShouldThrowAuthorNotFoundException()
    {
        var request = new BookRequest
        {
            AuthorId = 999,
            Title = "1984"
        };

        _authorRepositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var exception = await Assert.ThrowsAsync<AuthorNotFoundException>(
            () => _bookService.CreateAsync(request));

        Assert.Equal("Author with ID 999 was not found.", exception.Message);

        _bookRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        var request = new BookRequest
        {
            AuthorId = 1,
            Title = "Updated title"
        };

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var result = await _bookService.UpdateAsync(999, request);

        Assert.Null(result);

        _authorRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateBook()
    {
        var existingBook = CreateBook(1, "1984", 1, "George Orwell");

        var request = new BookRequest
        {
            AuthorId = 2,
            Title = "Animal Farm",
            Subtitle = "A Fairy Story"
        };

        var newAuthor = new Author
        {
            AuthorId = 2,
            Name = "Another Author"
        };

        _bookRepositoryMock
            .SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook)
            .ReturnsAsync(new Book
            {
                BookId = 1,
                AuthorId = 2,
                Author = newAuthor,
                Title = "Animal Farm",
                Subtitle = "A Fairy Story"
            });

        _authorRepositoryMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAuthor);

        var result = await _bookService.UpdateAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal("Animal Farm", result.Title);
        Assert.Equal("A Fairy Story", result.Subtitle);
        Assert.Equal(2, result.AuthorId);
        Assert.Equal("Another Author", result.AuthorName);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Book>(b =>
                    b.BookId == 1 &&
                    b.AuthorId == 2 &&
                    b.Title == "Animal Farm"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidAuthorId_ShouldThrowValidationException()
    {
        var existingBook = CreateBook(1, "1984", 1, "George Orwell");

        var request = new BookRequest
        {
            AuthorId = 0,
            Title = "Updated title"
        };

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookService.UpdateAsync(1, request));

        Assert.Equal("AuthorId must be greater than 0.", exception.Message);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentAuthor_ShouldThrowAuthorNotFoundException()
    {
        var existingBook = CreateBook(1, "1984", 1, "George Orwell");

        var request = new BookRequest
        {
            AuthorId = 999,
            Title = "Updated title"
        };

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _authorRepositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var exception = await Assert.ThrowsAsync<AuthorNotFoundException>(
            () => _bookService.UpdateAsync(1, request));

        Assert.Equal("Author with ID 999 was not found.", exception.Message);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookExists_ShouldReturnTrue()
    {
        _bookRepositoryMock
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _bookService.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookDoesNotExist_ShouldReturnFalse()
    {
        _bookRepositoryMock
            .Setup(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _bookService.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task SearchAsync_WithValidRequest_ShouldReturnPagedBooks()
    {
        var request = new BookSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        var books = new List<Book>
        {
            CreateBook(1, "1984", 1, "George Orwell"),
            CreateBook(2, "Animal Farm", 1, "George Orwell")
        };

        var repositoryResult = new DTO.Common.PagedResponse<Book>
        {
            Items = books,
            Page = 1,
            PageSize = 10,
            TotalCount = 2
        };

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResult);

        var result = await _bookService.SearchAsync(request);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("1984", result.Items[0].Title);
        Assert.Equal("George Orwell", result.Items[0].AuthorName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_WithInvalidPage_ShouldThrowValidationException(int page)
    {
        var request = new BookSearchRequest
        {
            Page = page,
            PageSize = 10
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookService.SearchAsync(request));

        Assert.Equal("Page must be greater than 0.", exception.Message);

        _bookRepositoryMock.Verify(
            r => r.SearchAsync(It.IsAny<BookSearchRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task SearchAsync_WithInvalidPageSize_ShouldThrowValidationException(int pageSize)
    {
        var request = new BookSearchRequest
        {
            Page = 1,
            PageSize = pageSize
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookService.SearchAsync(request));

        Assert.Equal("PageSize must be between 1 and 100.", exception.Message);

        _bookRepositoryMock.Verify(
            r => r.SearchAsync(It.IsAny<BookSearchRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task SearchAsync_WithBoundaryPageSize_ShouldCallRepository(int pageSize)
    {
        var request = new BookSearchRequest
        {
            Page = 1,
            PageSize = pageSize
        };

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DTO.Common.PagedResponse<Book>
            {
                Items = new List<Book>(),
                Page = 1,
                PageSize = pageSize,
                TotalCount = 0
            });

        await _bookService.SearchAsync(request);

        _bookRepositoryMock.Verify(
            r => r.SearchAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Book CreateBook(
        int bookId,
        string title,
        int authorId,
        string authorName)
    {
        return new Book
        {
            BookId = bookId,
            Title = title,
            AuthorId = authorId,
            Author = new Author
            {
                AuthorId = authorId,
                Name = authorName
            }
        };
    }
}