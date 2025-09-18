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

internal sealed class PhaseCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Phase", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Phase.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.A => new() { cost = 0 },
            _ => new() { cost = 1 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
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
                },
                new AStatus()
                {
                    status = Status.maxShield,
                    targetPlayer = true,
                    statusAmount = -1
                }
            ],
            _ => [
                new AStatus()
                {
                    status = ReflexManager.ReflexStatus.Status,
                    targetPlayer = true,
                    statusAmount = 3
                },
                new AStatus()
                {
                    status = Status.maxShield,
                    targetPlayer = true,
                    statusAmount = -1
                }
            ],

        };
}
