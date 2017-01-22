//Addins
#addin nuget:?package=Cake.VersionReader
#addin nuget:?package=Cake.FileHelpers
#addin nuget:?package=Cake.Coveralls
#addin nuget:?package=Cake.PaketRestore

//Tools
#tool nuget:?package=GitReleaseNotes
#tool nuget:?package=NUnit.ConsoleRunner
#tool nuget:?package=OpenCover
#tool coveralls.io

//Project Variables
var projectName = "VaraniumSharp";
var sln = string.Format("./{0}.sln", projectName);
var releaseFolder = string.Format("./{0}/bin/Release", projectName);
var releaseDll = string.Format("/{0}.dll", projectName);
var nuspecFile = string.Format("./{0}/{0}.nuspec", projectName);
var paketDirectory = "./.paket";

//Unit Tests
var unitTestFilter = "./*Tests/bin/Release/*.Tests.dll";
var testResultFile = "./TestResult.xml";
var errorResultFile = "./ErrorResult.xml";
var releaseNotes = "./ReleaseNotes.md";
var testsSucceeded = true;

//Coverage Settings
var coverallRepoToken = "";

//Arguments
var target = Argument ("target", "Build");
var buildType = Argument<string>("buildType", "develop");
var buildCounter = Argument<int>("buildCounter", 0);

//Execution Variables
var version = "0.0.0";
var ciVersion = "0.0.0-CI00000";
var runningOnTeamCity = false;
var runningOnAppVeyor = false;
var releaseNotesText = "";

//Code Coverage
var coverPath = "./coverageResults.xml";

// Find out if we are running on a Build Server
Task ("DiscoverBuildDetails")
	.Does (() =>
	{
		runningOnTeamCity = TeamCity.IsRunningOnTeamCity;
		Information("Running on TeamCity: " + runningOnTeamCity);
		runningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
		Information("Running on AppVeyor: " + runningOnAppVeyor);
	});

Task ("OutputVariables")
	.Does (() =>
	{
		Information("BuildType: " + buildType);
		Information("BuildCounter: " + buildCounter);
	});

Task ("Build")
	.Does (() => {
		DotNetBuild (sln, c => c.Configuration = "Release");
		var file = MakeAbsolute(Directory(releaseFolder)) + releaseDll;
		version = GetVersionNumber(file);
		ciVersion = GetVersionNumberWithContinuesIntegrationNumberAppended(file, buildCounter);
		Information("Version: " + version);
		Information("CI Version: " + ciVersion);
		PushVersion(ciVersion);
	});

// Unit Tests
Task ("UnitTests")
    .Does (() =>
    {
        var blockText = "Unit Tests";
        StartBlock(blockText);

        var testAssemblies = GetFiles(unitTestFilter);

        OpenCover(tool =>
        {
            tool.NUnit3(testAssemblies, new NUnit3Settings
            {
                ErrorOutputFile = errorResultFile,
                OutputFile = testResultFile,
                TeamCity = runningOnTeamCity,
                Full = true,
                WorkingDirectory = ".",
                Work = MakeAbsolute(Directory("."))
            });
        },
        new FilePath(coverPath),
        new OpenCoverSettings()
			.WithFilter(string.Format("+[{0}]*", projectName))
			.WithFilter(string.Format("-[{0}.Tests]*", projectName))
			.ExcludeByAttribute("System.CodeDom.Compiler.GeneratedCodeAttribute")
        );

        PushTestResults(testResultFile);

        if(FileExists(errorResultFile) && FileReadLines(errorResultFile).Count() > 0)
        {
            Information("Unit tests failed");
            testsSucceeded = false;
        }

        EndBlock(blockText);
    });

Task ("CoverageUpload")
	.Does (() => {

		coverallRepoToken = EnvironmentVariable("CoverallRepoToken");

		CoverallsIo(coverPath, new CoverallsIoSettings()
		{
			RepoToken = coverallRepoToken
		});
	});
	
Task ("GenerateReleaseNotes")
	.WithCriteria (buildType == "master")
	.Does (() => {
		var releasePath = MakeAbsolute(File(releaseNotes));
		GitReleaseNotes(releasePath, new GitReleaseNotesSettings{
			    WorkingDirectory = ".",
				Version = version,
				AllLabels = true
		});

		releaseNotesText = FileReadText(releasePath);
		Information(releaseNotesText);
	});

Task ("Nuget")
	.WithCriteria (buildType == "master")
	.Does (() => {
		if(!testsSucceeded)
		{
			Error("Unit tests failed - Cannot push to Nuget");
			throw new Exception("Unit tests failed");
		}

		CreateDirectory ("./nupkg/");
		ReplaceRegexInFiles(nuspecFile, "0.0.0", version);
		//ReplaceRegexInFiles(nuspecFile, "ReleaseNotesHere", releaseNotesText);
		
		NuGetPack (nuspecFile, new NuGetPackSettings { 
			Verbosity = NuGetVerbosity.Detailed,
			OutputDirectory = "./nupkg/"
		});	
	});

//Restore Paket
Task ("PaketRestore")
	.Does (() => {
		StartBlock("Restoring Paket");
		
		PaketRestore(MakeAbsolute(Directory(paketDirectory)), new PaketRestoreSettings{
			RetrieveBootstrapper = true,
			RetrievePaketExecutable = true
		});

		EndBlock("Restoring Paket");
	});

//Push to Nuget
Task ("Push")
	.WithCriteria (buildType == "master")
	.Does (() => {
		// Get the newest (by last write time) to publish
		var newestNupkg = GetFiles ("nupkg/*.nupkg")
			.OrderBy (f => new System.IO.FileInfo (f.FullPath).LastWriteTimeUtc)
			.LastOrDefault();

		var apiKey = EnvironmentVariable("NugetKey");

		NuGetPush (newestNupkg, new NuGetPushSettings { 
			Verbosity = NuGetVerbosity.Detailed,
			Source = "https://www.nuget.org/api/v2/package/",
			ApiKey = apiKey
		});
	});

Task ("Default")
	.IsDependentOn ("OutputVariables")
	.IsDependentOn ("DiscoverBuildDetails")
	.IsDependentOn ("PaketRestore")
	.IsDependentOn ("Build")
	.IsDependentOn ("UnitTests");
Task ("Release")
	.IsDependentOn ("OutputVariables")
	.IsDependentOn ("DiscoverBuildDetails")
	.IsDependentOn ("PaketRestore")
	.IsDependentOn ("Build")
	.IsDependentOn ("UnitTests")
	.IsDependentOn ("CoverageUpload")
	.IsDependentOn ("GenerateReleaseNotes")
	.IsDependentOn ("Nuget")
    .IsDependentOn ("Push");

RunTarget (target);

public void StartBlock(string blockName)
{
		if(runningOnTeamCity)
		{
			TeamCity.WriteStartBlock(blockName);
		}
}

public void StartBuildBlock(string blockName)
{
	if(runningOnTeamCity)
	{
		TeamCity.WriteStartBuildBlock(blockName);
	}
}

public void EndBlock(string blockName)
{
	if(runningOnTeamCity)
	{
		TeamCity.WriteEndBlock(blockName);
	}
}

public void EndBuildBlock(string blockName)
{
	if(runningOnTeamCity)
	{
		TeamCity.WriteEndBuildBlock(blockName);
	}
}

public void PushVersion(string version)
{
	if(runningOnTeamCity)
	{
		TeamCity.SetBuildNumber(version);
	}
	if(runningOnAppVeyor)
	{
		Information("Pushing version to AppVeyor: " + version);
		AppVeyor.UpdateBuildVersion(version);
	}
}

public void PushTestResults(string filePath)
{
	var file = MakeAbsolute(File(filePath));
	if(runningOnAppVeyor)
	{
		AppVeyor.UploadTestResults(file, AppVeyorTestResultsType.NUnit3);
	}
}