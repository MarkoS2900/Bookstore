namespace Bookstore.Api.Exceptions;

public sealed class AuthorNotFoundException(int authorId)
    : Exception($"Author with ID {authorId} was not found.");
