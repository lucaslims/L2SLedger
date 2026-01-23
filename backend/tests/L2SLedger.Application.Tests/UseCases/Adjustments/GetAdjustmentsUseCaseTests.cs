using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Adjustments;
using L2SLedger.Domain.Entities;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Adjustments;

public class GetAdjustmentsUseCaseTests
{
    private readonly Mock<IAdjustmentRepository> _adjustmentRepositoryMock;
    private readonly Mock<IValidator<GetAdjustmentsRequest>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;
    private readonly GetAdjustmentsUseCase _sut;

    private readonly Guid _userId = Guid.NewGuid();

    public GetAdjustmentsUseCaseTests()
    {
        _adjustmentRepositoryMock = new Mock<IAdjustmentRepository>();
        _validatorMock = new Mock<IValidator<GetAdjustmentsRequest>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdjustmentProfile>();
        });
        _mapper = config.CreateMapper();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(_userId);

        _sut = new GetAdjustmentsUseCase(
            _adjustmentRepositoryMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnAdjustments()
    {
        // Arrange
        var request = new GetAdjustmentsRequest
        {
            Page = 1,
            PageSize = 20
        };

        var adjustments = new List<Adjustment>
        {
            CreateAdjustment(100m, AdjustmentType.Correction),
            CreateAdjustment(200m, AdjustmentType.Reversal)
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _adjustmentRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                _userId,
                request.Page,
                request.PageSize,
                It.IsAny<Guid?>(),
                It.IsAny<AdjustmentType?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((adjustments, 2));

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Adjustments.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task ExecuteAsync_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        var request = new GetAdjustmentsRequest
        {
            OriginalTransactionId = transactionId,
            Type = 1, // Correction
            StartDate = startDate,
            EndDate = endDate,
            Page = 1,
            PageSize = 10
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _adjustmentRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                _userId,
                1,
                10,
                transactionId,
                AdjustmentType.Correction,
                startDate,
                endDate,
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Adjustment>(), 0));

        // Act
        await _sut.ExecuteAsync(request);

        // Assert
        _adjustmentRepositoryMock.Verify(x => x.GetByFiltersAsync(
            _userId,
            1,
            10,
            transactionId,
            AdjustmentType.Correction,
            startDate,
            endDate,
            null,
            false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var request = new GetAdjustmentsRequest
        {
            Page = 2,
            PageSize = 5
        };

        var adjustments = new List<Adjustment>
        {
            CreateAdjustment(100m, AdjustmentType.Correction)
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _adjustmentRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                _userId,
                2,
                5,
                It.IsAny<Guid?>(),
                It.IsAny<AdjustmentType?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((adjustments, 12));

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3); // 12 / 5 = 2.4, rounded up = 3
    }

    private Adjustment CreateAdjustment(decimal amount, AdjustmentType type)
    {
        return new Adjustment(
            Guid.NewGuid(),
            amount,
            type,
            "Justificativa válida para teste",
            DateTime.UtcNow.Date,
            _userId
        );
    }
}
