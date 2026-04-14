using FluentAssertions;
using L2SLedger.Application.Common.Logging;

namespace L2SLedger.Application.Tests.Common.Logging;

public class LogSanitizerTests
{
    [Fact]
    public void Sanitize_WithNullValue_ReturnsEmpty()
    {
        var result = LogSanitizer.Sanitize(null);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_RemovesCrLfTabsAndControlCharacters()
    {
        var input = "line1\r\nline2\tline3\u0001";

        var result = LogSanitizer.Sanitize(input);

        result.Should().Be("line1 line2 line3");
    }

    [Fact]
    public void Sanitize_WithMaskEmail_MasksEmailInText()
    {
        var input = "user john.doe@example.com autenticado";

        var result = LogSanitizer.Sanitize(input, maskEmail: true);

        result.Should().Be("user jo***@example.com autenticado");
    }

    [Fact]
    public void Sanitize_WithTextLongerThanMaxLength_TruncatesWithSuffix()
    {
        var input = new string('a', 300);

        var result = LogSanitizer.Sanitize(input, maxLength: 256);

        result.Length.Should().Be(256);
        result.Should().EndWith("...[truncated]");
    }

    [Fact]
    public void SanitizeExceptionMessage_AppliesNormalizationAndTruncation()
    {
        var input = "erro\ncom\rdetalhe";

        var result = LogSanitizer.SanitizeExceptionMessage(input, maxLength: 12);

        result.Should().Be("erro com det");
    }
}
