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
    private static ISpriteEntry InfinitePreventionCardArt = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        InfinitePreventionCardArt = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(
            ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/cards/Angery.png"));
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
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetDataWithOverrides)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(Card_GetDataWithOverrides_Postfix)))
        );
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetActionsOverridden)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(Card_GetActionsOverridden_Postfix)))
        );
        ModEntry.Instance.Helper.Events.RegisterAfterArtifactsHook(nameof(Artifact.OnPlayerPlayCard),
            (int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount) =>
            {
                
                if (ModEntry.Instance.Helper.Content.Cards.IsCardTraitActive(state, card, ModEntry.Instance.Helper.Content.Cards.InfiniteCardTrait) && card.GetDataWithOverrides(state).cost == 0)
                {
                    if (state.ship.Get(SilenceStatus.Status) > 0)
                    {
                        if (ModEntry.Instance.Helper.ModData.TryGetModData<int>(card, "InfinitePreventionCount",
                                out var number))
                        {
                            if (number+1 >= 5) 
                            {
                                ModEntry.Instance.Helper.Content.Cards.SetCardTraitOverride(state, card, ModEntry.Instance.Helper.Content.Cards.UnplayableCardTrait, true, false);
                                
                            }

                            if (number < 5)
                            {
                                ModEntry.Instance.Helper.ModData.SetModData<int>(card, "InfinitePreventionCount", number + 1);
                            }
                        }
                        else
                        {
                            ModEntry.Instance.Helper.ModData.SetModData<int>(card, "InfinitePreventionCount", 1);
                        }
                    }
                }
            }, 0);
        ModEntry.Instance.Helper.Events.RegisterAfterArtifactsHook(nameof(Artifact.OnTurnEnd),
            (State state, Combat combat) =>
            {
                var cardList = combat.hand.Concat(state.deck).Concat(combat.discard).Concat(combat.exhausted).ToList();
                foreach (Card card in cardList)
                {
                    if (ModEntry.Instance.Helper.ModData.TryGetModData<int>(card, "InfinitePreventionCount",
                            out var number))
                    {
                        if (number >= 5)
                        {
                            ModEntry.Instance.Helper.Content.Cards.SetCardTraitOverride(state, card, ModEntry.Instance.Helper.Content.Cards.UnplayableCardTrait, false, false);
                            ModEntry.Instance.Helper.ModData.RemoveModData(card,  "InfinitePreventionCount");
                        }
                    }
                }
            }, 0);
        ModEntry.Instance.Helper.Events.RegisterAfterArtifactsHook(nameof(Artifact.OnCombatEnd),
            (State state) =>
            {
                var cardList = state.deck;
                foreach (Card card in cardList)
                {
                    if (ModEntry.Instance.Helper.ModData.TryGetModData<int>(card, "InfinitePreventionCount",
                            out var number))
                    {
                        if (number >= 5)
                        {
                            ModEntry.Instance.Helper.Content.Cards.SetCardTraitOverride(state, card, ModEntry.Instance.Helper.Content.Cards.UnplayableCardTrait, false, false);
                            ModEntry.Instance.Helper.ModData.RemoveModData(card,  "InfinitePreventionCount");
                        }
                    }
                }
            });
    }

    private static void Card_GetDataWithOverrides_Postfix(State state, Card __instance, ref CardData __result)
    {
        if (state.route is Combat c)
        {
            if (ModEntry.Instance.Helper.ModData.TryGetModData<int>(__instance, "InfinitePreventionCount",
                    out var number))
            {
                if (number >= 5)
                {
                    __result.art = InfinitePreventionCardArt.Sprite;
                }
            }
        }
    }

    private static void Card_GetActionsOverridden_Postfix(State s, Combat c, Card __instance, ref List<CardAction> __result)
    {
        if (ModEntry.Instance.Helper.ModData.TryGetModData<int>(__instance, "InfinitePreventionCount",
                out var number))
        {
            if (number >= 5)
            {
                __result.Clear();
            }
        }
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
            foreach (var wrappedAction in ModEntry.Instance.KokoroApi.WrappedActions.GetWrappedCardActionsRecursively(
                         c.currentCardAction, false))
            {
                if (wrappedAction is not AAttack && g.state.ship.Get(SilenceStatus.Status) > 0 &&
                    card is not null)
                {
                    var shouldDequeue = true;
                    foreach (var hook in ModEntry.Instance.HookManager.GetHooksWithProxies(
                                 ModEntry.Instance.Helper.Utilities.ProxyManager, g.state.EnumerateAllArtifacts()))
                    {
                        shouldDequeue = hook.IsSilencable(g.state, c, wrappedAction).HasValue
                            ? hook.IsSilencable(g.state, c, wrappedAction).Value
                            : true;
                    }

                    if (!shouldDequeue) goto End;
                    c.currentCardAction = c.cardActions.Dequeue();
                }
                else goto End;
            } 
            End:
            break;
        }

        if (c.currentCardAction != null)
        {
            var card = ModEntry.Instance.KokoroApi.ActionInfo.GetSourceCard(g.state, c.currentCardAction);
            foreach (var wrappedAction in ModEntry.Instance.KokoroApi.WrappedActions.GetWrappedCardActionsRecursively(
                         c.currentCardAction, false))
            {
                if (wrappedAction is not AAttack && g.state.ship.Get(SilenceStatus.Status) > 0 &&
                    card is not null)
                {
                    var shouldDequeue = true;
                    foreach (var hook in ModEntry.Instance.HookManager.GetHooksWithProxies(
                                 ModEntry.Instance.Helper.Utilities.ProxyManager, g.state.EnumerateAllArtifacts()))
                    {
                        shouldDequeue = hook.IsSilencable(g.state, c, wrappedAction).HasValue
                            ? hook.IsSilencable(g.state, c, wrappedAction).Value
                            : true;
                    }

                    if (shouldDequeue) c.currentCardAction = c.cardActions.Dequeue();
                }
            } 

        }
    }
    
}
