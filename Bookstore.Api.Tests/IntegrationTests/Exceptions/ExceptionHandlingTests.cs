using System.Net;
using System.Text.Json;
using Bookstore.Api.ExceptionHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Api.Tests.IntegrationTests.Exceptions;

public sealed class ExceptionHandlingTests
{
    [Theory]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    [InlineData(true, HttpStatusCode.Conflict)]
    public async Task Exceptions_ReturnSafeProblemDetails(bool concurrency, HttpStatusCode expectedStatus)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        await using var app = builder.Build();
        app.UseExceptionHandler();
        app.Run(_ => Task.FromException(concurrency
            ? new DbUpdateConcurrencyException("Private database details")
            : new InvalidOperationException("Private connection details")));
        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/failure");

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Private", json);
        Assert.DoesNotContain("stackTrace", json, StringComparison.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(json);
        Assert.Equal((int)expectedStatus, document.RootElement.GetProperty("status").GetInt32());
        Assert.True(document.RootElement.TryGetProperty("traceId", out _));
    }
}
