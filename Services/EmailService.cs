using System.Net;
using System.Net.Mail;

namespace HRMCyberse.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmployeeInvitationAsync(string toEmail, string branchCode, string invitationToken, string branchName, string? departmentName = null, string? positionTitle = null, decimal? salaryRate = null, string? roleName = null)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "HRM Cyberse";

            // Nếu chưa config SMTP, chỉ log ra console (cho dev)
            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogWarning("SMTP chưa được cấu hình. Email thông báo trúng tuyển:");
                _logger.LogWarning($"To: {toEmail}");
                _logger.LogWarning($"Branch Code: {branchCode}");
                _logger.LogWarning($"Branch Name: {branchName}");
                _logger.LogWarning($"Department: {departmentName}");
                _logger.LogWarning($"Position: {positionTitle}");
                _logger.LogWarning($"Salary: {salaryRate}");
                _logger.LogWarning($"Role: {roleName}");
                return;
            }

            // Validate email addresses
            var emailAddress = fromEmail ?? smtpUsername;
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new InvalidOperationException("Email:FromEmail hoặc Email:Username phải được cấu hình");
            }

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                throw new InvalidOperationException("Email:Username và Email:Password phải được cấu hình");
            }

            // Validate và clean toEmail
            var cleanToEmail = toEmail?.Trim();
            if (string.IsNullOrEmpty(cleanToEmail))
            {
                throw new ArgumentException("Email người nhận không hợp lệ", nameof(toEmail));
            }

            _logger.LogInformation($"Đang gửi email từ {emailAddress} đến {cleanToEmail}");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailAddress),
                Subject = $"Thông báo trúng tuyển - {branchName}",
                Body = GetEmailBody(branchCode, branchName, departmentName, positionTitle, salaryRate, roleName),
                IsBodyHtml = true
            };

            mailMessage.To.Add(cleanToEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Đã gửi email thông báo trúng tuyển đến {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi gửi email đến {toEmail}");
            throw;
        }
    }

    private string GetEmailBody(string branchCode, string branchName, string? departmentName, string? positionTitle, decimal? salaryRate, string? roleName)
    {
        var departmentInfo = !string.IsNullOrEmpty(departmentName) ? $"<p><strong>Phòng ban:</strong> {departmentName}</p>" : "";
        var positionInfo = !string.IsNullOrEmpty(positionTitle) ? $"<p><strong>Chức vụ:</strong> {positionTitle}</p>" : "";
        var salaryInfo = salaryRate.HasValue ? $"<p><strong>Mức lương:</strong> {salaryRate:N0} VND</p>" : "";
        var roleInfo = !string.IsNullOrEmpty(roleName) ? $"<p><strong>Vai trò:</strong> {roleName}</p>" : "";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 20px; }}
        .info-box {{ background: white; padding: 20px; border-left: 4px solid #4CAF50; margin: 20px 0; }}
        .branch-code {{ font-size: 24px; font-weight: bold; color: #4CAF50; text-align: center; padding: 15px; background: white; border: 2px dashed #4CAF50; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Chúc mừng! Bạn đã trúng tuyển</h1>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Chúc mừng bạn đã trúng tuyển vào <strong>{branchName}</strong>!</p>
            
            <div class='info-box'>
                <h3>Thông tin công việc của bạn:</h3>
                {departmentInfo}
                {positionInfo}
                {salaryInfo}
                {roleInfo}
            </div>
            
            <p><strong>Mã chi nhánh để đăng ký tài khoản:</strong></p>
            <div class='branch-code'>{branchCode}</div>
            
            <p>Vui lòng sử dụng mã chi nhánh trên để đăng ký tài khoản trên hệ thống HRM Cyberse.</p>
            <p>Sau khi đăng ký, hệ thống sẽ tự động gán cho bạn các thông tin công việc đã được thiết lập.</p>
            
            <p><strong>Lưu ý:</strong> Vui lòng đăng ký trong vòng 7 ngày kể từ khi nhận email này.</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động từ HRM Cyberse. Vui lòng không trả lời email này.</p>
        </div>
    </div>
</body>
</html>";
    }
}
