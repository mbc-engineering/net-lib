#tool "nuget:?package=xunit.runner.console"
// Importand: Execute Set-ExecutionPolicy RemoteSigned and Set-ExecutionPolicy RemoteSigned -Scope Process as Administrator in x86 and x64 powershell!

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var testreportfolder = Argument("testreportfolder", "testresult").TrimEnd('/');
var testresultsfile = Argument("testresultsfile", "testresults.trx");
var nugetapikey =  Argument("apikey", "apikeymissing");


///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////
Task("Clean")
  .Does(() =>
{
    DotNetCoreClean("net-lib.sln", new DotNetCoreCleanSettings()
    {
        Configuration = configuration,
    });
    
    Information($"Clean Output Folders");
    var directoriesToClean = GetDirectories("./**/bin");
    CleanDirectories(directoriesToClean);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var solutions  = GetFiles("./**/*.sln");
    foreach (var solution in solutions)
    {
        Information($"Build Solution: {solution}");
        MSBuild(solution, configurator =>
            configurator
                .SetConfiguration(configuration)                
                .WithRestore()           
                .SetVerbosity(Verbosity.Minimal)                
                .UseToolVersion(MSBuildToolVersion.VS2019)
                .SetMSBuildPlatform(MSBuildPlatform.Automatic)
                .SetPlatformTarget(PlatformTarget.MSIL));   // MSIL = AnyCPU    
    }    
});

Task("Test")
    .IsDependentOn("Clean")
    .Does(() =>
{    
     var settings = new DotNetCoreTestSettings
     {
         Configuration = configuration,
         Logger = $"trx;LogFileName=\"{testresultsfile}\"",  // by default results in ..\TestResults\testresults.trx
     };

     var projectFiles = GetFiles("./**/*.sln");

     foreach(var file in projectFiles)
     {
         DotNetCoreTest(file.FullPath, settings);
     }
});

Task("NugetPublish")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .Does(() =>
{    
    string source = "nuget.org";    // Is defined in nuget.config!

    Information($"publish nuget to {source} with api key {nugetapikey}");

    var serverConfiguration = new NuGetPushSettings() 
    {
        Source = source,
        ApiKey = nugetapikey,
    };

    // Collect all nuget files
    // !!! NuGet will publish both packages to nuget.org. MyPackage.nupkg will be published first, followed by MyPackage.snupkg.
    var nugetPackages = GetFiles($"./**/bin/{configuration}/**/*.nupkg");

    foreach (var package in nugetPackages)
    {
        // Push the package
        try
        {
            NuGetPush(package, serverConfiguration);
        }
        catch (CakeException cex)
        {
            Information(cex);   // Should be somthing like: Response status code does not indicate success: 409 (Conflict - The feed already contains 'Mbc.Common.Interface 0.1.0'. (DevOps Activity ID: EC51694F-2AFF-4B2F-A98F-58FBC3C974FB)).
            Information($"Nuget package {package} perhaps is already published at {source}. It will not be try to publish it in this task!");

        }
    }      
});


Task("Default")
  .IsDependentOn("NugetPublish");

RunTarget(target);