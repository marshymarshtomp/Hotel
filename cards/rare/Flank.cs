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

internal sealed class FlankCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Flank", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Flank.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 1 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [
                new AStatus()
                {
                    status = ModEntry.Instance.KokoroApi.TempStrafeStatus.Status,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 2
                },
                new AEndTurn()
            ],
            Upgrade.B => [
                new AStatus()
                {
                    status = ModEntry.Instance.KokoroApi.TempStrafeStatus.Status,
                    targetPlayer = true,
                    statusAmount = 2
                },
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AEndTurn()
            ],
            _ => [
                new AStatus()
                {
                    status = ModEntry.Instance.KokoroApi.TempStrafeStatus.Status,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AStatus()
                {
                    status = Status.autododgeRight,
                    targetPlayer = true,
                    statusAmount = 1
                },
                new AEndTurn()
            ],

        };
}
