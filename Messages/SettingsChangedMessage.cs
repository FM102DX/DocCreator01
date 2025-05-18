using System;

namespace DocCreator01.Messages
{
    /// <summary>
    /// Broadcasts a settings property change between SettingsViewModel instances.
    /// </summary>
    public sealed record SettingsChangedMessage(string PropertyName, object? Value, object Sender);
}
