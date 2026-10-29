namespace Bookstore.Api.Models
{
    public class Book
    {
        public int BookId { get; set; }

        public int AuthorId { get; set; }

        public Author Author { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }
    }
}
