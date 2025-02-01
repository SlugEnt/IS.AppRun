namespace SlugEnt.IS.AppRun;

/// <summary>
/// This class is used to store initial Application Runtime Information.
/// </summary>
public class AppRuntimeInfo
{
    /// <summary>
    /// Whether the application is in development mode.  This allows it to use the Dev Vault Hash Key
    /// </summary>
    public bool DevelopmentMode { get; set; } = false;

    /// <summary>
    /// Whether the application should run in debug mode.
    /// </summary>
    public bool DebugMode { get; set; } = false;

    /// <summary>
    /// The name of the application as it is stored in the Vault.
    /// </summary>
    public string AssemblyQualifiedName { get; set; } = "";
}