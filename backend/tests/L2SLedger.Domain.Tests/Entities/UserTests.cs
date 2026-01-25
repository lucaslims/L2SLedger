using FluentAssertions;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUserWithDefaultRole()
    {
        // Arrange & Act
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Assert
        user.FirebaseUid.Should().Be("firebase-uid");
        user.Email.Should().Be("test@example.com");
        user.DisplayName.Should().Be("Test User");
        user.EmailVerified.Should().BeTrue();
        user.Roles.Should().Contain("Leitura");
        user.Roles.Should().HaveCount(1);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddRole_ShouldAddNewRole()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.AddRole("Financeiro");

        // Assert
        user.Roles.Should().Contain("Financeiro");
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain(new[] { "Leitura", "Financeiro" });
    }

    [Fact]
    public void AddRole_WithDuplicateRole_ShouldNotAddDuplicate()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Financeiro");

        // Act
        user.AddRole("Financeiro");

        // Assert
        user.Roles.Should().HaveCount(2); // Leitura + Financeiro (sem duplicata)
        user.Roles.Count(r => r == "Financeiro").Should().Be(1);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveExistingRole()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Financeiro");
        user.AddRole("Admin");

        // Act
        user.RemoveRole("Financeiro");

        // Assert
        user.Roles.Should().NotContain("Financeiro");
        user.Roles.Should().Contain(new[] { "Leitura", "Admin" });
        user.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveRole_WithNonExistentRole_ShouldDoNothing()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.RemoveRole("NonExistent");

        // Assert
        user.Roles.Should().HaveCount(1);
        user.Roles.Should().Contain("Leitura");
    }

    [Fact]
    public void UpdateDisplayName_ShouldUpdateName()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Old Name", true);

        // Act
        user.UpdateDisplayName("New Name");

        // Assert
        user.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public void VerifyEmail_ShouldSetEmailVerifiedToTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", false);

        // Act
        user.VerifyEmail();

        // Assert
        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithExistingRole_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Admin");

        // Act & Assert
        user.HasRole("Admin").Should().BeTrue();
        user.HasRole("Leitura").Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithNonExistentRole_ShouldReturnFalse()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act & Assert
        user.HasRole("Admin").Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_WithAdminRole_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Admin");

        // Act & Assert
        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WithoutAdminRole_ShouldReturnFalse()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act & Assert
        user.IsAdmin().Should().BeFalse();
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.MarkAsDeleted();

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    // ============ Status Tests ============

    [Fact]
    public void Constructor_ShouldCreateUserWithStatusPending()
    {
        // Arrange & Act
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Assert
        user.Status.Should().Be(UserStatus.Pending);
    }

    [Fact]
    public void Approve_FromPending_ShouldChangeStatusToActive()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.Approve();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Approve_FromActive_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();

        // Act
        var act = () => user.Approve();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Active para Active.");
    }

    [Fact]
    public void Approve_FromSuspended_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();
        user.Suspend();

        // Act
        var act = () => user.Approve();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Suspended para Active.");
    }

    [Fact]
    public void Approve_FromRejected_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Reject();

        // Act
        var act = () => user.Approve();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Rejected para Active.");
    }

    [Fact]
    public void Suspend_FromActive_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();

        // Act
        user.Suspend();

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
    }

    [Fact]
    public void Suspend_FromPending_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        var act = () => user.Suspend();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Pending para Suspended.");
    }

    [Fact]
    public void Suspend_FromRejected_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Reject();

        // Act
        var act = () => user.Suspend();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Rejected para Suspended.");
    }

    [Fact]
    public void Suspend_FromSuspended_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();
        user.Suspend();

        // Act
        var act = () => user.Suspend();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Suspended para Suspended.");
    }

    [Fact]
    public void Reject_FromPending_ShouldChangeStatusToRejected()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.Reject();

        // Assert
        user.Status.Should().Be(UserStatus.Rejected);
    }

    [Fact]
    public void Reject_FromActive_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();

        // Act
        var act = () => user.Reject();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Active para Rejected.");
    }

    [Fact]
    public void Reject_FromSuspended_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();
        user.Suspend();

        // Act
        var act = () => user.Reject();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Suspended para Rejected.");
    }

    [Fact]
    public void Reactivate_FromSuspended_ShouldChangeStatusToActive()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();
        user.Suspend();

        // Act
        user.Reactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Reactivate_FromPending_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        var act = () => user.Reactivate();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Pending para Active.");
    }

    [Fact]
    public void Reactivate_FromRejected_ShouldThrowInvalidStatusTransitionException()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Reject();

        // Act
        var act = () => user.Reactivate();

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("Não é possível alterar o status de Rejected para Active.");
    }
}
