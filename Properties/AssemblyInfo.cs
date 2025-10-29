using System;
using System.Reflection;

[assembly: AssemblyVersion(StickFightExtendedPlayers.Plugin.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(StickFightExtendedPlayers.Plugin.PLUGIN_VERSION)]
[assembly: AssemblyInformationalVersion(StickFightExtendedPlayers.Plugin.PLUGIN_VERSION)]
[assembly: Github("Bknibb", "StickFightExtendedPlayers")]

[AttributeUsage(AttributeTargets.Assembly)]
public class GithubAttribute : Attribute
{
    public string Owner { get; }
    public string Repo { get; }

    public GithubAttribute(string owner, string repo)
    {
        Owner = owner;
        Repo = repo;
    }
}
