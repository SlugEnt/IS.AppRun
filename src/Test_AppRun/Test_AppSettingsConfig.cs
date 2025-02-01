using Microsoft.Extensions.Configuration;
using System.Text.Json;
using SlugEnt.IS.AppRun;
using NUnit.Framework;


public class Test_AppSettingsConfig
{
    private readonly string rootDir = "C:\\Temp\\isapprunTest";


    [SetUp]
    public void Setup()
    {
        if (Directory.Exists(rootDir))
            Directory.Delete(rootDir, true);
    }


    /// <summary>
    /// Current Directory is accessed
    /// </summary>
    [Test]
    [Order(1)]
    public void Test_DirectoryCurrent()
    {
        // Arrange

        // Create Folders.  //Parent/Current, //Sensitive, //Other

        string dirParent  = Directory.CreateDirectory(Path.Combine(rootDir, "Parent")).FullName;
        string dirCurrent = Directory.CreateDirectory(Path.Combine(dirParent, "Current")).FullName;


        // Create Settings files in directories.
        SampleSettingFile sampleCurrent = new("Current", 2, 2000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.json"), sampleCurrent);


        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        AppSettingsConfig     appSettingsConfig    = new AppSettingsConfig(configurationBuilder, dirCurrent);


        // Act
        // No need to add directories the defaults should be used for this test.
        appSettingsConfig.Build();
        IConfiguration configuration = configurationBuilder.Build();


        // Assert
        // Read the configs.  The Parent should be overriding the current.
        Assert.That(configuration["Setting1"], Is.EqualTo("Current"), "A10: Setting1 not as expected.");
    }



    // Test Parent Overrides Current
    [Test]
    [Order(2)]
    public void Test_DirectoryParentOverridesCurrent()
    {
        // Arrange

        // Create Folders.  //Parent/Current, //Sensitive, //Other

        string dirParent    = Directory.CreateDirectory(Path.Combine(rootDir, "Parent")).FullName;
        string dirCurrent   = Directory.CreateDirectory(Path.Combine(dirParent, "Current")).FullName;
        string dirSensitive = Directory.CreateDirectory(Path.Combine(rootDir, "Sensitive")).FullName;
        string dirOther     = Directory.CreateDirectory(Path.Combine(rootDir, "Other")).FullName;


        // Create Settings files in directories.
        SampleSettingFile sampleParent = new("ParentProd", 1, 1000);
        WriteSettingFile(Path.Combine(dirParent, "appsettings.json"), sampleParent);
        SampleSettingFile sampleCurrent = new("CurrentProd", 2, 2000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.json"), sampleCurrent);


        ConfigurationBuilder configurationBuilder = new();
        AppSettingsConfig    appSettingsConfig    = new(configurationBuilder, dirCurrent);


        // Act
        // No need to add directories the defaults should be used for this test.
        appSettingsConfig.Build();
        IConfiguration configuration = configurationBuilder.Build();


        // Assert
        // Read the configs.  The Parent should be overriding the current.
        Assert.That(configuration["Setting1"], Is.EqualTo("ParentProd"), "A10: Setting1 not as expected.");
    }


    /// <summary>
    /// AppSetting.Environment takes precedence over appsetting.json
    /// </summary>
    [Test]
    [Order(3)]
    public void Test_DirectoryEnvPrecedenceOverNone()
    {
        // Arrange

        // Create Folders.  //Parent/Current, //Sensitive, //Other

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        string dirParent  = Directory.CreateDirectory(Path.Combine(rootDir, "Parent")).FullName;
        string dirCurrent = Directory.CreateDirectory(Path.Combine(dirParent, "Current")).FullName;


        // Create Settings files in directories.
        SampleSettingFile sampleCurrent = new("Current", 2, 2000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.json"), sampleCurrent);

        SampleSettingFile sampleCurrentEnv = new("CurProd", 3, 3000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.development.json"), sampleCurrentEnv);


        ConfigurationBuilder configurationBuilder = new();
        AppSettingsConfig    appSettingsConfig    = new(configurationBuilder, dirCurrent);


        // Act
        // No need to add directories the defaults should be used for this test.
        appSettingsConfig.Build();
        IConfiguration configuration = configurationBuilder.Build();


        // Assert
        // Read the configs.  The Parent should be overriding the current.
        Assert.That(configuration["Setting1"], Is.EqualTo("CurProd"), "A10: Setting1 not as expected.");
    }


    /// <summary>
    /// AppSetting.Environment in parent takes precedence over all others appsetting.json
    /// </summary>
    [Test]
    [Order(4)]
    public void Test_DirectoryParentEnvPrecedenceOverNone()
    {
        // Arrange

        // Create Folders.  //Parent/Current, //Sensitive, //Other

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        string dirParent  = Directory.CreateDirectory(Path.Combine(rootDir, "Parent")).FullName;
        string dirCurrent = Directory.CreateDirectory(Path.Combine(dirParent, "Current")).FullName;


        // Create Settings files in directories.
        SampleSettingFile sampleCurrent = new("Current", 2, 2000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.json"), sampleCurrent);

        SampleSettingFile sampleCurrentEnv = new("CurProd", 3, 3000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.development.json"), sampleCurrentEnv);

        SampleSettingFile sampleParent = new("Parent", 4, 4000);
        WriteSettingFile(Path.Combine(dirCurrent, "appsettings.json"), sampleCurrent);

        SampleSettingFile sampleParentEnv = new("ParentProd", 5, 5000);
        WriteSettingFile(Path.Combine(dirParent, "appsettings.development.json"), sampleParentEnv);


        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        AppSettingsConfig     appSettingsConfig    = new(configurationBuilder, dirCurrent);


        // Act
        // No need to add directories the defaults should be used for this test.
        appSettingsConfig.Build();
        IConfiguration configuration = configurationBuilder.Build();


        // Assert
        // Read the configs.  The Parent should be overriding the current.
        Assert.That(configuration["Setting1"], Is.EqualTo("ParentProd"), "A10: Setting1 not as expected.");
    }



    /// <summary>
    /// Writes the given settings to the given file
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="fileNameAndPath"></param>
    /// <param name="sampleSettingFile"></param>
    private static void WriteSettingFile(string fileNameAndPath,
                                         SampleSettingFile sampleSettingFile)
    {
        string json = JsonSerializer.Serialize(sampleSettingFile);
        File.WriteAllText(fileNameAndPath, json);
    }


    private class SampleSettingFile
    {
        public SampleSettingFile(string setting1,
                                 int setting2,
                                 int setting3)
        {
            Setting1 = setting1;
            Setting2 = setting2;
            Setting3 = setting3;
        }


        public string Setting1 { get; set; }
        public int Setting2 { get; set; }
        public int Setting3 { get; set; }
    }
}