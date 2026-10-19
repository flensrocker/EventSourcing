#!/usr/bin/env dotnet
#:sdk Cake.Sdk

var solution = "./Flensrocker.EventSourcing.slnx";

var target = Argument("target", "Install");
var configuration = Argument("configuration", "Release");

var pnpm = new CommandSettings { ToolName = "pnpm", ToolExecutableNames = ["pnpm"] };

Task("Install-Tools")
  .WithCriteria(c =>
    HasArgument("install")
    || target.Equals("install", StringComparison.OrdinalIgnoreCase)
    || target.Equals("format", StringComparison.OrdinalIgnoreCase)
  )
  .Does(() =>
  {
    DotNetToolRestore();
  });

Task("Install")
  .WithCriteria(c => HasArgument("install") || target.Equals("install", StringComparison.OrdinalIgnoreCase))
  .IsDependentOn("Install-Tools")
  .Does(() =>
  {
    DotNetRestore(solution, new DotNetRestoreSettings { Force = true });
    Command(pnpm, new ProcessArgumentBuilder().Append("install").Append("--frozen-lockfile"));
  });

Task("Clean")
  .WithCriteria(c => HasArgument("rebuild") || target.Equals("clean", StringComparison.OrdinalIgnoreCase))
  .Does(() =>
  {
    DotNetClean(solution, new DotNetCleanSettings { Configuration = configuration });
  });

Task("Build")
  .IsDependentOn("Install")
  .IsDependentOn("Clean")
  .Does(() =>
  {
    DotNetBuild(solution, new DotNetBuildSettings { Configuration = configuration });
    Command(pnpm, new ProcessArgumentBuilder().Append("build"));
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() =>
  {
    DotNetTest(solution, new DotNetTestSettings { Configuration = configuration, NoBuild = true });
    Command(pnpm, new ProcessArgumentBuilder().Append("test"));
  });

Task("Format")
  .IsDependentOn("Install-Tools")
  .Does(() =>
  {
    Command(["dotnet", "dotnet.exe"], "csharpier format .");
    Command(pnpm, new ProcessArgumentBuilder().Append("format"));
  });

RunTarget(target);
