using API.SignalR;

namespace API.Extensions;

public static class MapControllersExtensions
{
    public static void AddMapControllers(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<PresenceHub>("hubs/presence");
        app.MapHub<MessageHub>("hubs/message");
        app.MapFallbackToController("Index", "Fallback");

    }
}
