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

internal sealed class AllOutCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "AllOut", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/All-Out.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.B => new() { cost = 0 },
            _ => new() { cost = 1 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [
                new AAttack()
                {
                    damage = GetDmg(s, 3),
                    stunEnemy = true
                },
                new AStatus()
                {
                    status = Status.maxShield,
                    targetPlayer = true,
                    statusAmount = -1
                }
            ],
            Upgrade.B => [
                new AAttack()
                {
                    damage = GetDmg(s, 2),
                    stunEnemy = true
                },
                new AStatus()
                {
                    status = Status.maxShield,
                    targetPlayer = true,
                    statusAmount = -2
                }
            ],
            _ => [
                new AAttack()
                {
                    damage = GetDmg(s, 2),
                    stunEnemy = true
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
