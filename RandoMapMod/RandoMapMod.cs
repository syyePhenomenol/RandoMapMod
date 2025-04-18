﻿using System.Reflection;
using MapChanger;
using MapChanger.Defs;
using Modding;
using RandoMapMod.Input;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandoMapMod.UI;
using UnityEngine;

namespace RandoMapMod;

public class RandoMapMod : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>
{
    private static readonly IEnumerable<MapMode> _modes =
    [
        new FullMapMode(),
        new AllPinsMode(),
        new PinsOverAreaMode(),
        new PinsOverRoomMode(),
        new TransitionNormalMode(),
        new TransitionVisitedOnlyMode(),
        new TransitionAllRoomsMode(),
    ];

    private static readonly IEnumerable<HookModule> _hookModules =
    [
        new RmmColors(),
        new RmmRoomManager(),
        new TransitionData(),
        new RmmPathfinder(),
        new RmmPinManager(),
        new ItemCompass(),
        new RouteCompass(),
    ];

    public RandoMapMod()
    {
        Instance = this;
    }

    public static LocalSettings LS { get; private set; } = new();
    public static GlobalSettings GS { get; private set; } = new();

    internal static Assembly Assembly => Assembly.GetExecutingAssembly();
    internal static RandoMapMod Instance { get; private set; }

    public override string GetVersion()
    {
        return "3.5.8";
    }

    public override int LoadPriority()
    {
        return 10;
    }

    public void OnLoadLocal(LocalSettings ls)
    {
        LS = ls;
    }

    public LocalSettings OnSaveLocal()
    {
        return LS;
    }

    public void OnLoadGlobal(GlobalSettings gs)
    {
        GS = gs;
    }

    public GlobalSettings OnSaveGlobal()
    {
        return GS;
    }

    public override void Initialize()
    {
        LogDebug($"Initializing");

        if (!Dependencies.HasAll())
        {
            return;
        }

        Interop.FindInteropMods();

        InputManager.AddRange(
            [
                new LocationHintInput(),
                new TogglePinClusterInput(),
                new LockGridPinInput(),
                new SelectRoomRouteInput(),
                new BenchwarpInput(),
                new ProgressHintInput(),
                new ControlPanelInput(),
                new MapKeyInput(),
                new PinPanelInput(),
                new ToggleItemCompassInput(),
                new ToggleBenchwarpPinsInput(),
                new RoomPanelInput(),
                new SelectionReticleInput(),
                new PathfinderBenchwarpInput(),
                new ProgressHintPanelInput(),
                new ToggleSpoilersInput(),
                new ToggleVanillaInput(),
                new ToggleRandomizedInput(),
                new ToggleShapeInput(),
                new ToggleSizeInput(),
                new DebugInput(),
            ]
        );

        Finder.InjectLocations(
            JsonUtil.DeserializeFromAssembly<Dictionary<string, MapLocationDef>>(
                Assembly,
                "RandoMapMod.Resources.locations.json"
            )
        );

        Events.OnEnterGame += OnEnterGame;
        Events.OnQuitToMenu += OnQuitToMenu;

        LogDebug($"Initialization complete.");
    }

    internal static void ResetToDefaultSettings()
    {
        GS = new();
    }

    private static void OnEnterGame()
    {
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
        {
            return;
        }

        MapChanger.Settings.AddModes(_modes);
        Events.OnSetGameMap += OnSetGameMap;

        if (Interop.HasBenchwarp)
        {
            BenchwarpInterop.Load();
        }

        foreach (var hookModule in _hookModules)
        {
            hookModule.OnEnterGame();
        }
    }

    private static void OnQuitToMenu()
    {
        if (!RandomizerMod.RandomizerMod.IsRandoSave)
        {
            return;
        }

        Events.OnSetGameMap -= OnSetGameMap;

        if (Interop.HasBenchwarp)
        {
            BenchwarpInterop.Unload();
        }

        foreach (var hookModule in _hookModules)
        {
            hookModule.OnQuitToMenu();
        }
    }

    private static void OnSetGameMap(GameObject goMap)
    {
        try
        {
            // Make rooms and pins
            RmmRoomManager.Make(goMap);
            RmmPinManager.Make(goMap);

            LS.Initialize();

            RmmUIBuilder uiBuilder = new();
            uiBuilder.Build();
        }
        catch (Exception e)
        {
            Instance.LogError(e);
        }
    }
}
