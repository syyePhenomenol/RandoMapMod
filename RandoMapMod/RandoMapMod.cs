using System;
using System.Collections.Generic;
using System.Reflection;
using MapChanger;
using MapChanger.Defs;
using MapChanger.UI;
using Modding;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandoMapMod.UI;
using UnityEngine;

namespace RandoMapMod
{
    public class RandoMapMod : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>
    {
        internal const string MOD = "RandoMapMod";
        internal static Assembly Assembly => Assembly.GetExecutingAssembly();

        private static readonly string[] dependencies = new string[]
        {
            "MapChangerMod",
            "Randomizer 4",
            "CMICore",
        };

        private static readonly MapMode[] modes = new MapMode[]
        {
            new FullMapMode(),
            new AllPinsMode(),
            new PinsOverMapMode(),
            new TransitionNormalMode(),
            new TransitionVisitedOnlyMode(),
            new TransitionAllRoomsMode()
        };

        private static readonly Title title = new RmmTitle();

        private static readonly MainButton[] mainButtons = new MainButton[]
        {
            new ModEnabledButton(),
            new ModeButton(),
            new PinSizeButton(),
            new PinStyleButton(),
            new RandomizedButton(),
            new VanillaButton(),
            new SpoilersButton(),
            new PoolsPanelButton(),
            new ClearedButton(),
            new PersistentButton(),
            new GroupByButton()
        };

        private static readonly ExtraButtonPanel[] extraButtonPanels = new ExtraButtonPanel[]
        {
            new PoolsPanel()
        };

        private static readonly MapUILayer[] mapUILayers = new MapUILayer[]
        {
            new Hotkeys(),
            new ControlPanel(),
            new MapKey(),
            new SelectionPanels(),
            new RmmBottomRowText(),
            new RouteSummaryText(),
            new RouteText(),
            new QuickMapTransitions()
        };

        private static readonly List<HookModule> hookModules = new()
        {
            new RmmColors(),
            new TransitionData(),
            new Pathfinder(),
            new RmmPinManager(),
            new TransitionTracker(),
            new RouteTracker()
        };

        internal static RandoMapMod Instance;

        public RandoMapMod()
        {
            Instance = this;
        }

        public override string GetVersion() => "3.0.4";

        public override int LoadPriority() => 10;

        public static LocalSettings LS = new();

        public void OnLoadLocal(LocalSettings ls) => LS = ls;

        public LocalSettings OnSaveLocal() => LS;

        public static GlobalSettings GS = new();

        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;

        public GlobalSettings OnSaveGlobal() => GS;

        public override void Initialize()
        {
            LogDebug($"Initializing");

            foreach (string dependency in dependencies)
            {
                if (ModHooks.GetMod(dependency) is not Mod)
                {
                    MapChangerMod.Instance.LogWarn($"Dependency not found for {GetType().Name}: {dependency}");
                    return;
                }
            }

            Interop.FindInteropMods();
            RmmRoomManager.Load();
            RmmPinManager.Load();
            Pathfinder.Load();
            RouteCompass.Load();
            Finder.InjectLocations(JsonUtil.DeserializeFromAssembly<Dictionary<string, MapLocationDef>>(Assembly, "RandoMapMod.Resources.locations.json"));

            Events.OnEnterGame += OnEnterGame;
            Events.OnQuitToMenu += OnQuitToMenu;

            LogDebug($"Initialization complete.");
        }

        private static void OnEnterGame()
        {
            if (!RandomizerMod.RandomizerMod.IsRandoSave) return;

            MapChanger.Settings.AddModes(modes);
            Events.OnSetGameMap += OnSetGameMap;

            if (Interop.HasBenchwarp())
            {
                BenchwarpInterop.Load();
            }

            foreach (HookModule hookModule in hookModules)
            {
                hookModule.OnEnterGame();
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

                // Construct pause menu
                title.Make();

                foreach (MainButton button in mainButtons)
                {
                    button.Make();
                }

                foreach (ExtraButtonPanel ebp in extraButtonPanels)
                {
                    ebp.Make();
                }

                // Construct map UI
                foreach (MapUILayer uiLayer in mapUILayers)
                {
                    MapUILayerUpdater.Add(uiLayer);
                }
            }
            catch (Exception e)
            {
                Instance.LogError(e);
            }
        }

        private static void OnQuitToMenu()
        {
            Events.OnSetGameMap -= OnSetGameMap;

            foreach (HookModule hookModule in hookModules)
            {
                hookModule.OnQuitToMenu();
            }
        }
    }
}