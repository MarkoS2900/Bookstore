using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.Models
{
    public class Book
    {
        public int BookId { get; set; }

        public int AuthorId { get; set; }

        public Author Author { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Book title should be between 3 and 100 characters.")]
        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;
    }
}
