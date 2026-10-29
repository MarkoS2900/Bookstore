using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.DTO.Authors
{
    public class CreateAuthorRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Author name should be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}
