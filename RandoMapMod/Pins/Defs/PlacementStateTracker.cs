using ItemChanger;

namespace RandoMapMod.Pins;

internal class PlacementStateTracker
{
    private readonly AbstractPlacement _placement;
    private readonly ItemStateTracker[] _itemStateTrackers;

    internal PlacementStateTracker(AbstractPlacement placement)
    {
        _placement = placement;
        _itemStateTrackers = [.. placement.Items.Select(i => new ItemStateTracker(i, this))];
        Update();
    }

    internal PlacementState State { get; private set; }

    internal void Hook()
    {
        _placement.OnVisitStateChanged += OnVisitStateChanged;
        foreach (var ist in _itemStateTrackers)
        {
            ist.Hook();
        }
    }

    internal void Unhook()
    {
        _placement.OnVisitStateChanged -= OnVisitStateChanged;
        foreach (var ist in _itemStateTrackers)
        {
            ist.Unhook();
        }
    }

    internal void UpdatePersistentItems()
    {
        if (_itemStateTrackers.Any(i => i.TryRefreshPersistentItem()))
        {
            Update();
        }
    }

    internal void Update()
    {
        if (_placement.GetPreviewableItems().Any())
        {
            State = PlacementState.Previewable;
        }
        else if (_itemStateTrackers.Any(i => i.State is ItemState.NeverObtained))
        {
            State = PlacementState.NotCleared;
        }
        else if (_itemStateTrackers.Any(i => i.State is ItemState.Refreshed))
        {
            State = PlacementState.ClearedPersistent;
        }
        else
        {
            State = PlacementState.Cleared;
        }
    }

    private void OnVisitStateChanged(VisitStateChangedEventArgs args)
    {
        Update();
    }
}

internal enum PlacementState
{
    Previewable,
    NotCleared,

    // The placement is cleared, but has persistent items that are currently obtainable
    ClearedPersistent,
    Cleared,
}

internal class ItemStateTracker
{
    private readonly AbstractItem _item;
    private readonly PlacementStateTracker _pst;
    private readonly bool _isPersistent;

    internal ItemStateTracker(AbstractItem item, PlacementStateTracker pst)
    {
        _item = item;
        _pst = pst;
        _isPersistent = item.IsPersistent();

        if (!item.WasEverObtained())
        {
            State = ItemState.NeverObtained;
        }
        else if (_isPersistent)
        {
            State = ItemState.Refreshed;
        }
        else
        {
            State = ItemState.Obtained;
        }
    }

    internal ItemState State { get; private set; }

    internal void Hook()
    {
        _item.AfterGive += AfterGive;
    }

    internal void Unhook()
    {
        _item.AfterGive -= AfterGive;
    }

    internal bool TryRefreshPersistentItem()
    {
        if (_isPersistent && !_item.IsObtained() && State is ItemState.Obtained)
        {
            State = ItemState.Refreshed;
            return true;
        }

        return false;
    }

    private void AfterGive(EventArgs args)
    {
        State = ItemState.Obtained;
        _pst.Update();
    }
}

internal enum ItemState
{
    NeverObtained,

    // The item is persistent, was previously obtained and is now obtainable again.
    // The scene needs to change first, even after IC's ObtainState is set to Refreshed.
    Refreshed,

    // The item is not persistent and permanently obtained
    Obtained,
}
