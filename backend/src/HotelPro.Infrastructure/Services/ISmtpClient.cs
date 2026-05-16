using MimeKit;

namespace HotelPro.Infrastructure.Services;

public interface ISmtpClient : IDisposable
{
    Task ConnectAsync(string host, int port, bool useTls);
    Task AuthenticateAsync(string username, string password);
    Task SendAsync(MimeMessage message);
    Task DisconnectAsync(bool quit);
}
