using AutoMapper;
using FluentAssertions;
using L2SLedger.API.Controllers;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Users;
using L2SLedger.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.API.Tests.Controllers;

/// <summary>
/// Testes do UsersController focados nos cenários críticos de status.
/// Nota: Testes abrangentes são realizados nas camadas Domain e Application.
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _auditServiceMock = new Mock<IAuditService>();
        _mapperMock = new Mock<IMapper>();

        var getUsersUseCase = new GetUsersUseCase(
            _userRepositoryMock.Object,
            _mapperMock.Object);

        var getUserByIdUseCase = new GetUserByIdUseCase(
            _userRepositoryMock.Object,
            _mapperMock.Object);

        var getUserRolesUseCase = new GetUserRolesUseCase(
            _userRepositoryMock.Object);

        var updateUserRolesUseCase = new UpdateUserRolesUseCase(
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object,
            Mock.Of<ILogger<UpdateUserRolesUseCase>>());

        var updateUserStatusUseCase = new UpdateUserStatusUseCase(
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _auditServiceMock.Object,
            _mapperMock.Object,
            Mock.Of<ILogger<UpdateUserStatusUseCase>>());

        _controller = new UsersController(
            getUsersUseCase,
            getUserByIdUseCase,
            getUserRolesUseCase,
            updateUserRolesUseCase,
            updateUserStatusUseCase,
            Mock.Of<ILogger<UsersController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task UpdateUserStatus_WithValidApproval_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        
        var expectedDto = new UserDetailDto
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            EmailVerified = true,
            Status = "Active",
            Roles = new List<string> { "Leitura" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(x => x.Map<UserDetailDto>(It.IsAny<User>()))
            .Returns(expectedDto);

        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = "Cadastro aprovado"
        };

        // Act
        var result = await _controller.UpdateUserStatus(userId, request);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<UserDetailDto>();
    }

    [Fact]
    public async Task UpdateUserStatus_WithUserNotFound_ShouldReturn404()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = "Test reason"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.UpdateUserStatus(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}

