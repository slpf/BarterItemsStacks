using BepInEx.Configuration;

namespace BarterItemsStacksClient;

public class Settings
{
    public static ConfigEntry<bool> FirStackableResources {get; set;}
    public static ConfigEntry<bool> FirStackableMed {get; set;}
    public static ConfigEntry<bool> FirStackableFoodDrinks {get; set;}
    public static ConfigEntry<bool> FirStackableRepairKits {get; set;}

    public static void Init(ConfigFile config)
    {
        FirStackableResources = config.Bind("FiR & non-FiR stacking (behavior may be unpredictable)", "Barter items", false, new ConfigDescription("Allows you to stack FiR and non-FiR barter items."));
        FirStackableMed = config.Bind("FiR & non-FiR stacking (behavior may be unpredictable)","Medical items", false, new ConfigDescription("Allows you to stack FiR and non-FiR medical items."));
        FirStackableFoodDrinks = config.Bind("FiR & non-FiR stacking (behavior may be unpredictable)","Food/Drink items", false, new ConfigDescription("Allows you to stack FiR and non-FiR food/drink items."));
        FirStackableRepairKits = config.Bind("FiR & non-FiR stacking (behavior may be unpredictable)","Repair kit items", false, new ConfigDescription("Allows you to stack FiR and non-FiR repair kit items."));
    }
}