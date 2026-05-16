using HotelPro.Infrastructure.Services;
using MimeKit;

namespace HotelPro.Tests.UnitTests;

public class FakeSmtpClient : ISmtpClient
{
    public bool Connected { get; private set; }
    public bool Authenticated { get; private set; }
    public bool Disconnected { get; private set; }
    public bool SendCalled { get; private set; }
    public MimeMessage? LastMessage { get; private set; }

    public bool ShouldThrowOnConnect { get; set; }
    public bool ShouldThrowOnAuth { get; set; }
    public bool ShouldThrowOnSend { get; set; }
    public string ThrowMessage { get; set; } = "SMTP error";

    public Task ConnectAsync(string host, int port, bool useTls)
    {
        if (ShouldThrowOnConnect)
            throw new InvalidOperationException(ThrowMessage);
        Connected = true;
        return Task.CompletedTask;
    }

    public Task AuthenticateAsync(string username, string password)
    {
        if (ShouldThrowOnAuth)
            throw new InvalidOperationException(ThrowMessage);
        Authenticated = true;
        return Task.CompletedTask;
    }

    public Task SendAsync(MimeMessage message)
    {
        if (ShouldThrowOnSend)
            throw new InvalidOperationException(ThrowMessage);
        SendCalled = true;
        LastMessage = message;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(bool quit)
    {
        Disconnected = true;
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
