# Spec Delta

## Purpose
Defines who may approve an invoice, based on the approver's role and the invoice amount, so spending authority matches role and the largest invoices need two distinct approvers.

## ADDED Requirements

### Requirement: Manager approval limit
A Manager SHALL be able to approve a pending invoice whose amount is 50,000 or less. A Manager SHALL NOT be able to approve an invoice above 50,000 on their own.

#### Scenario: Manager approves at the limit
- **WHEN** a Manager approves a pending invoice of exactly 50,000
- **THEN** the invoice becomes Approved and records that Manager as the approver

#### Scenario: Manager approves below the limit
- **WHEN** a Manager approves a pending invoice of 49,999.99
- **THEN** the invoice becomes Approved

#### Scenario: Manager is refused just over the limit
- **WHEN** a Manager approves a pending invoice of 50,000.01
- **THEN** the attempt is refused with a reason stating the Manager limit of 50,000 is exceeded
- **AND** the invoice remains Pending Approval with no approver recorded

### Requirement: Director approval limit
A Director SHALL be able to approve a pending invoice whose amount is 500,000 or less. A Director SHALL NOT be able to approve an invoice above 500,000 on their own.

#### Scenario: Director approves just over the Manager limit
- **WHEN** a Director approves a pending invoice of 50,000.01
- **THEN** the invoice becomes Approved

#### Scenario: Director approves at the limit
- **WHEN** a Director approves a pending invoice of exactly 500,000
- **THEN** the invoice becomes Approved

#### Scenario: Director alone cannot approve above the limit
- **WHEN** a Director approves a pending invoice of 500,000.01 and no other approval exists
- **THEN** the invoice does not become Approved
- **AND** the invoice remains Pending Approval with the Director recorded as the first approver

### Requirement: Two approvers above 500,000
An invoice above 500,000 SHALL be Approved only after two distinct users have approved it, and the second approver MUST be a Director. The first approver MAY be a Manager or a Director. Until the second approval, the invoice SHALL remain Pending Approval.

#### Scenario: Manager then Director approves
- **WHEN** a Manager approves a pending invoice of 600,000 and then a different user who is a Director approves it
- **THEN** the invoice becomes Approved and records both approvers

#### Scenario: Two different Directors approve
- **WHEN** one Director approves a pending invoice of 600,000 and then a different Director approves it
- **THEN** the invoice becomes Approved

#### Scenario: Second approver is a Manager
- **WHEN** a Director approves a pending invoice of 600,000 and then a different user who is a Manager tries to approve it
- **THEN** the attempt is refused with a reason stating the second approver must be a Director
- **AND** the invoice remains Pending Approval with only the first approval recorded

### Requirement: Same user cannot approve twice
The same user SHALL NOT count as both approvers on one invoice. Usernames MUST be compared without regard to letter case.

#### Scenario: Same user tries to be the second approver
- **WHEN** a Director approves a pending invoice of 600,000 and then the same Director tries to approve it again
- **THEN** the attempt is refused with a reason stating the same user cannot approve twice
- **AND** the invoice remains Pending Approval with only the first approval recorded

#### Scenario: Same user with different letter case
- **WHEN** the user "demo.director" approves a pending invoice of 600,000 and then "Demo.Director" tries to approve it
- **THEN** the attempt is refused as the same user

### Requirement: Refused attempts change nothing
When an approval attempt is refused, the system SHALL return a clear reason and SHALL NOT change anything about the invoice: its status, its recorded approvers, its decision time and its decision note.

#### Scenario: Refusal reason is shown to the user
- **WHEN** a Manager approves a pending invoice above their limit through the web interface
- **THEN** the page shows the refusal reason and the invoice still appears under pending approvals

#### Scenario: Refusal is reported as forbidden
- **WHEN** an approval attempt is refused by the approval rules through the API
- **THEN** the API responds with status 403 and a body containing the reason

### Requirement: Only approving roles can approve
Only users with the Manager or Director role SHALL be able to approve an invoice. Users with any other role MUST be refused.

#### Scenario: Admin cannot approve
- **WHEN** a user with the Admin role tries to approve a pending invoice
- **THEN** the attempt is refused and the invoice is unchanged
