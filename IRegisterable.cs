using Nanoray.PluginManager;
using Nickel;

namespace Hotel;

internal interface IRegisterable
{
    static abstract void Register(IPluginPackage<IModManifest> package, IModHelper helper);
}