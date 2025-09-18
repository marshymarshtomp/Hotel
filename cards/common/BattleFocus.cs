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

internal sealed class BattleFocusCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "BattleFocus", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/BattleFocus.png")).Sprite
        });
    }

    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 0 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [
                new ADrawCard()
                {
                    count = 5
                },
                new AStatus()
                {
                    status = SilenceManager.SilenceStatus.Status,
                    targetPlayer = true,
                    statusAmount = 1
                }
            ],
            Upgrade.B => [
                new ADrawCard()
                {
                    count = 9
                },
                new AStatus()
                {
                    status = SilenceManager.SilenceStatus.Status,
                    targetPlayer = true,
                    statusAmount = 2
                }
            ],
            _ => [
                new ADrawCard()
                {
                    count = 3
                },
                new AStatus()
                {
                    status = SilenceManager.SilenceStatus.Status,
                    targetPlayer = true,
                    statusAmount = 1
                }
            ],

        };
}
