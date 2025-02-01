using Microsoft.Extensions.Configuration;
using System.Text;


namespace SlugEnt.IS.AppRun;

/// <summary>
/// Builds the Appsettings Configuration for a Sheakley App.  Provides for our standardized way of looking for and loading from Various AppSettings files.
/// It also allows for the addition of other Appsettings files as well into the process.
/// Standard order is:
/// <para>  - Environment Variables which can be loaded by the calling application</para>
/// <para>  - The Appsettings file from the current directory</para>
/// <para>  - The AppSettings file from the parent directory of the current directory the application is running from (This is our std for apps deployed via Service Installer)</para>
/// <para>  - Sensitive App Settings File.  Must have provided the Sensitive Name property.  This is rarely used, but provides the calling application a way to store sensitive information somewhere else.</para>
/// <para>  - Sheakley Vault.  If the VaultClientConnector is provided, then the vault will be used to load settings.</para>
/// <para>  - Any files added with the AddSettingFile method.  These are added in the order they are added.</para>
/// </summary>
public class AppSettingsConfig
{
    private readonly IConfigurationBuilder _configurationBuilder;

    private readonly List<string> ConfigFiles = new();

    /// <summary>
    /// This is a list of custom AppSettings Files to use.  These are full path and file name values, including extensions
    /// </summary>
    private readonly List<string> CustomFiles = new();

    private string _sensitiveName = string.Empty;


    /// <summary>
    /// Environment variables to add to the configuration
    /// </summary>
    private readonly List<string> EnvironmentVariables = new();



    /// <summary>
    /// Constructor for the AppSettingsConfig
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AppSettingsConfig(IConfigurationBuilder configurationBuilder)
    {
        _configurationBuilder = configurationBuilder;
        DirectoryCurrent      = Environment.CurrentDirectory;

        Initialize();
    }



    /// <summary>
    /// Constructor accepting a FileSystem object.  This is used for testing purposes.
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="fileSystem"></param>
    public AppSettingsConfig(IConfigurationBuilder configurationBuilder,
                             string currentDirectory)
    {
        _configurationBuilder = configurationBuilder;
        DirectoryCurrent      = currentDirectory;
        Initialize();
    }


    /// <summary>
    /// Performs initialization so the object is ready to be used.
    /// </summary>
    public void Initialize()
    {
        if (!Directory.Exists(DirectoryCurrent))
            throw new Exception("The Current Directory does not exist.  It must be a valid directory.  Not sure how this could ever happen!");

        DirectoryParent = Directory.GetParent(DirectoryCurrent).FullName;


        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (EnvironmentName == null)
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (EnvironmentName == null)
            Console.WriteLine("EnvironmentName is null. It should be specified in a normal run mode.");
    }


    /// <summary>
    /// The Current Directory the application is run from.
    /// </summary>
    public string DirectoryCurrent { get; set; } = string.Empty;

    /// <summary>
    /// The Parent of the current directory being run from.  For our server deployed applications we put the permanent non changing appconfig in that directory.
    /// </summary>
    public string DirectoryParent { get; set; } = string.Empty;

    /// <summary>
    /// Name of the current Environment
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// If true, then the Sensitive Appsetings file will be added.
    /// </summary>
    public bool UseSensitiveAppSettings { get; set; } = false;

    /// <summary>
    /// If true the Parent Folder AppSettings file will be added.
    /// </summary>
    public bool UseParentAppSettings { get; set; } = true;

    /// <summary>
    /// f true the appsettings file in the deployed folder will be used.
    /// </summary>
    public bool UseDeployedFolderAppSettings { get; set; } = true;


    /// <summary>
    /// Adds the given environment variable to the list of environment variables to be added to the configuration.
    /// </summary>
    /// <param name="envVarName"></param>
    public void AddEnvironmentVariables(string envVarName) { EnvironmentVariables.Add(envVarName); }



    /// <summary>
    /// The name of the sensitive Appsetting file name.  Include the full path and extension
    /// </summary>
    public string SensitiveName
    {
        get { return _sensitiveName; }
        set
        {
            _sensitiveName          = value;
            UseSensitiveAppSettings = true;
        }
    }


    /// <summary>
    /// Adds the Specified AppSetting File to the list to be included.  They will be included in the order they are added.
    /// </summary>
    /// <param name="fullPathandName">Full path and file name, including extension</param>
    /// <param name="requiredToExist">If true, the app will abort if the file does not exist.</param>
    /// <exception cref="Exception"></exception>
    public void AddSettingFile(string fullPathandName,
                               bool requiredToExist = false)
    {
        if (requiredToExist)
            if (!System.IO.File.Exists(fullPathandName))
                throw new Exception($"The AppSetting file {fullPathandName} does not exist and is required.");

        CustomFiles.Add(fullPathandName);
    }



    /// <summary>
    /// Builds the Actual Application Configuration based upoon the current environment.
    /// </summary>
    /// <returns></returns>
    public void Build()
    {
        // A.  Add all the files to the appropriate lists.
        // Add Environment Variables.  These are first so they can be overridden as needed.
        foreach (string envVar in EnvironmentVariables)
            _configurationBuilder.AddEnvironmentVariables(envVar);

        // Add the appropriate Appsettings files.  These are always the first settings files added (Meaning there values can be overridden by other files.
        SetAppSettingsDirNoEnvValue(DirectoryCurrent);
        SetAppSettingsDirNoEnvValue(DirectoryParent);

        SetAppSettingsDirAndEnvValue(DirectoryCurrent);
        SetAppSettingsDirAndEnvValue(DirectoryParent);


        // This file is always last added.
        if (UseSensitiveAppSettings)
            AddSettingFile(SensitiveName, true);


        //************************************************************************************
        // B.  Actually Add the lists of files to the Configuration
        // Add Config files
        foreach (string file in ConfigFiles)
            AddJsonRecord(_configurationBuilder, file);


        // Add Custom Files
        foreach (string customFile in CustomFiles)
            AddJsonRecord(_configurationBuilder, customFile);
    }



    /// <summary>
    /// Adds the AppSetting file to the ConfigurationBuilder
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="file"></param>
    private void AddJsonRecord(IConfigurationBuilder configurationBuilder,
                               string file)
    {
        // See if it exists
        if (System.IO.File.Exists(file))
        {
            Console.WriteLine("AppSetting File [ {0} ] exists and will be used.", file);
            configurationBuilder.AddJsonFile(file, optional: true, reloadOnChange: true);
        }
        else
        {
            Console.WriteLine("Optional AppSetting File [ {0} ] does not exist and will not be used.", file);
        }
    }



    /// <summary>
    /// Sets an Appsetting file value based upon the path and environment name.  Adds it to tjhe Config List
    /// </summary>
    /// <param name="path"></param>
    private void SetAppSettingsDirAndEnvValue(string path,
                                              bool isRequired = false)
    {
        String file = "";
        if (!string.IsNullOrWhiteSpace(EnvironmentName))
            file = Path.Join(path, $"appsettings." + EnvironmentName + ".json");

        if (isRequired)
        {
            if (!System.IO.File.Exists(file))
            {
                Console.WriteLine("The AppSetting file {file} does not exist and is required.", file);
                throw new ApplicationException("The Appsetting file [ " + file + " ] does not exist!  It is required.");
            }
        }

        ConfigFiles.Add(file);
    }



    /// <summary>
    /// Sets an Appsetting file value based upon the path and environment name.  Adds it to tjhe Config List
    /// </summary>
    /// <param name="path"></param>
    private void SetAppSettingsDirNoEnvValue(string path,
                                             bool isRequired = false)
    {
        String file = "";
        file = Path.Join(path, $"appsettings.json");

        if (isRequired)
        {
            if (!System.IO.File.Exists(file))
            {
                Console.WriteLine("The AppSetting file {file} does not exist and is required.", file);
                throw new ApplicationException("The Appsetting file [ " + file + " ] does not exist!  It is required.");
            }
        }

        ConfigFiles.Add(file);
    }
}