using System.IO;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath PackagesDirectory => RootDirectory / "packages";
    
    AbsolutePath SqlLibPath => PackagesDirectory 
                                   / "stub.system.data.sqlite.core.netstandard" 
                                   / "1.0.113.2" 
                                   / "lib" 
                                   / "netstandard2.1" 
                                   / "SQLite.Interop.dll";
    
    AbsolutePath SqlInteropPath => PackagesDirectory 
                               / "stub.system.data.sqlite.core.netstandard" 
                               / "1.0.113.2" 
                               / "runtimes" 
                               / GetPlatform()
                               / "native"
                               / "SQLite.Interop.dll";

    Target Tests => _ => _
        .DependsOn(Clean, Restore, CopySqlInterop, Compile)
        .Executes(() =>
        {
            var tests = GlobFiles(TestsDirectory, "*/bin/*/net*/*.Tests.dll").NotEmpty();
            
            foreach (var test in tests)
            {
                DotNetTest(config =>
                {
                    config = config
                        .SetProjectFile(test);

                    return config;
                });
            }
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(config =>
            {
                config = config
                    .SetConfigFile(RootDirectory / "NuGet.Config")
                    .SetProcessWorkingDirectory(RootDirectory)
                    .SetPackageDirectory(PackagesDirectory);

                return config;
            });
        });

    Target CopySqlInterop => _ => _
        .After(Restore)
        .Executes(() =>
        {
            if (FileExists(SqlInteropPath) && !File.Exists(SqlLibPath))
                CopyFile(SqlInteropPath, SqlLibPath);
        });

    Target Compile => _ => _
        .DependsOn(Clean, Restore, CopySqlInterop)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    private string GetPlatform()
    {
        if(EnvironmentInfo.IsOsx)
            return "osx-x64";
        
        if(EnvironmentInfo.IsLinux)
            return "linux-x64";
        
        if (EnvironmentInfo.Is32Bit)
            return "win-x86";
        
        return "win-x64";
    }
}
