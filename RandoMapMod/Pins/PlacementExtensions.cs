using ItemChanger;
using RandomizerCore;
using RandomizerMod.IC;
using RandomizerMod.RC;
using System.Collections.Generic;
using System.Linq;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pins
{
    internal static class PlacementExtensions
    {
        internal static void LogDebug(this AbstractPlacement placement)
        {
            RandoMapMod.Instance.LogDebug($"- Name: {placement.Name}");
            RandoMapMod.Instance.LogDebug($"- - Visited: {placement.Visited}");
            RandoMapMod.Instance.LogDebug($"- - AllObtained: {placement.AllObtained()}");
            RandoMapMod.Instance.LogDebug($"- - Tags: {placement.tags.Count}");

            foreach (Tag tag in placement.tags)
            {
                RandoMapMod.Instance.LogDebug($"- - - String: {tag}");
            }

            RandoMapMod.Instance.LogDebug($"- - Items: {placement.Items.Count}");

            foreach (AbstractItem item in placement.Items)
            {
                RandoMapMod.Instance.LogDebug($"- - - Name: {item.name}");
                RandoMapMod.Instance.LogDebug($"- - - IsObtained: {item.IsObtained()}");
                RandoMapMod.Instance.LogDebug($"- - - WasEverObtained: {item.WasEverObtained()}");
                RandoMapMod.Instance.LogDebug($"- - - PreviewName: {item.GetPreviewName()}");
                RandoMapMod.Instance.LogDebug($"- - - Tags: {item.tags.Count}");

                foreach (Tag tag in item.tags)
                {
                    RandoMapMod.Instance.LogDebug($"- - - - String: {tag}");
                }
            }
        }

        internal static void LogDebug(this GeneralizedPlacement placement)
        {
            RandoMapMod.Instance.LogDebug($"- Location: {placement.Location.Name}");
            RandoMapMod.Instance.LogDebug($"- - Item: {placement.Item.Name}");
        }

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
                    && mprt.previewTexts != null)
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
