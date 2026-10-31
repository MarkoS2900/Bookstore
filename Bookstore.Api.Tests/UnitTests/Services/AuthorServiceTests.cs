using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Models;
using Bookstore.Api.Repositories.Interfaces;
using Bookstore.Api.Services;
using Moq;

namespace Bookstore.Api.Tests.UnitTests.Services;

public class AuthorServiceTests
{
    private readonly Mock<IAuthorRepository> _authorRepositoryMock;
    private readonly AuthorService _authorService;

    public AuthorServiceTests()
    {
        _authorRepositoryMock = new Mock<IAuthorRepository>();

        _authorService = new AuthorService(
            _authorRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAuthors()
    {
        var authors = new List<Author>
        {
            new() { AuthorId = 1, Name = "George Orwell" },
            new() { AuthorId = 2, Name = "J. R. R. Tolkien" }
        };

        _authorRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(authors);

        var result = await _authorService.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("George Orwell", result[0].Name);
        Assert.Equal("J. R. R. Tolkien", result[1].Name);

        _authorRepositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoAuthorsExist_ShouldReturnEmptyList()
    {
        _authorRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Author>());

        var result = await _authorService.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorExists_ShouldReturnAuthor()
    {
        var author = new Author
        {
            AuthorId = 1,
            Name = "George Orwell"
        };

        _authorRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        var result = await _authorService.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.AuthorId);
        Assert.Equal("George Orwell", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        _authorRepositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var result = await _authorService.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnAuthor()
    {
        var request = new CreateAuthorRequest
        {
            Name = "George Orwell"
        };

        _authorRepositoryMock
            .Setup(r => r.CreateAsync(
                It.IsAny<Author>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author author, CancellationToken _) =>
            {
                author.AuthorId = 1;
                return author;
            });

        var result = await _authorService.CreateAsync(request);

        Assert.Equal(1, result.AuthorId);
        Assert.Equal("George Orwell", result.Name);

        _authorRepositoryMock.Verify(
            r => r.CreateAsync(
                It.Is<Author>(a => a.Name == request.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
