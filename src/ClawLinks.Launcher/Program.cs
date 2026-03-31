using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Cli;
using ClawLinks.Launcher.Infrastructure;

var pathResolver = new DefaultPathResolver();
var browserBundleBootstrapper = new FileSystemBrowserBundleBootstrapper();
var profileBootstrapper = new FileSystemProfileBootstrapper();
var diagnosticsLogger = new FileDiagnosticsLogger();
var browserLauncher = new ProcessBrowserLauncher();
var service = new LinkOpenService(pathResolver, browserBundleBootstrapper, profileBootstrapper, diagnosticsLogger, browserLauncher);

return await CliRunner.RunAsync(args, service, Console.Out, Console.Error, CancellationToken.None);
