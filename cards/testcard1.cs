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

namespace Hotel.cards;

internal sealed class SilenceTestCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "TEMPLATE", "name"]).Localize
        });
    }

    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 0, infinite = true},
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            _ => [
                new AAttack()
                {
                    damage = GetDmg(s, 0),
                }
            ],

        };
}
