namespace YorkshireWater.Api.Domain
{
    /// <summary>
    /// The possible outcomes of an update, modelled as a closed set of types so callers have to handle each case explicitly (the endpoints will map each one to an HTTP status)
    /// </summary>
    public abstract record UpdatePreferencesResult
    {
        public sealed record Updated(NotificationPreferences Preferences) : UpdatePreferencesResult;
        public sealed record CustomerNotFound : UpdatePreferencesResult;
        public sealed record NotificationFailed(NotificationPreferences Preferences) : UpdatePreferencesResult;
    }
}
