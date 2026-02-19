using Microsoft.AspNetCore.Http.HttpResults;

public class OneApiMiddleware
{
    private readonly RequestDelegate _next;

    public OneApiMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // run ONLY for this path
        if (context.Request.Path.StartsWithSegments("/api/users"))
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");

                return;
            }
        }

        await _next(context);
    }
}
