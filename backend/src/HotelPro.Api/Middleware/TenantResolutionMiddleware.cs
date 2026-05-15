using HotelPro.Core.Services;

namespace HotelPro.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (path != null && (path == "/health" || path == "/api/health" || path.StartsWith("/api/auth")))
        {
            await _next(context);
            return;
        }

        string? hotelCode = null;

        if (context.Request.Headers.TryGetValue("X-Hotel-Code", out var headerValue))
        {
            hotelCode = headerValue.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(hotelCode))
        {
            var subdomain = context.Request.Host.Host.Split('.').FirstOrDefault();
            if (!string.IsNullOrEmpty(subdomain) && subdomain != "www")
            {
                hotelCode = subdomain;
            }
        }

        if (!string.IsNullOrEmpty(hotelCode))
        {
            var hotel = await tenantService.ResolveByCodeAsync(hotelCode);
            if (hotel != null)
            {
                context.Items["HotelId"] = hotel.Id;
                context.Items["HotelCode"] = hotel.Code;
                _logger.LogDebug("Tenant resolved: {HotelCode} ({HotelId})", hotel.Code, hotel.Id);
            }
            else
            {
                _logger.LogWarning("Tenant not found for code: {HotelCode}", hotelCode);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid hotel code" });
                return;
            }
        }

        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
