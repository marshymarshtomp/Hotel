using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using Nickel.Common;
using System.Reflection;
using Hotel.ExternalAPIs.Kokoro;
using Hotel.ExternalAPIs;
namespace Hotel;

public sealed class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    public string Name { get; init; } = typeof(ModEntry).Namespace!;
    internal readonly IKokoroApi.IV2 KokoroApi;
    internal readonly IHarmony Harmony;
    internal readonly ILocalizationProvider<IReadOnlyList<string>> AnyLocs;
    internal readonly ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Locs;

    internal readonly ApiImplementation Api;

    internal readonly IDeckEntry SierraDeck;

    internal static readonly IReadOnlyList<Type> CommonCardTypes = [
            
        ];
    internal static readonly IReadOnlyList<Type> UncommonCardTypes = [
            
        ];
    internal static readonly IReadOnlyList<Type> RareCardTypes = [
            
        ];
    internal static readonly IEnumerable<Type> AllCardTypes =
        [
            .. CommonCardTypes,
            .. UncommonCardTypes,
            .. RareCardTypes,
        ];

    internal static readonly IReadOnlyList<Type> CommonArtifacts = [
           
        ];
    internal static readonly IReadOnlyList<Type> BossArtifacts = [
           
        ];
    internal static readonly IEnumerable<Type> AllArtifactTypes = [
            .. CommonArtifacts,
            .. BossArtifacts
        ];

    internal static readonly IEnumerable<Type> RegisterableTypes =
        [
            ..AllCardTypes,
            ..AllArtifactTypes,
           
        ];
   
    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        var sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/sierra_character_neutral_0.png")).Sprite;
        Instance = this;
        Harmony = helper.Utilities.Harmony;
        KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!.V2!;
        Api = new ApiImplementation();

        AnyLocs = new JsonLocalizationProvider(
                tokenExtractor: new SimpleLocalizationTokenExtractor(),
                localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
                );
        Locs = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
                new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocs)
            );

        SierraDeck = helper.Content.Decks.RegisterDeck("Sierra", new()
        {
            Definition = new() { color = new("ffc860"), titleColor = Colors.black },
            BorderSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/sierra_card_panel.png")).Sprite,
            Name = AnyLocs.Bind(["character", "name"]).Localize,
            DefaultCardArt = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/sierra_card_background.png")).Sprite
        });

        foreach (var type in RegisterableTypes)
        {
            AccessTools.DeclaredMethod(type, nameof(IRegisterable.Register))?.Invoke(null, [package, helper]);
        }

        helper.Content.Characters.V2.RegisterPlayableCharacter("Sierra", new()
        {
            Deck = SierraDeck.Deck,
            Description = AnyLocs.Bind(["character", "description"]).Localize,
            BorderSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/sierra_character_panel.png")).Sprite,
            NeutralAnimation = new()
            {
                CharacterType = SierraDeck.UniqueName,
                LoopTag = "neutral",
                Frames = [
                    sprite
                    ]
            },
            MiniAnimation = new()
            {
                CharacterType = SierraDeck.UniqueName,
                LoopTag = "mini",
                Frames = [helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/char/sierra_character_mini.png")).Sprite]
            },
            Starters = new()
            {
                cards = [

                    ]
            }
        });

        helper.Content.Characters.V2.RegisterCharacterAnimation(new()
        {
            CharacterType = SierraDeck.UniqueName,
            LoopTag = "gameover",
            Frames = [
                sprite
                ]
        });

        helper.Content.Characters.V2.RegisterCharacterAnimation(new()
        {
            CharacterType = SierraDeck.UniqueName,
            LoopTag = "squint",
            Frames = [
            sprite
        ]
        });

        helper.ModRegistry.AwaitApi<IMoreDifficultiesApi>(
            "TheJazMaster.MoreDifficulties",
            new SemanticVersion(1, 3, 0),
            api => api.RegisterAltStarters(
                deck: SierraDeck.Deck,
                starterDeck: new StarterDeck
                {
                    cards = [
                        ]
                }
                )
            );
    }
    public override object? GetApi(IModManifest reguestingMod) => new ApiImplementation();
}
