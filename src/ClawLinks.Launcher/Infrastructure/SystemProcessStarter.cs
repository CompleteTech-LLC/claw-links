using System.Diagnostics;

namespace ClawLinks.Launcher.Infrastructure;

public sealed class SystemProcessStarter : IProcessStarter
{
    public void Start(ProcessStartInfo startInfo)
    {
        Process.Start(startInfo);
    }
}
