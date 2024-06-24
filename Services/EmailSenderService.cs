using System.Net;
using System.Net.Mail;

namespace TrekReserve.Auth.Services;

public class EmailSenderService
{
    private readonly ConfigurationService _configuration;
    public EmailSenderService(ConfigurationService configuration)
    {
        _configuration = configuration;

        InitializeSMTPSettings();
    }

    // SMTP setting fields
    private string _smtpHost = string.Empty;
    private string _smtpPort = string.Empty;
    private string _smtpPassword = string.Empty;
    public string SMTPUser { get; set; } = string.Empty;

    public async Task SendEmailAsync(MailMessage mailMessage)
    {
        var smtpClient = new SmtpClient(_smtpHost, int.Parse(_smtpPort));
        var smtpCredentials = new NetworkCredential(SMTPUser, _smtpPassword);

        smtpClient.Credentials = smtpCredentials;

        await smtpClient.SendMailAsync(mailMessage);
    }

    private void InitializeSMTPSettings()
    {
        _smtpHost = _configuration.GetSection("SMTP:Host");
        _smtpPort = _configuration.GetSection("SMTP:Port");
        SMTPUser = _configuration.GetSection("SMTP:User");
        _smtpPassword = _configuration.GetSection("SMTP:Password");
    }
}