using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

/// <summary>Single-approver limit rules (Manager 50,000, Director 500,000), written from the spec scenarios.</summary>
public class ApprovalLimitsTests
{
    private const string InvoiceId = "INV-1";

    private static (ApprovalService Service, FakeInvoiceRepository Repository) PendingInvoiceOf(decimal amount)
    {
        var invoice = new Invoice
        {
            Id = InvoiceId,
            CustomerName = "Test School",
            Amount = amount,
            Status = InvoiceStatus.PendingApproval
        };
        var repository = new FakeInvoiceRepository(invoice);
        return (new ApprovalService(repository), repository);
    }

    private static void AssertInvoiceUnchanged(FakeInvoiceRepository repository)
    {
        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.PendingApproval, stored.Status);
        Assert.Null(stored.DecidedBy);
        Assert.Null(stored.DecidedAt);
        Assert.Null(stored.DecisionNote);
        Assert.Empty(stored.Approvals);
    }

    private static void AssertApprovedBy(FakeInvoiceRepository repository, string approver)
    {
        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.Approved, stored.Status);
        Assert.Equal(approver, stored.DecidedBy);
        Assert.NotNull(stored.DecidedAt);
    }

    [Fact]
    public void Approve_ManagerAtExactLimit_ApprovesInvoice()
    {
        var (service, repository) = PendingInvoiceOf(50000m);

        var result = service.Approve(InvoiceId, "demo.manager", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        AssertApprovedBy(repository, "demo.manager");
    }

    [Fact]
    public void Approve_ManagerJustBelowLimit_ApprovesInvoice()
    {
        var (service, repository) = PendingInvoiceOf(49999.99m);

        var result = service.Approve(InvoiceId, "demo.manager", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        AssertApprovedBy(repository, "demo.manager");
    }

    [Fact]
    public void Approve_ManagerJustOverLimit_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(50000.01m);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.manager", "Manager"));

        Assert.Contains("Manager", exception.Message);
        Assert.Contains("50,000", exception.Message);
        AssertInvoiceUnchanged(repository);
    }

    [Fact]
    public void Approve_ManagerAtDirectorLimit_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(500000m);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.manager", "Manager"));

        Assert.Contains("Manager limit of 50,000", exception.Message);
        AssertInvoiceUnchanged(repository);
    }

    [Fact]
    public void Approve_DirectorJustOverManagerLimit_ApprovesInvoice()
    {
        var (service, repository) = PendingInvoiceOf(50000.01m);

        service.Approve(InvoiceId, "demo.director", "Director");

        AssertApprovedBy(repository, "demo.director");
    }

    [Fact]
    public void Approve_DirectorAtExactLimit_ApprovesInvoice()
    {
        var (service, repository) = PendingInvoiceOf(500000m);

        service.Approve(InvoiceId, "demo.director", "Director");

        AssertApprovedBy(repository, "demo.director");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Approver")]
    [InlineData("")]
    public void Approve_RoleThatCannotApprove_IsRefusedAndInvoiceUnchanged(string role)
    {
        var (service, repository) = PendingInvoiceOf(100m);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.user", role));

        Assert.Contains("cannot approve", exception.Message);
        AssertInvoiceUnchanged(repository);
    }

    public static TheoryData<decimal, bool> ManagerBoundaryCases => new()
    {
        { 49999.99m, true },
        { 50000m, true },
        { 50000.01m, false },
        { 100000m, false }
    };

    public static TheoryData<decimal> DirectorBoundaryCases => new()
    {
        50000.01m,
        499999.99m,
        500000m
    };

    [Theory]
    [MemberData(nameof(ManagerBoundaryCases))]
    public void Approve_ManagerBoundary_AllowedOnlyUpToLimit(decimal amount, bool expectedAllowed)
    {
        var (service, repository) = PendingInvoiceOf(amount);

        if (expectedAllowed)
        {
            service.Approve(InvoiceId, "demo.manager", "Manager");
            AssertApprovedBy(repository, "demo.manager");
        }
        else
        {
            Assert.Throws<ApprovalNotAllowedException>(() => service.Approve(InvoiceId, "demo.manager", "Manager"));
            AssertInvoiceUnchanged(repository);
        }
    }

    [Theory]
    [MemberData(nameof(DirectorBoundaryCases))]
    public void Approve_DirectorBoundary_ApprovesUpToLimit(decimal amount)
    {
        var (service, repository) = PendingInvoiceOf(amount);

        service.Approve(InvoiceId, "demo.director", "Director");

        AssertApprovedBy(repository, "demo.director");
    }
}
