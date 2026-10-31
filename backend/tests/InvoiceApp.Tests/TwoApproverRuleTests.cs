using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

/// <summary>Two-approver rule for invoices above 500,000, written from the spec scenarios.</summary>
public class TwoApproverRuleTests
{
    private const string InvoiceId = "INV-1";
    private const string ManagerRole = "Manager";
    private const string DirectorRole = "Director";

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

    private static void AssertOnlyFirstApprovalRecorded(FakeInvoiceRepository repository, string firstApprover)
    {
        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.PendingApproval, stored.Status);
        Assert.Single(stored.Approvals);
        Assert.Equal(firstApprover, stored.Approvals[0].Username);
        Assert.Null(stored.DecidedBy);
        Assert.Null(stored.DecidedAt);
        Assert.Null(stored.DecisionNote);
    }

    [Fact]
    public void Approve_ManagerThenDirectorOnAboveDirectorLimit_ApprovesWithBothApprovers()
    {
        var (service, repository) = PendingInvoiceOf(600000m);

        service.Approve(InvoiceId, "demo.manager", ManagerRole);
        service.Approve(InvoiceId, "demo.director", DirectorRole);

        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.Approved, stored.Status);
        Assert.Equal(2, stored.Approvals.Count);
        Assert.Equal("demo.manager", stored.Approvals[0].Username);
        Assert.Equal("demo.director", stored.Approvals[1].Username);
        Assert.Equal("demo.director", stored.DecidedBy);
        Assert.NotNull(stored.DecidedAt);
    }

    [Fact]
    public void Approve_TwoDifferentDirectors_ApprovesInvoice()
    {
        var (service, repository) = PendingInvoiceOf(600000m);

        service.Approve(InvoiceId, "demo.director", DirectorRole);
        service.Approve(InvoiceId, "other.director", DirectorRole);

        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.Approved, stored.Status);
        Assert.Equal(2, stored.Approvals.Count);
        Assert.Equal("other.director", stored.DecidedBy);
    }

    [Fact]
    public void Approve_FirstApprovalAboveDirectorLimit_StaysPendingWithFirstApproverRecorded()
    {
        var (service, repository) = PendingInvoiceOf(600000m);

        var result = service.Approve(InvoiceId, "demo.manager", ManagerRole);

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
        AssertOnlyFirstApprovalRecorded(repository, "demo.manager");
    }

    [Fact]
    public void Approve_DirectorAloneJustOverLimit_StaysPendingWithDirectorRecordedAsFirstApprover()
    {
        var (service, repository) = PendingInvoiceOf(500000.01m);

        service.Approve(InvoiceId, "demo.director", DirectorRole);

        AssertOnlyFirstApprovalRecorded(repository, "demo.director");
        Assert.Equal(DirectorRole, repository.GetById(InvoiceId)!.Approvals[0].Role);
    }

    [Fact]
    public void Approve_ManagerFirstJustOverLimit_IsAcceptedAsFirstApprover()
    {
        var (service, repository) = PendingInvoiceOf(500000.01m);

        service.Approve(InvoiceId, "demo.manager", ManagerRole);

        AssertOnlyFirstApprovalRecorded(repository, "demo.manager");
    }

    [Fact]
    public void Approve_DirectorAtExactLimit_ApprovesOutrightWithOneApproval()
    {
        var (service, repository) = PendingInvoiceOf(500000m);

        service.Approve(InvoiceId, "demo.director", DirectorRole);

        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.Approved, stored.Status);
        Assert.Single(stored.Approvals);
        Assert.Equal("demo.director", stored.DecidedBy);
    }

    [Fact]
    public void Approve_SameUserTwice_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.director", DirectorRole);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.director", DirectorRole));

        Assert.Contains("same user", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("twice", exception.Message, StringComparison.OrdinalIgnoreCase);
        AssertOnlyFirstApprovalRecorded(repository, "demo.director");
    }

    [Fact]
    public void Approve_SameUserWithDifferentCasing_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.director", DirectorRole);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "Demo.Director", DirectorRole));

        Assert.Contains("same user", exception.Message, StringComparison.OrdinalIgnoreCase);
        AssertOnlyFirstApprovalRecorded(repository, "demo.director");
    }

    [Fact]
    public void Approve_SameManagerAsSecondApprover_RefusedWithSameUserReason()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.manager", ManagerRole);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.manager", ManagerRole));

        Assert.Contains("same user", exception.Message, StringComparison.OrdinalIgnoreCase);
        AssertOnlyFirstApprovalRecorded(repository, "demo.manager");
    }

    [Fact]
    public void Approve_DirectorThenDifferentManager_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.director", DirectorRole);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.manager", ManagerRole));

        Assert.Contains("second approver must be a Director", exception.Message, StringComparison.OrdinalIgnoreCase);
        AssertOnlyFirstApprovalRecorded(repository, "demo.director");
    }

    [Fact]
    public void Approve_ManagerThenDifferentManager_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.manager", ManagerRole);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "other.manager", ManagerRole));

        Assert.Contains("second approver must be a Director", exception.Message, StringComparison.OrdinalIgnoreCase);
        AssertOnlyFirstApprovalRecorded(repository, "demo.manager");
    }

    [Fact]
    public void Approve_AdminAsSecondApprover_IsRefusedAndInvoiceUnchanged()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.director", DirectorRole);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.admin", "Admin"));

        Assert.Contains("cannot approve", exception.Message);
        AssertOnlyFirstApprovalRecorded(repository, "demo.director");
    }

    [Fact]
    public void Approve_SameDirectorFromTwoSimultaneousRequests_RecordsOnlyOneApproval()
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var (service, repository) = PendingInvoiceOf(600000m);

            Parallel.For(0, 2, _ =>
            {
                try { service.Approve(InvoiceId, "demo.director", DirectorRole); }
                catch (ApprovalNotAllowedException) { }
            });

            var stored = repository.GetById(InvoiceId)!;
            Assert.Equal(InvoiceStatus.PendingApproval, stored.Status);
            Assert.Single(stored.Approvals);
        }
    }

    [Fact]
    public void Approve_AdminAsFirstApprover_IsRefusedAndLeavesNoApprovals()
    {
        var (service, repository) = PendingInvoiceOf(600000m);

        var exception = Assert.Throws<ApprovalNotAllowedException>(
            () => service.Approve(InvoiceId, "demo.admin", "Admin"));

        Assert.Contains("cannot approve", exception.Message);
        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.PendingApproval, stored.Status);
        Assert.Empty(stored.Approvals);
        Assert.Null(stored.DecidedBy);
        Assert.Null(stored.DecidedAt);
        Assert.Null(stored.DecisionNote);
    }

    [Fact]
    public void Reject_InvoiceWithOneApproval_RejectsAsBefore()
    {
        var (service, repository) = PendingInvoiceOf(600000m);
        service.Approve(InvoiceId, "demo.manager", ManagerRole);

        service.Reject(InvoiceId, "demo.director", "Amount not justified");

        var stored = repository.GetById(InvoiceId)!;
        Assert.Equal(InvoiceStatus.Rejected, stored.Status);
        Assert.Equal("demo.director", stored.DecidedBy);
        Assert.NotNull(stored.DecidedAt);
        Assert.Equal("Amount not justified", stored.DecisionNote);
    }
}
