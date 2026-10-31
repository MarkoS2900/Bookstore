using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [Authorize(Policy = "BookstoreRead")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var author = await authorService.GetAllAsync(cancellationToken);
        return Ok(author);
    }

    [Authorize(Policy = "BookstoreRead")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await authorService.GetByIdAsync(id, cancellationToken);

        return author is null ? NotFound() : Ok(author);
    }

    [Authorize(Policy = "BookstoreWrite")]
    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> Create(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var author = await authorService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = author.AuthorId },
                author);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
