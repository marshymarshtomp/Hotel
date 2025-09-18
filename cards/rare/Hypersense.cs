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

namespace Hotel.cards.rare;

internal sealed class HypersenseCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Hypersense", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Hypersense.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.A => new() { cost = 2, exhaust = true },
            _ => new() { cost = 3, exhaust = true },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.B => [
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 99
                },
                new AStatus()
                {
                    status = Status.maxShield,
                    targetPlayer = true,
                    statusAmount = -2
                }
            ],
            _ => [
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 99
                },
                new AStatus()
                {
                    status = Status.maxShield,
                    targetPlayer = true,
                    statusAmount = -4
                }
            ],

        };
}
