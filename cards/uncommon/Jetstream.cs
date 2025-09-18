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

namespace Hotel.cards.uncommon;

internal sealed class JetstreamCard : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(MethodBase.GetCurrentMethod()!.DeclaringType!.Name, new()
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new()
            {
                deck = ModEntry.Instance.HotelDeck.Deck,
                rarity = Rarity.uncommon,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Jetstream", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Jetstream.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.B => new() { cost = 3, exhaust = true, retain = true },
            Upgrade.A => new() { cost = 2, exhaust = true },
            _ => new() { cost = 3, exhaust = true },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            _ => [
                new AStatus()
                {
                    targetPlayer = true,
                    status = Status.autododgeLeft,
                    statusAmount = 1
                }
            ],
        };
}
