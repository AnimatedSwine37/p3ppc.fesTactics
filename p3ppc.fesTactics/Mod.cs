using p3ppc.fesTactics.Configuration;
using p3ppc.fesTactics.NuGet.templates.defaultPlus;
using p3ppc.fesTactics.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Text;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p3ppc.fesTactics;
/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private IAsmHook _startTacticSelectHook;
    private Memory _memory;

    public Mod(ModContext context)
    {
        //Debugger.Launch();
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        _memory = Memory.Instance;
        Utils.Initialise(_logger, _configuration, _modLoader);

        Utils.SigScan("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ??", "StartTacticSelect", address =>
        {
            string[] function =
            {
                "use64",
                "cmp r8, 6",
                "jl endHook", // navi is being selected (don't touch them!)
                $"mov r8, {Tactics.English.Length-1}",
                "je endHook", 
                "inc r8", // Aigis has one more tactic (Orgia)
                "label endHook",
            };
            _startTacticSelectHook = _hooks.CreateAsmHook(function, address, AsmHookBehaviour.ExecuteFirst).Activate();
        });

        Utils.SigScan("48 8B 84 ?? ?? ?? ?? ?? 4A 8B 2C ??", "TacticNamesPtr", address =>
        {
            var tacticNamesPtr = *(int*)(address + 4) + Utils.BaseAddress;
            Utils.LogDebug($"Found tactic names at 0x{tacticNamesPtr:X}");
            SetupTacticNames((nuint)tacticNamesPtr);
        });

        Utils.SigScan("49 8B 84 ?? ?? ?? ?? ?? 48 8B 44 ?? ?? F3 0F 10 0D ?? ?? ?? ??", "TacticDescriptionsPtr", address =>
        {
            // Change the number of descriptions (the second line is gotten by adding the total number)
            _memory.SafeWrite((nuint)(address + 12), new byte[] { (byte)(Tactics.English.Length * 8) });

            // Setup new descriptions
            var tacticDescriptionsPtr = *(int*)(address + 4) + Utils.BaseAddress;
            Utils.LogDebug($"Found tactic descriptions at 0x{tacticDescriptionsPtr:X}");
            SetupTacticDescriptions((nuint)tacticDescriptionsPtr);
        });
    }

    private void SetupTacticNames(nuint nameGroups)
    {
        var tactics = Tactics.English;
        char** namesPtr = (char**)_memory.Allocate((nuint)(tactics.Length * sizeof(char*))).Address;
        Utils.LogDebug($"Allocated array of English tactic names at 0x{(nuint)namesPtr:X}");
        _memory.SafeWrite(nameGroups + 8, BitConverter.GetBytes((nuint)namesPtr));

        // TODO add stuff for other languages and use the correct encodings for them
        for (int i = 0; i < tactics.Length; i++)
        {
            namesPtr[i] = Utils.WriteStr(tactics[i].Name, Encoding.UTF8);
        }
    }

    private void SetupTacticDescriptions(nuint descriptionGroups)
    {
        var tactics = Tactics.English;
        char** descriptionsPtr = (char**)_memory.Allocate((nuint)(tactics.Length * sizeof(char*))).Address;
        Utils.LogDebug($"Allocated array of English tactic descriptions at 0x{(nuint)descriptionsPtr:X}");
        _memory.SafeWrite(descriptionGroups + 8, BitConverter.GetBytes((nuint)descriptionsPtr));

        // TODO add stuff for other languages and use the correct encodings for them
        for (int i = 0; i < tactics.Length; i++)
        {
            // Descriptions are split over two lines
            descriptionsPtr[i] = Utils.WriteStr(tactics[i].Description1, Encoding.UTF8);
            descriptionsPtr[i + tactics.Length] = Utils.WriteStr(tactics[i].Description2, Encoding.UTF8);
        }
    }


    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}