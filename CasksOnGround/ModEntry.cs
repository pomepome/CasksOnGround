using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Harmony;
using System.Reflection;

namespace CasksOnGround
{
    using SVObject = StardewValley.Object;
    using Player = StardewValley.Farmer;

    public class ModEntry : Mod
    {
        private static IReflectionHelper Reflection;
        public static Multiplayer Multiplayer
        {
            get
            {
                return Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            }
        }

        public static IMonitor monitor { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Reflection = helper.Reflection;
            monitor = Monitor;
            HarmonyInstance harmony = HarmonyInstance.Create("punyo.CasksOnGround");
            MethodInfo methodBase = typeof(Cask).GetMethod("performObjectDropInAction", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo methodPatcher = typeof(CaskPatcher).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
            
            if(methodBase == null)
            {
                Monitor.Log("Original method null, what's wrong?");
                return;
            }
            if(methodPatcher == null)
            {
                Monitor.Log("Patcher null, what's wrong?");
                return;
            }
            harmony.Patch(methodBase, new HarmonyMethod(methodPatcher), null);
            Monitor.Log($"Patched {methodBase.DeclaringType.FullName}.{methodBase.Name} by {methodPatcher.DeclaringType.FullName}.{methodPatcher.Name}");
        }
    }

    public class CaskPatcher
    {
        public static bool Prefix(Cask __instance, ref Item dropIn, ref bool probe, ref Player who, ref bool __result)
        {
            __result = PerformObjectDropInAction(__instance, dropIn, probe, who);
            return false;
        }

        public static bool PerformObjectDropInAction(Cask cask, Item dropIn, bool probe, Player who)
        {
            if (dropIn != null && dropIn is SVObject && (dropIn as SVObject).bigCraftable.Value)
            {
                return false;
            }
            if (cask.heldObject.Value != null)
            {
                return false;
            }
            if (cask.Quality >= 4)
            {
                return false;
            }
            
            bool goodItem = false;
            float multiplier = 1f;
            switch (dropIn.ParentSheetIndex)
            {
                case 426:
                    goodItem = true;
                    multiplier = 4f;
                    break;
                case 424:
                    goodItem = true;
                    multiplier = 4f;
                    break;
                case 348:
                    goodItem = true;
                    multiplier = 1f;
                    break;
                case 459:
                    goodItem = true;
                    multiplier = 2f;
                    break;
                case 303:
                    goodItem = true;
                    multiplier = 1.66f;
                    break;
                case 346:
                    goodItem = true;
                    multiplier = 2f;
                    break;
            }
            if (goodItem)
            {
                cask.heldObject.Value = (dropIn.getOne() as SVObject);
                if (!probe)
                {
                    cask.agingRate.Value = multiplier;
                    cask.daysToMature.Value = 56f;
                    cask.MinutesUntilReady = 999999;
                    if (cask.heldObject.Value.Quality == 1)
                    {
                        cask.daysToMature.Value = 42f;
                    }
                    else if (cask.heldObject.Value.Quality == 2)
                    {
                        cask.daysToMature.Value = 28f;
                    }
                    else if (cask.heldObject.Value.Quality == 4)
                    {
                        cask.daysToMature.Value = 0f;
                        cask.MinutesUntilReady = 1;
                    }
                    who.currentLocation.playSound("Ship");
                    who.currentLocation.playSound("bubbles");
                    ModEntry.Multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(256, 1856, 64, 128), 80f, 6, 999999, cask.TileLocation * 64f + new Vector2(0f, -128f), false, false, (cask.TileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f, false)
                    {
                        alphaFade = 0.005f
                    });
                }
                return true;
            }
            return false;
        }
    }
}
