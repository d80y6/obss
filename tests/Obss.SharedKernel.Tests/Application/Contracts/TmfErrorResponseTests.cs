using FluentAssertions;
using Obss.SharedKernel.Application.Contracts;
using Xunit;

namespace Obss.SharedKernel.Tests.Application.Contracts;

public class TmfErrorResponseTests
{
    [Fact]
    public void Create_Should_SetAllProperties()
    {
        var error = TmfErrorResponse.Create(400, "VALIDATION_ERROR", "Invalid input", "The 'name' field is required", "/customer/123");

        error.Status.Should().Be(400);
        error.Code.Should().Be("VALIDATION_ERROR");
        error.Reason.Should().Be("Invalid input");
        error.Message.Should().Be("The 'name' field is required");
        error.ReferenceError.Should().Be("/customer/123");
        error.SchemaLocation.Should().Be("https://tmf-open-api.net/schemas/error/400.json");
    }

    [Fact]
    public void Create_Should_SetDefaultType()
    {
        var error = TmfErrorResponse.Create(500, "ERROR", "Error", "Something went wrong");

        error.Type.Should().Be("https://tmf-open-api.net/error");
    }

    [Fact]
    public void FromValidationException_Should_MapCorrectly()
    {
        var errors = new List<FluentValidation.Results.ValidationFailure>
        {
            new("name", "Name is required") { ErrorCode = "NotEmptyValidator" }
        };
        var exception = new FluentValidation.ValidationException(errors);

        var error = TmfErrorResponse.FromValidationException(exception);

        error.Status.Should().Be(400);
        error.Code.Should().Be("VALIDATION_ERROR");
        error.Reason.Should().Be("Validation failed");
        error.Message.Should().Be("name: Name is required");
        error.ReferenceError.Should().BeNull();
    }

    [Fact]
    public void FromValidationException_WithMultipleErrors_Should_JoinMessages()
    {
        var errors = new List<FluentValidation.Results.ValidationFailure>
        {
            new("name", "Name is required") { ErrorCode = "NotEmptyValidator" },
            new("email", "Email is invalid") { ErrorCode = "EmailValidator" }
        };
        var exception = new FluentValidation.ValidationException(errors);

        var error = TmfErrorResponse.FromValidationException(exception);

        error.Message.Should().Contain("name: Name is required");
        error.Message.Should().Contain("email: Email is invalid");
    }

    [Fact]
    public void FromNotFoundException_Should_MapCorrectly()
    {
        var error = TmfErrorResponse.FromNotFoundException("Customer not found");

        error.Status.Should().Be(404);
        error.Code.Should().Be("NOT_FOUND");
        error.Reason.Should().Be("Resource not found");
        error.Message.Should().Be("Customer not found");
    }
}
