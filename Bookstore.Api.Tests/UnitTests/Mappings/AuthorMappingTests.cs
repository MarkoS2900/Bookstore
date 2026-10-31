using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Mappings;
using Bookstore.Api.Models;

namespace Bookstore.Api.Tests.UnitTests.Mappings;

public class AuthorMappingTests
{
    [Fact]
    public void ToResponse_ShouldMapAuthorToAuthorResponse()
    {
        var author = new Author
        {
            AuthorId = 1,
            Name = "George Orwell"
        };

        var result = author.ToResponse();

        Assert.Equal(author.AuthorId, result.AuthorId);
        Assert.Equal(author.Name, result.Name);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateAuthorRequestToAuthor()
    {
        var request = new CreateAuthorRequest
        {
            Name = "George Orwell"
        };

        var result = request.ToEntity();

        Assert.Equal(request.Name, result.Name);
        Assert.Equal(0, result.AuthorId);
    }
}