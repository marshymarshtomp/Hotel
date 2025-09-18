using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Hotel.ExternalAPIs.Kokoro;
using Nickel;
using Nanoray.PluginManager;
using System.Reflection;
using static Hotel.ExternalAPIs.Kokoro.IKokoroApi.IV2.IStatusLogicApi.IHook;

namespace Hotel.features;

internal sealed class ReflexManager : IRegisterable
{
    internal static IStatusEntry ReflexStatus { get; private set; } = null!;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        ReflexStatus = helper.Content.Statuses.RegisterStatus("Reflex", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/icons/Reflex.png")).Sprite,
                color = new("a71ae6"),
                isGood = true
            },
            Name = ModEntry.Instance.AnyLocs.Bind(["status", "Reflex", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocs.Bind(["status", "Reflex", "description"]).Localize
        });
        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook());
    }
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        public bool HandleStatusTurnAutoStep(IHandleStatusTurnAutoStepArgs args)
        {
            bool targetPlayer = args.Ship == args.Combat.otherShip ? false : true;
            if (args.Status == ReflexStatus.Status)
            {
                if (args.Timing == IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
                {
                    if (args.Amount > 0)
                    {
                        args.Combat.QueueImmediate(new AStatus()
                        {
                            status = Status.autododgeRight,
                            targetPlayer = targetPlayer,
                            statusAmount = 1
                        });
                        args.Amount -= 1;
                    }
                }
            }
            return false;
        }
    }
}
