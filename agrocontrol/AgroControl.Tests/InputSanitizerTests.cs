using Xunit;
using AgroControl.Api.Security;

namespace AgroControl.Tests;

public class InputSanitizerTests
{
    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("javascript:alert(1)")]
    [InlineData("SELECT * FROM Users;--")]
    [InlineData("some value /* comment */")]
    [InlineData("onload=alert(1)")]
    [InlineData("onerror=alert(1)")]
    public void ContemConteudoSuspeito_ShouldReturnTrue_ForSuspiciousContent(string input)
    {
        // Act
        var result = InputSanitizer.ContemConteudoSuspeito(input);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("Gustavo Operador")]
    [InlineData("gustavo@agro.com")]
    [InlineData("Normal Text 123")]
    [InlineData("")]
    [InlineData(null)]
    public void ContemConteudoSuspeito_ShouldReturnFalse_ForSafeContent(string? input)
    {
        // Act
        var result = InputSanitizer.ContemConteudoSuspeito(input);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("ESTUFA-MARTE-01")]
    [InlineData("ESTUFA_MARTE_01")]
    [InlineData("estufa123")]
    [InlineData("A")]
    public void UuidValido_ShouldReturnTrue_ForValidUuids(string input)
    {
        // Act
        var result = InputSanitizer.UuidValido(input);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("ESTUFA MARTE 01")] // Spaces not allowed
    [InlineData("ESTUFA@MARTE")]    // Special chars not allowed
    [InlineData("<script>")]        // HTML tags not allowed
    [InlineData("")]                // Empty not allowed
    [InlineData(null)]              // Null not allowed
    [InlineData("VeryLongUuidThatExceedsTheSixtyCharacterLimitSupportedByOurRegexPattern12345")] // > 60 chars
    public void UuidValido_ShouldReturnFalse_ForInvalidUuids(string? input)
    {
        // Act
        var result = InputSanitizer.UuidValido(input);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(24.5)]
    [InlineData(0.0)]
    [InlineData(-10.5)]
    [InlineData(100.0)]
    public void NumeroValido_ShouldReturnTrue_ForFiniteNumbers(double input)
    {
        // Act
        var result = InputSanitizer.NumeroValido(input);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NumeroValido_ShouldReturnFalse_ForNull()
    {
        // Act
        var result = InputSanitizer.NumeroValido(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NumeroValido_ShouldReturnFalse_ForNaN()
    {
        // Act
        var result = InputSanitizer.NumeroValido(double.NaN);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NumeroValido_ShouldReturnFalse_ForInfinity()
    {
        // Act
        var result1 = InputSanitizer.NumeroValido(double.PositiveInfinity);
        var result2 = InputSanitizer.NumeroValido(double.NegativeInfinity);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }
}
