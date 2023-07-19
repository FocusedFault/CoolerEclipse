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
  [BepInPlugin("com.Nuxlar.CoolerEclipse", "CoolerEclipse", "1.0.1")]

  public class CoolerEclipse : BaseUnityPlugin
  {
    GameObject eclipseWeather = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/eclipseworld/Weather, Eclipse.prefab").WaitForCompletion();
    public static ConfigEntry<bool> shouldBeChance;
    private static ConfigFile CEConfig { get; set; }

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
      if (gameObjects.Where((obj) => obj.name.Contains("Weather,")).Count() > 0 && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse1 && sceneName != "voidraid" && sceneName != "voidstage")
      {
        if (shouldBeChance.Value && Random.value > 0.5)
        {
          orig(self);
          return;
        }
        GameObject newWeather = Instantiate(eclipseWeather, new Vector3(0, 0, 0), Quaternion.identity);
        newWeather.transform.GetChild(3).GetChild(2).gameObject.SetActive(true);
        NetworkServer.Spawn(newWeather);
        gameObjects.Where((obj) => obj.name.Contains("Weather,")).First<GameObject>().SetActive(false);
        if (gameObjects.Where((obj) => obj.name == "Sun").Count() > 0)
          gameObjects.Where((obj) => obj.name == "Sun").First<GameObject>().SetActive(false);
      }
      orig(self);
    }

  }
}