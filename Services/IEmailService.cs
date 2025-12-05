namespace HRMCyberse.Services;

public interface IEmailService
{
    Task SendEmployeeInvitationAsync(string toEmail, string branchCode, string invitationToken, string branchName, string? departmentName = null, string? positionTitle = null, decimal? salaryRate = null, string? roleName = null);
}
