/*
 * Build solutions using dotnet build
 */

#region Addins

#addin nuget:?package=Cake.VersionReader&version=5.1.0

#endregion

# region Variables



#endregion

#region Tasks

Task ("Build")
    .Does(() => {
        var blockText = "Build";
        StartBuildBlock(blockText);

        foreach(var solution in solutionFiles)
        {
            Information($"Building: {solution.Key}...");

            DotNetBuild(solution.Value, new DotNetBuildSettings 
                {
                    
                });

            var releaseFolder = string.Format(releaseFolderString, solution.Key, buildConfiguration);
            var file = MakeAbsolute(Directory(releaseFolder)) + $"/{solution.Key}.{releaseBinaryType}";
            version = GetVersionNumber(file);
            ciVersion = GetVersionNumberWithContinuesIntegrationNumberAppended(file, buildCounter);
            Information("Version: " + version);
            Information("CI Version: " + ciVersion);
            PushVersion(ciVersion);
        }

        EndBuildBlock(blockText);
    });

#endregion