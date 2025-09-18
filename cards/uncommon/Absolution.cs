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

internal sealed class AbsolutionCard : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocs.Bind(["card", "Absolution", "name"]).Localize,
            Art = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Absolution.png")).Sprite
        });
    }
    public override CardData GetData(State state)
        => upgrade switch
        {
            _ => new() { cost = 2 },
        };
    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A => [
                new AAttack()
                {
                    damage = GetDmg(s, 7)
                },
                new AStatus()
                {
                    status = Status.shield,
                    targetPlayer = true,
                    statusAmount = 0,
                    mode = AStatusMode.Set
                },
            ],
            Upgrade.B => [
                new AAttack()
                {
                    damage = GetDmg(s, 8)
                },
                new AStatus()
                {
                    status = Status.shield,
                    targetPlayer = true,
                    statusAmount = 0,
                    mode = AStatusMode.Set
                },
                new AStatus()
                {
                    status = Status.evade,
                    targetPlayer = true,
                    statusAmount = 0,
                    mode = AStatusMode.Set
                }
            ],
            _ => [
                new AAttack()
                {
                    damage = GetDmg(s, 6)
                },
                new AStatus()
                {
                    status = Status.shield,
                    targetPlayer = true,
                    statusAmount = 0,
                    mode = AStatusMode.Set
                },
                new AStatus()
                {
                    status = Status.evade,
                    targetPlayer = true,
                    statusAmount = 0,
                    mode = AStatusMode.Set
                }
            ],

        };
}
