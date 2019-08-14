#tool "nuget:?package=xunit.runner.console"
// Importand: Execute Set-ExecutionPolicy RemoteSigned and Set-ExecutionPolicy RemoteSigned -Scope Process as Administrator in x86 and x64 powershell!

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var testreportfolder = Argument("testreportfolder", "testresult").TrimEnd('/');


///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////
Task("Build")
  .Does(() =>
{
    Information($"Clean Output Folders");
    var directoriesToClean = GetDirectories("./**/bin");
    CleanDirectories(directoriesToClean);

    var solutions  = GetFiles("./**/*.sln");
    foreach (var solution in solutions)
    {
        Information($"Build Solution: {solution}");
        MSBuild(solution, configurator =>
            configurator
                .SetConfiguration(configuration)                
                .WithRestore()           
                .SetVerbosity(Verbosity.Minimal)                
                .UseToolVersion(MSBuildToolVersion.VS2017)
                .SetPlatformTarget(PlatformTarget.MSIL));   // MSIL = AnyCPU    
    }    
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{    
    var testAssemblies = GetFiles($"./**/bin/{configuration}/**/*.test.dll");    
    var xunitSettings = new XUnit2Settings {
        Parallelism = ParallelismOption.None,
        UseX86 = false,
        HtmlReport = true,
        JUnitReport = true,
        NoAppDomain = true,                        
        OutputDirectory = $"./{testreportfolder}",
    };     
    
    // Run Tests in x64 Process
    XUnit2(testAssemblies, xunitSettings); 

    // Run Tests in x86 Process
    //xunitSettings.UseX86 = true;
    //xunitSettings.OutputDirectory += "x86";
    //XUnit2(testAssemblies, xunitSettings); 
});

Task("NugetPublish")
    .IsDependentOn("Test")
    .Does(() =>
{    
    string source = "mbcpublic";    // Is defined in nuget.config!
    var serverConfiguration = new NuGetPushSettings() 
    {
        Source = source,
        ApiKey = "VSTS"
    };

    // Collect all nuget files
    var nugetPackages = GetFiles($"./**/bin/{configuration}/**/*.symbols.nupkg");

    foreach (var package in nugetPackages)
    {
        // Push the package
        try
        {
            // ToDo: check for already published!

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
  .IsDependentOn("Build");

RunTarget(target);