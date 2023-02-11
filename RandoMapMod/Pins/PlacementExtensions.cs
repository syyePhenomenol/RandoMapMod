using ItemChanger;
using RandomizerMod.IC;
using RandomizerMod.RC;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pins
{
    internal static class PlacementExtensions
    {
        // Next five helper functions are based on BadMagic100's Rando4Stats RandoExtensions
        // MIT License

        // Copyright(c) 2022 BadMagic100

        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files(the "Software"), to deal
        // in the Software without restriction, including without limitation the rights
        // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        // copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:

        // The above copyright notice and this permission notice shall be included in all
        // copies or substantial portions of the Software.
        internal static ItemPlacement RandoPlacement(this AbstractItem item)
        {
            if (item.GetTag(out RandoItemTag tag))
            {
                return RM.RS.Context.itemPlacements[tag.id];
            }
            return default;
        }

        internal static RandoModLocation RandoModLocation(this AbstractPlacement placement)
        {
            return placement.Items.First().RandoPlacement().Location;
        }

        internal static bool CanPreview(this AbstractPlacement placement)
        {
            return !placement.HasTag<ItemChanger.Tags.DisableItemPreviewTag>();
        }

        internal static bool TryGetPreviewText(this AbstractPlacement placement, out List<string> text)
        {
            text = new();

            if (placement.GetTag<ItemChanger.Tags.MultiPreviewRecordTag>() is ItemChanger.Tags.MultiPreviewRecordTag mprt
                    && mprt.previewTexts is not null)
            {
                for (int i = 0; i < mprt.previewTexts.Length; i++)
                {
                    string t = mprt.previewTexts[i];
                    if (!string.IsNullOrEmpty(t) && i < placement.Items.Count && !placement.Items[i].WasEverObtained())
                    {
                        text.Add(t);
                    }
                }

                return true;
            }
            else if (placement.GetTag<ItemChanger.Tags.PreviewRecordTag>() is ItemChanger.Tags.PreviewRecordTag prt
                    && !string.IsNullOrEmpty(prt.previewText) && !placement.Items.All(i => i.WasEverObtained()))
            {
                text.Add(prt.previewText);
            }

            return text.Any();
        }

        internal static bool IsPersistent(this AbstractPlacement placement)
        {
            return placement.Items.Any(item => item.HasTag<ItemChanger.Tags.PersistentItemTag>());
        }

        internal static bool IsPersistent(this AbstractItem item)
        {
            return item.HasTag<ItemChanger.Tags.PersistentItemTag>();
        }
    }
}
