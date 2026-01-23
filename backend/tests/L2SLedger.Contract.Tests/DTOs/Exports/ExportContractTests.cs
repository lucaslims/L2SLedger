using System.Text.Json;
using FluentAssertions;
using L2SLedger.Application.DTOs.Exports;

namespace L2SLedger.Contract.Tests.DTOs.Exports;

/// <summary>
/// Testes de contrato para DTOs de exportação.
/// Valida estrutura, serialização e imutabilidade dos contratos públicos.
/// </summary>
public class ExportContractTests
{
    [Fact]
    public void ExportDto_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(ExportDto).GetProperties();

        // Assert
        properties.Should().HaveCount(15);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Id", "ExportType", "Format", "Status", "FilePath",
            "FileSizeBytes", "ParametersJson", "RequestedByUserId", "RequestedByUserName",
            "RequestedAt", "ProcessingStartedAt", "CompletedAt", "ErrorMessage",
            "RecordCount", "CreatedAt"
        });
    }

    [Fact]
    public void ExportDto_ShouldSerializeWithCamelCase()
    {
        // Arrange
        var dto = new ExportDto
        {
            Id = Guid.NewGuid(),
            ExportType = "Transactions",
            Format = "Csv",
            Status = "Completed",
            FilePath = "exports/test.csv",
            FileSizeBytes = 1024,
            ParametersJson = "{}",
            RequestedByUserId = Guid.NewGuid(),
            RequestedByUserName = "John Doe",
            RequestedAt = new DateTime(2026, 1, 15, 10, 30, 0),
            ProcessingStartedAt = new DateTime(2026, 1, 15, 10, 31, 0),
            CompletedAt = new DateTime(2026, 1, 15, 10, 32, 0),
            ErrorMessage = null,
            RecordCount = 100,
            CreatedAt = new DateTime(2026, 1, 15, 10, 30, 0)
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(dto, options);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"exportType\":");
        json.Should().Contain("\"format\":");
        json.Should().Contain("\"status\":");
        json.Should().Contain("\"filePath\":");
        json.Should().Contain("\"requestedByUserId\":");
        json.Should().Contain("\"requestedByUserName\":");
    }

    [Fact]
    public void RequestExportRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(RequestExportRequest).GetProperties();

        // Assert
        properties.Should().HaveCount(5);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Format", "StartDate", "EndDate", "CategoryId", "TransactionType"
        });
    }

    [Fact]
    public void RequestExportRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new RequestExportRequest
        {
            Format = 1, // CSV
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            CategoryId = Guid.NewGuid(),
            TransactionType = 2
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);
        var deserialized = JsonSerializer.Deserialize<RequestExportRequest>(json, options);

        // Assert
        json.Should().Contain("\"format\":");
        json.Should().Contain("\"startDate\":");
        json.Should().Contain("\"endDate\":");
        deserialized.Should().NotBeNull();
        deserialized!.Format.Should().Be(request.Format);
        deserialized.StartDate.Should().Be(request.StartDate);
        deserialized.EndDate.Should().Be(request.EndDate);
    }

    [Fact]
    public void ExportStatusResponse_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(ExportStatusResponse).GetProperties();

        // Assert
        properties.Should().HaveCount(7);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Id", "Status", "ErrorMessage", "ProgressPercentage",
            "RequestedAt", "CompletedAt", "IsDownloadable"
        });
    }

    [Fact]
    public void ExportStatusResponse_ShouldSerializeCorrectly()
    {
        // Arrange
        var response = new ExportStatusResponse
        {
            Id = Guid.NewGuid(),
            Status = "Processing",
            ErrorMessage = null,
            ProgressPercentage = 50,
            RequestedAt = new DateTime(2026, 1, 15, 10, 30, 0),
            CompletedAt = null,
            IsDownloadable = false
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(response, options);
        var deserialized = JsonSerializer.Deserialize<ExportStatusResponse>(json, options);

        // Assert
        json.Should().Contain("\"status\":");
        json.Should().Contain("\"progressPercentage\":");
        json.Should().Contain("\"isDownloadable\":");
        deserialized.Should().NotBeNull();
        deserialized!.Status.Should().Be(response.Status);
        deserialized.ProgressPercentage.Should().Be(50);
    }

    [Fact]
    public void GetExportsResponse_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(GetExportsResponse).GetProperties();

        // Assert
        properties.Should().HaveCount(4);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Exports", "TotalCount", "Page", "PageSize"
        });
    }

    [Fact]
    public void GetExportsResponse_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto1 = new ExportDto
        {
            Id = Guid.NewGuid(),
            ExportType = "Transactions",
            Format = "Csv",
            Status = "Pending",
            ParametersJson = "{}",
            RequestedByUserId = Guid.NewGuid(),
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var response = new GetExportsResponse
        {
            Exports = new List<ExportDto> { dto1 },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(response, options);
        var deserialized = JsonSerializer.Deserialize<GetExportsResponse>(json, options);

        // Assert
        json.Should().Contain("\"exports\":");
        json.Should().Contain("\"totalCount\":");
        json.Should().Contain("\"page\":");
        json.Should().Contain("\"pageSize\":");
        deserialized.Should().NotBeNull();
        deserialized!.Exports.Should().HaveCount(1);
        deserialized.TotalCount.Should().Be(1);
    }

    [Fact]
    public void GetExportsRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(GetExportsRequest).GetProperties();

        // Assert
        properties.Should().HaveCount(4);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Status", "Format", "Page", "PageSize"
        });
    }

    [Fact]
    public void GetExportsRequest_FormatAndStatus_ShouldSerializeAsIntegers()
    {
        // Arrange
        var request = new GetExportsRequest
        {
            Status = 1, // Pending
            Format = 2, // PDF
            Page = 1,
            PageSize = 10
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);
        var deserialized = JsonSerializer.Deserialize<GetExportsRequest>(json, options);

        // Assert
        json.Should().Contain("\"status\":1");
        json.Should().Contain("\"format\":2");
        deserialized.Should().NotBeNull();
        deserialized!.Status.Should().Be(1);
        deserialized.Format.Should().Be(2);
    }
}
