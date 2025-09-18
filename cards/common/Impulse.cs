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

internal sealed class ImpulseCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Impulse", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Impulse.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 3 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 2
                },
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 2
                }
            ],
            Upgrade.B => [
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 3
                }
            ],
            _ => [
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 2
                }
            ],

        };
}
