using DocCreator01.Models;

namespace DocCreator01.Messages;

/// <summary>
/// Published via MessageBus when a project has been loaded
/// </summary>
public sealed record ProjectLoadedMessage(Project Project);
