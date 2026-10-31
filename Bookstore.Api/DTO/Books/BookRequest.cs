using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.DTO.Books
{
    public class BookRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be greater than 0.")]
        public int AuthorId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Book title should be between 3 and 100 characters.")]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }
    }
}
