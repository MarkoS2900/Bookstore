using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.Models
{
    public class Author
    {
        public int AuthorId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Author name should be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}
