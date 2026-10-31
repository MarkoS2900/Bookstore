using Bookstore.Api.DTO.Authors;
using Bookstore.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthorsController(IAuthorService authorService, ILogger<AuthorsController> logger) : ControllerBase
{
    [Authorize(Policy = "BookstoreRead")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all authors.");

        var authors = await authorService.GetAllAsync(cancellationToken);

        logger.LogInformation(
            "Successfully retrieved {AuthorCount} authors.",
            authors.Count());

        return Ok(authors);
    }

    [Authorize(Policy = "BookstoreRead")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving author with ID {AuthorId}.", id);

        var author = await authorService.GetByIdAsync(id, cancellationToken);

        if (author is null)
        {
            logger.LogWarning("Author with ID {AuthorId} was not found.", id);
            return NotFound();
        }

        logger.LogInformation("Successfully retrieved author with ID {AuthorId}.", id);

        return Ok(author);
    }

    [Authorize(Policy = "BookstoreWrite")]
    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> Create(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating author.");

        var author = await authorService.CreateAsync(request, cancellationToken);

        logger.LogInformation("Successfully created author with ID {AuthorId}.", author.AuthorId);

        return CreatedAtAction(nameof(GetById), new { id = author.AuthorId }, author);
    }
}
