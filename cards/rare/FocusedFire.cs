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

internal sealed class FocusedFireCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "FocusedFire", "name"]).Localize
            //Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/FocusedFire.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            Upgrade.A => new() { },
            Upgrade.B => new() { },
            _ => new() { },
        };
    public override List<CardAction> GetActions(State s, Combat c)
    {
        var a = 0;
        foreach (var card in c.hand)
        {
            if (card.GetActionsOverridden(s, c).Any(x => x is AAttack))
            {
                a++;
            }
        }
        return upgrade switch
        {
            Upgrade.A => [
                new AAttack()
                {
                    damage = GetDmg(s, 0+a)
                }
            ],
            Upgrade.B => [

            ],
            _ => [

            ],

        };
    }

}
