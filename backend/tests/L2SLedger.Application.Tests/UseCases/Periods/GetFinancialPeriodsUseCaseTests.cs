using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Periods;

public class GetFinancialPeriodsUseCaseTests
{
    private readonly Mock<IFinancialPeriodRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetFinancialPeriodsUseCase _useCase;

    public GetFinancialPeriodsUseCaseTests()
    {
        _mockRepository = new Mock<IFinancialPeriodRepository>();
        _mockMapper = new Mock<IMapper>();

        _useCase = new GetFinancialPeriodsUseCase(
            _mockRepository.Object,
            _mockMapper.Object);
    }

    private static FinancialPeriodDto CreateDto(FinancialPeriod p, string status = "Open") => new()
    {
        Id = p.Id,
        Year = p.Year,
        Month = p.Month,
        PeriodName = p.GetPeriodName(),
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = status,
        TotalIncome = 0,
        TotalExpense = 0,
        NetBalance = 0,
        CreatedAt = p.CreatedAt
    };

    [Fact]
    public async Task ExecuteAsync_NoFilters_ReturnsAllPeriods()
    {
        // Arrange
        var request = new GetPeriodsRequest();
        var periods = new List<FinancialPeriod>
        {
            new FinancialPeriod(2026, 1),
            new FinancialPeriod(2026, 2),
            new FinancialPeriod(2025, 12)
        };

        var periodDtos = periods.Select(p => CreateDto(p)).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(null, null, null, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((periods, 3));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<FinancialPeriodDto>>(periods))
            .Returns(periodDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Periods.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByYear_ReturnsFilteredPeriods()
    {
        // Arrange
        var request = new GetPeriodsRequest(Year: 2026);
        var periods = new List<FinancialPeriod>
        {
            new FinancialPeriod(2026, 1),
            new FinancialPeriod(2026, 2)
        };

        var periodDtos = periods.Select(p => CreateDto(p)).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(2026, null, null, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((periods, 2));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<FinancialPeriodDto>>(periods))
            .Returns(periodDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Periods.Should().HaveCount(2);
        result.Periods.Should().AllSatisfy(p => p.Year.Should().Be(2026));
    }

    [Fact]
    public async Task ExecuteAsync_FilterByMonth_ReturnsFilteredPeriods()
    {
        // Arrange
        var request = new GetPeriodsRequest(Month: 1);
        var periods = new List<FinancialPeriod>
        {
            new FinancialPeriod(2026, 1),
            new FinancialPeriod(2025, 1)
        };

        var periodDtos = periods.Select(p => CreateDto(p)).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(null, 1, null, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((periods, 2));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<FinancialPeriodDto>>(periods))
            .Returns(periodDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Periods.Should().HaveCount(2);
        result.Periods.Should().AllSatisfy(p => p.Month.Should().Be(1));
    }

    [Fact]
    public async Task ExecuteAsync_FilterByStatus_ReturnsFilteredPeriods()
    {
        // Arrange
        var request = new GetPeriodsRequest(Status: "Closed");
        var periods = new List<FinancialPeriod>
        {
            new FinancialPeriod(2025, 12)
        };

        var periodDtos = periods.Select(p => CreateDto(p, "Closed")).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(null, null, PeriodStatus.Closed, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((periods, 1));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<FinancialPeriodDto>>(periods))
            .Returns(periodDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Periods.Should().HaveCount(1);
        result.Periods.Should().AllSatisfy(p => p.Status.Should().Be("Closed"));
    }

    [Fact]
    public async Task ExecuteAsync_Pagination_WorksCorrectly()
    {
        // Arrange
        var request = new GetPeriodsRequest(Page: 2, PageSize: 5);
        var periods = new List<FinancialPeriod>
        {
            new FinancialPeriod(2025, 7),
            new FinancialPeriod(2025, 6)
        };

        var periodDtos = periods.Select(p => CreateDto(p)).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(null, null, null, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((periods, 12));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<FinancialPeriodDto>>(periods))
            .Returns(periodDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
    }

    [Fact]
    public async Task ExecuteAsync_OrderedByYearDescMonthDesc()
    {
        // Arrange
        var request = new GetPeriodsRequest();
        
        // Repository returns periods in correct order (Year DESC, Month DESC)
        var periods = new List<FinancialPeriod>
        {
            new FinancialPeriod(2026, 2),
            new FinancialPeriod(2026, 1),
            new FinancialPeriod(2025, 12)
        };

        var periodDtos = periods.Select(p => CreateDto(p)).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(null, null, null, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((periods, 3));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<FinancialPeriodDto>>(periods))
            .Returns(periodDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        var periodsList = result.Periods.ToList();
        periodsList[0].Year.Should().Be(2026);
        periodsList[0].Month.Should().Be(2);
        periodsList[1].Year.Should().Be(2026);
        periodsList[1].Month.Should().Be(1);
        periodsList[2].Year.Should().Be(2025);
        periodsList[2].Month.Should().Be(12);
    }
}
