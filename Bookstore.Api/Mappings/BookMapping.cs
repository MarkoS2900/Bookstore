using Bookstore.Api.DTO.Books;
using Bookstore.Api.Models;

namespace Bookstore.Api.Mappings;

public static class BookMapping
{
    public static BookResponse ToResponse(this Book book)
    {
        return new BookResponse
        {
            BookId = book.BookId,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            Title = book.Title,
            Subtitle = book.Subtitle
        };
    }

    public static Book ToEntity(this BookRequest request)
    {
        return new Book
        {
            AuthorId = request.AuthorId,
            Title = request.Title,
            Subtitle = request.Subtitle
        };
    }
}
