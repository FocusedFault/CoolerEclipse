using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System.Linq;

namespace CoolerEclipse
{
  [BepInPlugin("com.Nuxlar.CoolerEclipse", "CoolerEclipse", "1.0.2")]

  public class CoolerEclipse : BaseUnityPlugin
  {
    GameObject eclipseWeather = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/eclipseworld/Weather, Eclipse.prefab").WaitForCompletion();
    public static ConfigEntry<bool> shouldBeChance;
    private static ConfigFile CEConfig { get; set; }

    private string[] whitelistedMaps = new string[] {
      "blackbeach2",
      "golemplains",
      "golemplains2",
      "goolake",
      "ancientloft",
      "frozenwall",
      "wispgraveyard",
      "sulfurpools",
      "FBLScene",
      "shipgraveyard",
      "rootjungle"
};

    public void Awake()
    {
      CEConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.CoolerEclipse.cfg", true);
      shouldBeChance = CEConfig.Bind<bool>("General", "Chance Based", false, "Make the weather changes be a 50% chance instead of guaranteed.");
      eclipseWeather.AddComponent<NetworkIdentity>();
      On.RoR2.Stage.Start += Stage_Start;
    }

    private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
    {
      GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
      string sceneName = SceneManager.GetActiveScene().name;
      if (Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse1 && whitelistedMaps.Contains(sceneName))
      {
        if (shouldBeChance.Value && Random.value > 0.5)
        {
          orig(self);
          return;
        }

        if (gameObjects.Where((obj) => obj.name.Contains("Weather,")).Count() > 0)
        {
          gameObjects.Where((obj) => obj.name.Contains("Weather,")).First<GameObject>().SetActive(false);
          if (gameObjects.Where((obj) => obj.name == "Sun").Count() > 0)
            gameObjects.Where((obj) => obj.name == "Sun").First<GameObject>().SetActive(false);
        }
        else
          GameObject.Find("Directional Light (SUN)").gameObject.SetActive(false);

        if (sceneName == "rootjungle")
          GameObject.Find("HOLDER: Weather Set 1").gameObject.SetActive(false);

        GameObject newWeather = Instantiate(eclipseWeather, new Vector3(0, 0, 0), Quaternion.identity);
        newWeather.transform.GetChild(3).GetChild(2).gameObject.SetActive(true);

        if (sceneName == "FBLScene" || sceneName == "shipgraveyard")
          newWeather.transform.GetChild(1).GetComponent<Light>().intensity = 1;
        if (sceneName == "frozenwall")
        {
          newWeather.transform.GetChild(0).gameObject.SetActive(false);
          newWeather.transform.GetChild(1).gameObject.SetActive(false);
        }
        NetworkServer.Spawn(newWeather);
      }
      orig(self);
    }

  }
}