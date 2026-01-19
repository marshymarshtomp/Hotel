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

namespace Hotel.cards;

internal sealed class FusilladeCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Fusillade", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Fusillade.png")).Sprite
        });
    }


    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.A => new() { cost = 1, infinite = true, retain = true },
            _ => new() { cost = 1, infinite = true },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.B => [
                new AAttack()
                {
                    damage = GetDmg(s, 1),
                    fast = true
                },
                new AAttack()
                {
                    damage = GetDmg(s, 1),
                    fast = true
                }
            ],
            _ => [
                new AAttack()
                {
                    damage = GetDmg(s, 1),
                    fast = true
                }
            ],

        };
}
