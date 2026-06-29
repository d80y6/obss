using Xunit;
using FluentAssertions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.SharedKernel.Tests.Application.Contracts;

public class ResultTests
{
    [Fact]
    public void SuccessResult_ShouldHaveIsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void FailureResult_ShouldHaveIsFailureTrue()
    {
        var result = Result.Failure(Error.NotFound("User", Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FailureResult_ShouldHaveError()
    {
        var error = Error.NotFound("User", Guid.NewGuid());
        var result = Result.Failure(error);

        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessResultWithValue_ShouldReturnValue()
    {
        var result = Result.Success("test value");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
    }

    [Fact]
    public void FailureResultWithValue_ShouldThrowOnAccess()
    {
        var result = Result.Failure<string>(Error.NotFound("User", Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();

        FluentActions.Invoking(() => result.Value)
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access value of a failed result*");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        Result<string> result = "implicit value";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("implicit value");
    }

    [Fact]
    public void Result_WithNullValue_ShouldNotThrow()
    {
        FluentActions.Invoking(() => Result.Failure<string>(Error.NullValue))
            .Should().NotThrow<InvalidOperationException>();
    }

    [Fact]
    public void ErrorNotFound_ShouldCreateProperly()
    {
        var id = Guid.NewGuid();
        var error = Error.NotFound("User", id);

        error.Code.Should().Be("Error.NotFound");
        error.Description.Should().Be($"'User' with id '{id}' was not found.");
    }

    [Fact]
    public void ErrorConflict_ShouldCreateProperly()
    {
        var error = Error.Conflict("Email already exists");

        error.Code.Should().Be("Error.Conflict");
        error.Description.Should().Be("Email already exists");
    }

    [Fact]
    public void SuccessResult_ShouldThrow_WhenCreatedWithError()
    {
        FluentActions.Invoking(() => new TestResult(true, Error.NotFound("Test", "1")))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FailureResult_ShouldThrow_WhenCreatedWithoutError()
    {
        FluentActions.Invoking(() => new TestResult(false, Error.None))
            .Should().Throw<InvalidOperationException>();
    }

    private sealed class TestResult : Result
    {
        public TestResult(bool isSuccess, Error error) : base(isSuccess, error) { }
    }
}
