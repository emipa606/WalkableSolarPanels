using RimWorld;
using Verse;

namespace WalkableSolarPanels;

[DefOf]
public static class WalkableSolarPanelsDefOf
{
    public static ThingDef WalkableSolarPanel;

    static WalkableSolarPanelsDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(WalkableSolarPanelsDefOf));
    }
}