using SendGrid;
using SendGrid.Helpers.Mail;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string plainTextBody, CancellationToken ct = default);
}

public class SendGridEmailSender : IEmailSender
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailSender(IConfiguration config)
    {
        _apiKey = config["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid:ApiKey not configured");
        _fromEmail = config["SendGrid:FromEmail"] ?? throw new ArgumentNullException("SendGrid:FromEmail not configured");
        _fromName = config["SendGrid:FromName"] ?? "Crowded Exams";
    }

    public async Task SendAsync(string to, string subject, string htmlBody, string plainTextBody, CancellationToken ct = default)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextBody, htmlBody);
        
        var response = await client.SendEmailAsync(msg, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(ct);
            throw new Exception($"SendGrid error: {response.StatusCode} - {body}");
        }
    }
}