using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hotel.features;
using Hotel;
using Microsoft.Extensions.Logging;
using HarmonyLib;
using System.Net.WebSockets;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Hotel.cards.rare;

internal sealed class FocusedFireCard : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(MethodBase.GetCurrentMethod()!.DeclaringType!.Name, new()
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new()
            {
                deck = ModEntry.Instance.HotelDeck.Deck,
                rarity = Rarity.rare,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "FocusedFire", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/FocusedFire.png")).Sprite,
            
        });
        ModEntry.Instance.Api.RegisterHook(new SilenceHook(), 0);
    }

    private sealed class SilenceHook : ISilenceHook
    {
        public bool? IsSilencable(State state, Combat combat, CardAction cardAction)
        {
            if (cardAction is AFocusedFireAttack) return false;
            return null;
        }
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 1,
            description = $"Attack for {GetDmg(state, upgrade == Upgrade.B ? 1 : 0)} damage. + 1 for every card with an attack in hand.",
            retain = upgrade == Upgrade.A ? true : false
        };
    }
    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.A => [
                new AFocusedFireAttack()
                {
                    card = this
                }
            ],
            Upgrade.B => [
                new AFocusedFireAttack()
                {
                    card = this
                }
            ],
            _ => [
                new AFocusedFireAttack()
                {
                    card = this
                }
            ],
        };
    }
}
public class AFocusedFireAttack : CardAction
{
    public Card card;
    private int HandAttackCount(State s)
    {
        int damage = 0;
        if (s.route is Combat c)
        {
            foreach (Card card in c.hand)
            {
                if (card.GetActionsOverridden(s, c).Any(a => a is AAttack)) damage++;
            }
        }
        return damage;
    }
    public override void Begin(G g, State s, Combat c)
    {
        c.QueueImmediate(new AAttack()
        {
            damage = Card.GetActualDamage(s, HandAttackCount(s), false, card),
            targetPlayer = false
        });
    }
}
