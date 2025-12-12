using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using Nickel.Common;
using System.Reflection;
using Hotel.artifacts;
using Hotel.ExternalAPIs.Kokoro;
using Hotel.ExternalAPIs;
using Hotel.features;
using Hotel.cards;
using Hotel.cards.common;
using Hotel.cards.uncommon;
using Hotel.cards.rare;
namespace Hotel;

public sealed class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    public string Name { get; init; } = typeof(ModEntry).Namespace!;
    internal readonly IKokoroApi.IV2 KokoroApi;
    internal readonly IHarmony Harmony;
    internal readonly ILocalizationProvider<IReadOnlyList<string>> AnyLocs;
    internal readonly ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Locs;
    internal readonly HookManager<ISilenceHook> HookManager;

    internal readonly ApiImplementation Api;

    internal readonly IDeckEntry HotelDeck;

    internal static readonly IReadOnlyList<Type> CommonCardTypes = [
        typeof(SureShotCard),
        typeof(PrepareHotelCard),
        typeof(CombatProwessCard),
        typeof(BattleFocusCard),
        typeof(AllOutCard),
        typeof(ImpulseCard),
        typeof(FusilladeCard),
        typeof(AssaultBatteryCard),
        typeof(StunGunCard),
        typeof(SilenceTestCard)
        ];
    internal static readonly IReadOnlyList<Type> UncommonCardTypes = [
        typeof(AbsolutionCard),
        typeof(AmbushCard),
        typeof(CalmBeforeTheStormCard),
        typeof(ForesightCard),
        typeof(JetstreamCard),
        typeof(PhaseCard),
        typeof(PhaseCard),
        typeof(RiptideCard)
        ];
    internal static readonly IReadOnlyList<Type> RareCardTypes = [
        typeof(FlankCard),
        typeof(FocusedFireCard),
        typeof(HypersenseCard),
        typeof(PartingShotCard),
        typeof(SneakCard)
        ];
    internal static readonly IEnumerable<Type> AllCardTypes =
        [
            .. CommonCardTypes,
            .. UncommonCardTypes,
            .. RareCardTypes,
        ];

    internal static readonly IReadOnlyList<Type> CommonArtifacts = [
           typeof(Megaphone),
           typeof(QuickDrive),
           typeof(Spice),
           typeof(Kunai),
           typeof(SecretCompartment)
        ];
    internal static readonly IReadOnlyList<Type> BossArtifacts = [
           typeof(Contract),
           typeof(Silencer),
           typeof(Stimulants)
        ];
    internal static readonly IEnumerable<Type> AllArtifactTypes = [
            .. CommonArtifacts,
            .. BossArtifacts
        ];

    internal static readonly IEnumerable<Type> RegisterableTypes =
        [
            ..AllCardTypes,
            ..AllArtifactTypes,
            typeof(SilenceManager),
            typeof(ReflexManager),
            typeof(AFocusedFireAttack)
        ];
   
    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        var sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/Hotel_Neutral_0.png")).Sprite;
        Instance = this;
        Harmony = helper.Utilities.Harmony;
        KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!.V2!;
        Api = new ApiImplementation();
        HookManager = new(package.Manifest.UniqueName);

        AnyLocs = new JsonLocalizationProvider(
                tokenExtractor: new SimpleLocalizationTokenExtractor(),
                localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
                );
        Locs = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
                new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocs)
            );

        HotelDeck = helper.Content.Decks.RegisterDeck("Hotel", new()
        {
            Definition = new() { color = new("d2cdc1"), titleColor = Colors.black },
            BorderSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/HotelCard.png")).Sprite,
            Name = AnyLocs.Bind(["character", "name"]).Localize,
            DefaultCardArt = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/cards/Default_Card.png")).Sprite
        });

        foreach (var type in RegisterableTypes)
        {
            AccessTools.DeclaredMethod(type, nameof(IRegisterable.Register))?.Invoke(null, [package, helper]);
        }

        helper.Content.Characters.V2.RegisterPlayableCharacter("Hotel", new()
        {
            Deck = HotelDeck.Deck,
            Description = AnyLocs.Bind(["character", "description"]).Localize,
            BorderSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/HotelPanel.png")).Sprite,
            NeutralAnimation = new()
            {
                CharacterType = HotelDeck.UniqueName,
                LoopTag = "neutral",
                Frames = [
                    sprite
                    ]
            },
            MiniAnimation = new()
            {
                CharacterType = HotelDeck.UniqueName,
                LoopTag = "mini",
                Frames = [helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/Mini_Hotel.png")).Sprite]
            },
            Starters = new()
            {
                cards = [
                    new SureShotCard(),
                    new PrepareHotelCard()
                    ]
            }
        });

        helper.Content.Characters.V2.RegisterCharacterAnimation(new()
        {
            CharacterType = HotelDeck.UniqueName,
            LoopTag = "gameover",
            Frames = [
                helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/Hotel_Ded_0.png")).Sprite
                ]
        });

        helper.Content.Characters.V2.RegisterCharacterAnimation(new()
        {
            CharacterType = HotelDeck.UniqueName,
            LoopTag = "squint",
            Frames = [
            sprite
        ]
        });
        
        helper.ModRegistry.AwaitApi<IMoreDifficultiesApi>(
            "TheJazMaster.MoreDifficulties",
            new SemanticVersion(1, 3, 0),
            api => api.RegisterAltStarters(
                deck: HotelDeck.Deck,
                starterDeck: new StarterDeck
                {
                    cards = [
                        new BattleFocusCard(),
                        new AllOutCard()
                    ]
                }
            )
        );
    }
    public override object? GetApi(IModManifest reguestingMod) => new ApiImplementation();
}
