using ConnectionMetadataInjector;
using ItemChanger;
using RandomizerCore.Logic;
using SD = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins
{
    internal class AbstractPlacementPinDef
    {
        internal AbstractPlacement Placement { get; init; }
        internal string SceneName { get; init; }
        internal string LocationPoolGroup { get; init; }
        internal IReadOnlyCollection<string> ItemPoolGroups { get; init; }
        internal LogicDef Logic { get; init; }
        internal HintDef HintDef { get; init; }

        internal AbstractPlacementState State { get; private set; }

        internal AbstractPlacementPinDef(AbstractPlacement placement)
        {
            Placement = placement;
            placement.OnVisitStateChanged += OnVisitStateChanged;
            foreach (var item in Placement.Items)
            {
                item.OnGive += OnGive;
            }

            SceneName = placement.GetScene();

            LocationPoolGroup = SD.Of(placement).Get(InjectedProps.LocationPoolGroup);

            HashSet<string> itemPoolGroups = [];
            foreach (AbstractItem item in placement.Items)
            {
                itemPoolGroups.Add(SD.Of(item).Get(InjectedProps.ItemPoolGroup));
            }
            ItemPoolGroups = itemPoolGroups;

            Logic = SD.Of(placement).Get(InteropProperties.Logic);

            HintDef = new(SD.Of(placement).Get(InteropProperties.LocationHints));

            InitializeState();
        }

        private void OnGive(ReadOnlyGiveEventArgs args)
        {
            if (Placement.AllEverObtained())
            {
                State = AbstractPlacementState.Cleared;
                // RandoMapMod.Instance.LogDebug($"Updated state of {Placement.Name} to {AbstractPlacementState.Cleared}");
            }
        }

        // Hot fix because IC only sets the flag after invoking the hook
        private void OnVisitStateChanged(VisitStateChangedEventArgs args)
        {
            if ((args.NewFlags & VisitState.Previewed) == VisitState.Previewed
                && Placement.Items.Any(i => i.CanPreview() && !i.WasEverObtained()))
            {
                State = AbstractPlacementState.Previewable;
                // RandoMapMod.Instance.LogDebug($"Updated state of {Placement.Name} to {AbstractPlacementState.Previewable}");
            }
        }

        private void InitializeState()
        {
            if (Placement.AllEverObtained())
            {
                State = AbstractPlacementState.Cleared;
            }
            else if (Placement.GetPreviewableItems().Any())
            {
                State = AbstractPlacementState.Previewable;
            }
            else
            {
                State = AbstractPlacementState.NotCleared;
            }

            // RandoMapMod.Instance.LogDebug($"Initialized state of {Placement.Name} to {State}");
        }
    }
}