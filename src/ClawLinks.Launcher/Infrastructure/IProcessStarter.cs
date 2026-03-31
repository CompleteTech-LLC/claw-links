using System.Diagnostics;

namespace ClawLinks.Launcher.Infrastructure;

public interface IProcessStarter
{
    void Start(ProcessStartInfo startInfo);
}
