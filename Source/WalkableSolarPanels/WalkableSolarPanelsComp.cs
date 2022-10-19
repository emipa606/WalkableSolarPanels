using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace WalkableSolarPanels;

public class WalkableSolarPanelsComp : CompPowerPlant
{
    private const float MaxCapacity = 100f;

    private int CachedTotalOutput;

    private int LastUpdateTick;

    private IntVec3 parentPosition;

    private ThingDef solarPanelDef;

    private float PowerOutputD => Mathf.Lerp(0f, MaxCapacity, parent.Map.skyManager.CurSkyGlow);

    protected override float DesiredPowerOutput => PowerOutputD * RoofedPowerOutputFactor;

    private float RoofedPowerOutputFactor
    {
        get
        {
            var num = 0;
            var num2 = 0;
            foreach (var current in parent.OccupiedRect())
            {
                num++;
                if (parent.Map.roofGrid.Roofed(current))
                {
                    num2++;
                }
            }

            return (num - num2) / (float)num;
        }
    }

    public override void PostDraw()
    {
        var powerOutputD = DesiredPowerOutput;

        base.PostDraw();
        var powerColor = Color.gray;
        if (powerOutputD > 0)
        {
            powerColor = Color.red;
        }

        if (powerOutputD > MaxCapacity * 0.2)
        {
            powerColor = new Color(1f, 0.5f, 0.0156862754f, 1f);
        }

        if (powerOutputD > MaxCapacity * 0.4)
        {
            powerColor = Color.yellow;
        }

        if (powerOutputD > MaxCapacity * 0.6)
        {
            powerColor = Color.cyan;
        }

        if (powerOutputD > MaxCapacity * 0.8)
        {
            powerColor = Color.green;
        }

        DrawCircle(parent.DrawPos, new Vector2(0.08f, 0.08f),
            SolidColorMaterials.SimpleSolidColorMaterial(powerColor));
    }

    public override string CompInspectStringExtra()
    {
        var returnString = base.CompInspectStringExtra();
        if (LastUpdateTick != 0 && GenTicks.TicksGame < LastUpdateTick + 400)
        {
            return $"{returnString}\n{"WSP.totalpower".Translate(CachedTotalOutput)}";
        }

        if (solarPanelDef == null)
        {
            solarPanelDef = DefDatabase<ThingDef>.GetNamedSilentFail("WalkableSolarPanel");
        }

        var allPanels = parent.Map.listerBuildings.AllBuildingsColonistOfDef(solarPanelDef);
        var returnValue = 0f;
        foreach (var building in allPanels)
        {
            returnValue += building.GetComp<WalkableSolarPanelsComp>().DesiredPowerOutput;
        }

        CachedTotalOutput = (int)Math.Round(returnValue);
        LastUpdateTick = GenTicks.TicksGame;
        return $"{returnString}\n{"WSP.totalpower".Translate(CachedTotalOutput)}";
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        parentPosition = parent.Position;

        if (parent.Position.GetTerrain(parent.Map) == TerrainDefOf.PavedTile)
        {
            return;
        }

        if (parent.Map.terrainGrid.CanRemoveTopLayerAt(parentPosition))
        {
            parent.Map.terrainGrid.RemoveTopLayer(parentPosition);
        }

        parent.Map.terrainGrid.SetTerrain(parent.Position, TerrainDefOf.PavedTile);
    }


    public override void PostDeSpawn(Map map)
    {
        if (parentPosition.GetTerrain(map) == TerrainDefOf.PavedTile)
        {
            map.terrainGrid.RemoveTopLayer(parentPosition, false);
        }

        base.PostDeSpawn(map);
    }

    private static void DrawCircle(Vector3 center, Vector2 size, Material colorMaterial)
    {
        var matrix = default(Matrix4x4);
        var lineWidth = 0.02f;
        var frameHorizontalSize = new Vector3(size.x / 2, 1f, size.y);
        var frameVerticalSize = new Vector3(size.x, 1f, size.y / 2);
        var framePosition = center + (Vector3.up * 0.01f);
        var frameMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.black);
        var colorHorizontalSize = new Vector3(size.x - lineWidth, 1f, (size.y - lineWidth) / 2);
        var colorVerticalSize = new Vector3((size.x - lineWidth) / 2, 1f, size.y - lineWidth);
        var colorPosition = center + (Vector3.up * 0.02f);

        matrix.SetTRS(framePosition, new Quaternion(), frameHorizontalSize);
        Graphics.DrawMesh(MeshPool.plane10, matrix, frameMaterial, 0);
        matrix.SetTRS(framePosition, new Quaternion(), frameVerticalSize);
        Graphics.DrawMesh(MeshPool.plane10, matrix, frameMaterial, 0);
        matrix.SetTRS(colorPosition, new Quaternion(), colorHorizontalSize);
        Graphics.DrawMesh(MeshPool.plane10, matrix, colorMaterial, 0);
        matrix.SetTRS(colorPosition, new Quaternion(), colorVerticalSize);
        Graphics.DrawMesh(MeshPool.plane10, matrix, colorMaterial, 0);
    }
}