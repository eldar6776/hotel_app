using HotelPro.Core.Attributes;
using HotelPro.Core.Entities;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System.Security.Claims;

namespace HotelPro.Tests;

public class AuditInterceptorTests
{
    private AuditInterceptor CreateInterceptor(Mock<IHttpContextAccessor>? httpMock = null)
    {
        return new AuditInterceptor(httpMock?.Object);
    }

    private DbContextOptions<HotelProDbContext> CreateInMemoryOptions(AuditInterceptor interceptor)
    {
        return new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .AddInterceptors(interceptor)
            .Options;
    }

    [Fact]
    public async Task AuditInterceptor_LogsInsert_WhenAuditableEntityAdded()
    {
        var interceptor = CreateInterceptor();
        var options = CreateInMemoryOptions(interceptor);

        await using var context = new HotelProDbContext(options);

        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = "Test Building",
            Code = "TB1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Buildings.Add(building);
        await context.SaveChangesAsync();

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.EntityName == "Building");
        Assert.NotNull(auditLog);
        Assert.Equal("Added", auditLog.Action);
        Assert.Equal(building.Id.ToString(), auditLog.EntityId);
        Assert.NotNull(auditLog.NewValues);
        Assert.Null(auditLog.OldValues);
    }

    [Fact]
    public async Task AuditInterceptor_LogsUpdate_WhenAuditableEntityModified()
    {
        var interceptor = CreateInterceptor();
        var options = CreateInMemoryOptions(interceptor);

        await using (var context = new HotelProDbContext(options))
        {
            context.Buildings.Add(new Building
            {
                Id = Guid.NewGuid(),
                Name = "Original",
                Code = "OR1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new HotelProDbContext(options))
        {
            var building = await context.Buildings.FirstAsync();
            building.Name = "Updated";
            await context.SaveChangesAsync();
        }

        await using (var context = new HotelProDbContext(options))
        {
            var updateLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "Modified");
            Assert.NotNull(updateLog);
            Assert.NotNull(updateLog.OldValues);
            Assert.NotNull(updateLog.NewValues);
            Assert.Contains("Original", updateLog.OldValues);
            Assert.Contains("Updated", updateLog.NewValues);
        }
    }

    [Fact]
    public async Task AuditInterceptor_CapturesUserId_FromJwtClaims()
    {
        var userId = Guid.NewGuid().ToString();
        var email = "admin@hotelpro.local";

        var claims = new List<Claim>
        {
            new Claim("sub", userId),
            new Claim("email", email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.User).Returns(user);
        httpContextMock.Setup(c => c.Connection).Returns(new Mock<ConnectionInfo>().Object);

        var httpAccessorMock = new Mock<IHttpContextAccessor>();
        httpAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var interceptor = CreateInterceptor(httpAccessorMock);
        var options = CreateInMemoryOptions(interceptor);

        await using var context = new HotelProDbContext(options);

        context.Buildings.Add(new Building
        {
            Id = Guid.NewGuid(),
            Name = "Claim Test",
            Code = "CT1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Equal(userId, auditLog.ChangedById?.ToString());
        Assert.Equal(email, auditLog.ChangedByEmail);
    }

    [Fact]
    public async Task AuditInterceptor_CapturesIpAddress_FromHttpContext()
    {
        var ipString = "192.168.1.100";
        var ip = System.Net.IPAddress.Parse(ipString);

        var connectionInfoMock = new Mock<ConnectionInfo>();
        connectionInfoMock.Setup(c => c.RemoteIpAddress).Returns(ip);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Connection).Returns(connectionInfoMock.Object);

        var httpAccessorMock = new Mock<IHttpContextAccessor>();
        httpAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var interceptor = CreateInterceptor(httpAccessorMock);
        var options = CreateInMemoryOptions(interceptor);

        await using var context = new HotelProDbContext(options);

        context.Buildings.Add(new Building
        {
            Id = Guid.NewGuid(),
            Name = "IP Test",
            Code = "IP1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Equal(ipString, auditLog.IpAddress);
    }

    [Fact]
    public async Task AuditInterceptor_DoesNotLog_NonAuditableEntity()
    {
        var interceptor = CreateInterceptor();
        var options = CreateInMemoryOptions(interceptor);

        await using var context = new HotelProDbContext(options);

        context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = "Test",
            EntityId = "1",
            Action = "Test",
            ChangedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var count = await context.AuditLogs.CountAsync(a => a.EntityName == "AuditLog");
        Assert.Equal(0, count);
    }
}
