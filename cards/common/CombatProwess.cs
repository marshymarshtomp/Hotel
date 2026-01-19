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

namespace Hotel.cards.common;

internal sealed class CombatProwessCard : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(MethodBase.GetCurrentMethod()!.DeclaringType!.Name, new()
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new()
            {
                deck = ModEntry.Instance.HotelDeck.Deck,
                rarity = Rarity.common,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "CombatProwess", "name"]).Localize
        });
    }

    

    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 2, floppable = true, art = flipped ? StableSpr.cards_Adaptability_Bottom : StableSpr.cards_Adaptability_Top },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [
                new AAttack()
                {
                    targetPlayer = false,
                    damage = GetDmg(s, 5),
                    disabled = flipped
                },
                new ADummyAction(),
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 2,
                    disabled = !flipped
                }
            ],
            Upgrade.B => [
                new AAttack()
                {
                    targetPlayer = false,
                    damage = GetDmg(s, 3),
                    disabled = flipped,
                    piercing = true
                },
                new ADummyAction(),
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 2,
                    disabled = !flipped
                }
            ],
            _ => [
                new AAttack()
                {
                    targetPlayer = false,
                    damage = GetDmg(s, 3),
                    disabled = flipped
                },
                new ADummyAction(),
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 1,
                    disabled = !flipped
                }
            ],

        };
}
