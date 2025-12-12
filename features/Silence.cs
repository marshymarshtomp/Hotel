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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FSPRO;
using Hotel.artifacts;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using static Hotel.ExternalAPIs.Kokoro.IKokoroApi.IV2.IStatusLogicApi.IHook;

namespace Hotel.features;

internal sealed class SilenceManager : IRegisterable
{
    internal static IStatusEntry SilenceStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        SilenceStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("Silence", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/icons/Silence.png")).Sprite,
                color = new("c1ced6"),
                isGood = false
            },
            Name = ModEntry.Instance.AnyLocs.Bind(["status", "Silence", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocs.Bind(["status", "Silence", "description"]).Localize
        });
        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook(), 0);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(AStatus), nameof(AStatus.Begin)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(AStatus_Begin_Postfix)))
        );
        ModEntry.Instance.Harmony.Patch(
            original:AccessTools.DeclaredMethod(typeof(Combat), nameof(Combat.DrainCardActions)),
            transpiler: new HarmonyMethod(AccessTools.DeclaredMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(Combat_DrainCardActions_Transpiler_AttackCheck)))
        );
    }
    private static void AStatus_Begin_Postfix(G g, State s, Combat c, AStatus __instance)
    {
        if (__instance.status == SilenceStatus.Status)
        {
            if (__instance.targetPlayer == false)
            {
                if (c.otherShip.Get(SilenceStatus.Status) < 1) return;
                foreach (var part in c.otherShip.parts)
                {
                    if (part is null) continue;
                    if (part.intent is not IntentAttack)
                    {
                        part.intent = null;
                    }
                }
                Audio.Play(Event.Status_ShieldDown);
            }
        }
    }
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        public bool HandleStatusTurnAutoStep(IHandleStatusTurnAutoStepArgs args)
        {
            if (args.Status == SilenceStatus.Status)
            {
                if (args.Timing == IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnEnd)
                {
                    if (args.Amount > 0)
                    {
                        args.Amount -= 1;
                    }
                }
            }
            return false;
        }
    }
    
    private static IEnumerable<CodeInstruction> Combat_DrainCardActions_Transpiler_AttackCheck(
        IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
    {
        try
        {
            return new SequenceBlockMatcher<CodeInstruction>(instructions)
                .Find([
                    ILMatches.Ldarg(0),
                    ILMatches.Ldfld("cardActions"),
                    ILMatches.Call("Dequeue"),
                    ILMatches.Stfld("currentCardAction"),
                ])
                .Insert(SequenceMatcherPastBoundsDirection.After,
                    SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,
                            AccessTools.DeclaredMethod(MethodBase.GetCurrentMethod()!.DeclaringType!,
                                nameof(Combat_DrainCardActions_RemoveNonAttacks)))
                    ])
                
                .AllElements();
                
        }
        catch (Exception ex)
        {
            ModEntry.Instance.Logger.LogError("Could not patch method {DeclaringType}::{Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod.DeclaringType, originalMethod, ModEntry.Instance.Package.Manifest.GetDisplayName(@long: false), ex);
            return instructions;
        }
    }

    private static void Combat_DrainCardActions_RemoveNonAttacks(Combat c, G g)
    {
        while (c.cardActions.Count > 0)
        {
            var card = ModEntry.Instance.KokoroApi.ActionInfo.GetSourceCard(g.state, c.currentCardAction);
            if (c.currentCardAction is not AAttack && g.state.ship.Get(SilenceStatus.Status) > 0 &&
                card is not null)
            {
                var shouldDequeue = true;
                foreach (var hook in ModEntry.Instance.HookManager.GetHooksWithProxies(
                             ModEntry.Instance.Helper.Utilities.ProxyManager, g.state.EnumerateAllArtifacts()))
                {
                    shouldDequeue = hook.IsSilencable(g.state, c, c.currentCardAction).HasValue
                        ? hook.IsSilencable(g.state, c, c.currentCardAction).Value
                        : true;
                }
                if (!shouldDequeue) break;
                c.currentCardAction = c.cardActions.Dequeue();
            }
            else break;
        } 
    }
}
