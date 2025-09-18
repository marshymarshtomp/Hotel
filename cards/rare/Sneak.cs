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

internal sealed class SneakCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Sneak", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Sneak.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.A => new() { cost = 1 },
            Upgrade.B => new() { cost = 2, retain = true },
            _ => new() { cost = 2 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            _ => [
                new AStatus()
                {
                    status = Status.autododgeLeft,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AStatus()
                {
                    status = SilenceManager.SilenceStatus.Status,
                    targetPlayer = true,
                    statusAmount = 2
                }
            ],

        };
}
