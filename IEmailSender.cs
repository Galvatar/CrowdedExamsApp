using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

public interface IEmailSender
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default);
}

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config) => _config = config;

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default)
    {
        var smtp = _config.GetSection("Smtp");
        var host = smtp["Host"]!;
        var port = int.Parse(smtp["Port"] ?? "587");
        var user = smtp["User"];
        var pass = smtp["Pass"];
        var from = smtp["From"]!;
        var fromName = smtp["FromName"] ?? from;
        var useStartTls = bool.Parse(smtp["UseStartTls"] ?? "true");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, from));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var body = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = plainTextBody ?? "View this email in an HTML-capable client."
        };
        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        await client.ConnectAsync(host, port, socketOptions, ct);

        if (!string.IsNullOrWhiteSpace(user))
            await client.AuthenticateAsync(user, pass, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}