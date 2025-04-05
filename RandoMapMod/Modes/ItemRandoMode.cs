﻿using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Modes;

internal class ItemRandoMode : RmmMapMode
{
    public override Vector4? RoomColorOverride(RoomSprite roomSprite)
    {
        return GetCustomColor(roomSprite.Rsd.ColorSetting);
    }

    public override bool DisableAreaNames => !RandoMapMod.GS.ShowAreaNames;

    public override Vector4? AreaNameColorOverride(AreaName areaName)
    {
        return GetCustomColor(areaName.MiscObjectDef.ColorSetting);
    }

    public override bool? NextAreaNameActiveOverride(NextAreaName nextAreaName)
    {
        return RandoMapMod.GS.ShowNextAreas switch
        {
            NextAreaSetting.Off or NextAreaSetting.Arrows => false,
            NextAreaSetting.Full or _ => null,
        };
    }

    public override bool? NextAreaArrowActiveOverride(NextAreaArrow nextAreaArrow)
    {
        return RandoMapMod.GS.ShowNextAreas switch
        {
            NextAreaSetting.Off => false,
            NextAreaSetting.Arrows or NextAreaSetting.Full or _ => null,
        };
    }

    public override Vector4? NextAreaColorOverride(MiscObjectDef miscObjectDef)
    {
        return GetCustomColor(miscObjectDef.ColorSetting);
    }

    private Vector4? GetCustomColor(ColorSetting colorSetting)
    {
        var customColor = RmmColors.GetColor(colorSetting);

        if (!customColor.Equals(Vector4.negativeInfinity))
        {
            return customColor.ToOpaque();
        }

        return null;
    }

    public override Vector4? QuickMapTitleColorOverride(QuickMapTitle qmt)
    {
        var customColor = RmmColors.GetColorFromMapZone(Finder.GetCurrentMapZone());

        if (!customColor.Equals(Vector4.negativeInfinity))
        {
            return customColor.ToOpaque();
        }

        return null;
    }
}

internal class FullMapMode : ItemRandoMode
{
    public override string Mod => nameof(RandoMapMod);
    public override string ModeName => RmmMode.Full_Map.ToString().ToCleanName();

    public override bool? NextAreaNameActiveOverride(NextAreaName nextAreaName)
    {
        return RandoMapMod.GS.ShowNextAreas switch
        {
            NextAreaSetting.Off or NextAreaSetting.Arrows => false,
            NextAreaSetting.Full or _ => true,
        };
    }

    public override bool? NextAreaArrowActiveOverride(NextAreaArrow nextAreaArrow)
    {
        return RandoMapMod.GS.ShowNextAreas switch
        {
            NextAreaSetting.Off => false,
            NextAreaSetting.Arrows or NextAreaSetting.Full or _ => true,
        };
    }
}

internal class AllPinsMode : ItemRandoMode
{
    public override string Mod => nameof(RandoMapMod);
    public override string ModeName => RmmMode.All_Pins.ToString().ToCleanName();
    public override bool FullMap => false;
}

internal class PinsOverAreaMode : ItemRandoMode
{
    public override string Mod => nameof(RandoMapMod);
    public override string ModeName => RmmMode.Pins_Over_Area.ToString().ToCleanName();
    public override bool FullMap => false;
}

internal class PinsOverRoomMode : ItemRandoMode
{
    public override string Mod => nameof(RandoMapMod);
    public override string ModeName => RmmMode.Pins_Over_Room.ToString().ToCleanName();
    public override bool FullMap => false;
}
