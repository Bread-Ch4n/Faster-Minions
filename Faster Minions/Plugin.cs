using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using IAmFuture.Gameplay.Buildings;
using IAmFuture.Minions;
using Pathfinding;

namespace Faster_Minions;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static ManualLogSource _logger;

    private static ConfigEntry<int> _speedMultiplier;
    private static ConfigEntry<bool> _infiniteBattery;

    [HarmonyPatch(typeof(MinionMover))]
    class FasterMinionPatch
    {
        [HarmonyPatch(typeof(MinionMover),"MoveSpeed", MethodType.Setter)]
        static bool Prefix(
            MinionMover __instance, float value)
        {
            RichAI agent = (RichAI)AccessTools.Field(typeof(MinionMover), "agent").GetValue(__instance);

            agent.maxSpeed = value * _speedMultiplier.Value;
            agent.acceleration = float.PositiveInfinity;;
            agent.rotationSpeed = float.PositiveInfinity;
            agent.slowdownTime = float.Epsilon;
            agent.slowWhenNotFacingTarget = true;

            if (_infiniteBattery.Value)
            {
                MinionUnit unit = (MinionUnit)AccessTools.Field(typeof(MinionMover), "minionUnit").GetValue(__instance);
                FuelConsumer consumer = (FuelConsumer)AccessTools.Field(typeof(MinionUnit), "consumer").GetValue(unit);
                consumer.CurrentFuelLevel = consumer.MaxFuelLevel;
            }

            return false;
        }
    }

    private void Awake()
    {
        _logger = Logger;
        var h = new Harmony("faster_minions");
        h.PatchAll();

        _speedMultiplier = Config.Bind("Faster Minions",
            "Speed_Multiplier",
            4,
            "Multiples the speed of the minion by x.");

        _infiniteBattery = Config.Bind("Faster Minions",
            "Infinite_Battery",
            false,
            "Minions have infinite power.");

        _logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}