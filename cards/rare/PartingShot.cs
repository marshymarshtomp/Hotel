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

internal sealed class PartingShotCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "PartingShot", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/PartingShot.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 1, infinite = true },
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
                new AEndTurn()
            ],
            Upgrade.B => [
                new AAttack()
                {
                    damage = GetDmg(s, 2),
                    stunEnemy = true,
                    piercing = true
                },
                new AEndTurn()
            ],
            _ => [
                new AAttack()
                {
                    damage = GetDmg(s, 2),
                    stunEnemy = true
                },
                new AEndTurn()
            ],

        };
}
