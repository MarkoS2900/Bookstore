namespace Bookstore.Api.DTO.Books
{
    public class BookSearchRequest
    {
        public string? Title { get; set; }

        public string? Author { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
