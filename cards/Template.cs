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
using Hotel.ExternalAPIs;

namespace Hotel.cards;

internal sealed class TemplateCard : Card, IRegisterable, IHasCustomCardTraits
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
    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade switch
    {
        Upgrade.A => new HashSet<ICardTraitEntry>(),
        Upgrade.B => new HashSet<ICardTraitEntry>(),
        _ => new HashSet<ICardTraitEntry>() { }
    };

    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.A => new() { },
            Upgrade.B => new() { },
            _ => new() { },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [

            ],
            Upgrade.B => [

            ],
            _ => [
            ],

        };
}
