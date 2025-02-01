using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;
using ILogger = Serilog.ILogger;


namespace SlugEnt.IS.AppRun;

/// <summary>
/// Core Application runtime information.  Required by majority of Sheakley Applications in order to run.
/// </summary>
public class AppRuntime
{
    /// <summary>
    /// AppSettings Configuration Builder for the application
    /// </summary>
    public AppSettingsConfig AppSettings { get; private set; }

    /// <summary>
    /// The Microsoft Configuration object for the application
    /// </summary>
    public IConfiguration? AppConfiguration { get; private set; } = null;


    public Serilog.ILogger? Logger { get; private set; } = null;

    /// <summary>
    /// Object that builds the configuration
    /// </summary>
    private ConfigurationBuilder ConfigurationBuilder { get; set; }

    public string AppFullName { get; private set; }

    /// <summary>
    /// Object that holds all Vault information
    /// </summary>
    public AppRuntimeInfo AppRuntimeInfo { get; private set; }



    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="appFullName"></param>
    /// <param name="AppRuntimeInfo"></param>
    public AppRuntime(string appFullName,
                      AppRuntimeInfo appRuntimeInfo)
    {
        AppFullName          = appFullName;
        AppRuntimeInfo       = appRuntimeInfo;
        ConfigurationBuilder = new ConfigurationBuilder();
        AppSettings          = new(ConfigurationBuilder);
    }



    /// <summary>
    /// Initializes the Vault and Logging for the application.  
    /// </summary>
    /// <returns></returns>
    public bool SetupLogging()
    {
        // B.  AppSettings Logic
        AppSettings.Build();

        AppConfiguration = ConfigurationBuilder.Build();


        // Logging Setup this is initial for logging during the build process! This is later replaced by the UseSerilog section later once the builder is built!
        LoggerConfiguration logconfig = new LoggerConfiguration()
                                        .ReadFrom.Configuration(AppConfiguration);


        // This provides the context for this initial logger.  If not provided then there is no context!
        Serilog.Core.Logger logger = logconfig.CreateLogger();
        Logger = logger.ForContext("SourceContext", AppFullName);
        Log.Information($"Starting {Assembly.GetEntryAssembly().FullName}");
        return false;
    }
}