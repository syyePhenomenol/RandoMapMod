using ItemChanger;
using static RandomizerMod.Extensions.ICExtensions;

namespace RandoMapMod.Pins;

internal static class PlacementExtensions
{
    internal static string GetScene(this AbstractPlacement placement)
    {
        return placement.RandoLocation()?.LocationDef?.SceneName ?? Finder.GetLocation(placement.Name)?.sceneName;
    }

    internal static bool CanPreview(this TaggableObject taggable)
    {
        return !taggable.HasTag<ItemChanger.Tags.DisableItemPreviewTag>();
    }

    internal static string[] GetPreviewText(this AbstractPlacement placement)
    {
        List<string> texts = [];

        if (!placement.CheckVisitedAny(VisitState.Previewed))
        {
            return [.. texts];
        }

        if (
            placement.GetTag<ItemChanger.Tags.MultiPreviewRecordTag>() is ItemChanger.Tags.MultiPreviewRecordTag mprt
            && mprt.previewTexts is not null
        )
        {
            for (var i = 0; i < mprt.previewTexts.Length; i++)
            {
                var t = mprt.previewTexts[i];
                if (!string.IsNullOrEmpty(t) && i < placement.Items.Count && !placement.Items[i].WasEverObtained())
                {
                    texts.Add(t);
                }
            }
        }
        else if (
            placement.GetTag<ItemChanger.Tags.PreviewRecordTag>() is ItemChanger.Tags.PreviewRecordTag prt
            && !string.IsNullOrEmpty(prt.previewText)
            && !placement.Items.All(i => i.WasEverObtained())
        )
        {
            texts.Add(prt.previewText);
        }

        return [.. texts];
    }

    internal static bool IsPersistent(this AbstractItem item)
    {
        return item.HasTag<ItemChanger.Tags.PersistentItemTag>();
    }

    internal static IEnumerable<AbstractItem> GetPreviewableItems(this AbstractPlacement placement)
    {
        if (placement.CanPreview() && placement.CheckVisitedAny(VisitState.Previewed))
        {
            return placement.Items.Where(i => i.CanPreview() && !i.WasEverObtained());
        }

        return [];
    }

    internal static IEnumerable<AbstractItem> GetEverObtainedItems(this AbstractPlacement placement)
    {
        return placement.Items.Where(i => i.WasEverObtained());
    }

    internal static IEnumerable<AbstractItem> GetNeverObtainedItems(this AbstractPlacement placement)
    {
        return placement.Items.Where(i => !i.WasEverObtained());
    }

    internal static IEnumerable<AbstractItem> GetNeverObtainedUnpreviewableItems(this AbstractPlacement placement)
    {
        // Exclude items that are previewable
        if (placement.CanPreview() && placement.CheckVisitedAny(VisitState.Previewed))
        {
            return placement.Items.Where(i => !i.WasEverObtained() && !i.CanPreview());
        }

        return placement.Items.Where(i => !i.WasEverObtained());
    }

    internal static IEnumerable<AbstractItem> GetObtainablePersistentItems(this AbstractPlacement placement)
    {
        return placement.Items.Where(i => i.IsPersistent() && !i.IsObtained());
    }

    internal static bool AllEverObtained(this AbstractPlacement placement)
    {
        return placement.Items.All(i => i.WasEverObtained());
    }
}
