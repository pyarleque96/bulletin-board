namespace Bulletin.Board.Web.Client;

/// <summary>
/// Marker class for IStringLocalizer&lt;SharedResource&gt;.
/// Must be in the root namespace of the assembly (Bulletin.Board.Web.Client),
/// NOT inside the Resources sub-namespace, so that ResourceManagerStringLocalizerFactory
/// correctly resolves Resources/SharedResource.resx as the embedded resource.
/// Resolution formula: {AssemblyName}.{ResourcesPath}.{TypeName}
///   = Bulletin.Board.Web.Client.Resources.SharedResource  ✓
/// </summary>
public class SharedResource { }
