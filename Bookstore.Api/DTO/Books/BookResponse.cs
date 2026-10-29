namespace Bookstore.Api.DTO.Books
{
    public class BookResponse
    {
        public int BookId { get; set; }

        public int AuthorId { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }
    }
}
