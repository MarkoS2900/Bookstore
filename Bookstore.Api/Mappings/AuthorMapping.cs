using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Models;

namespace Bookstore.Api.Mappings;

public static class AuthorMapping
{
    public static AuthorResponse ToResponse(this Author author)
    {
        return new AuthorResponse
        {
            AuthorId = author.AuthorId,
            Name = author.Name
        };
    }

    public static Author ToEntity(this CreateAuthorRequest request)
    {
        return new Author
        {
            Name = request.Name
        };
    }
}
