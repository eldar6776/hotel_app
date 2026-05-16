using HotelPro.Core.Services;
using MailKit.Net.Smtp;
using MimeKit;

namespace HotelPro.Infrastructure.Services;

public class MailKitSmtpClient : ISmtpClient
{
    private readonly SmtpClient _client = new();

    public async Task ConnectAsync(string host, int port, bool useTls)
    {
        await _client.ConnectAsync(host, port, useTls ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None);
    }

    public async Task AuthenticateAsync(string username, string password)
    {
        await _client.AuthenticateAsync(username, password);
    }

    public async Task SendAsync(MimeMessage message)
    {
        await _client.SendAsync(message);
    }

    public async Task DisconnectAsync(bool quit)
    {
        await _client.DisconnectAsync(quit);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
