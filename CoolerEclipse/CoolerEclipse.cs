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
  [BepInPlugin("com.Nuxlar.CoolerEclipse", "CoolerEclipse", "1.1.0")]

  public class CoolerEclipse : BaseUnityPlugin
  {
    GameObject eclipseWeather = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/eclipseworld/Weather, Eclipse.prefab").WaitForCompletion();
    Material opaqueMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/voidraid/matVoidRaidPlanetBlueMarbleShell.mat").WaitForCompletion();
    Material moonMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/skymeadow/matSMSkyboxMoon.mat").WaitForCompletion();

    public static ConfigEntry<bool> shouldBeChance;
    private static ConfigFile CEConfig { get; set; }

    private string[] whitelistedMaps = new string[] {
      "snowyforest",
      "blackbeach",
      "blackbeach2",
      "golemplains",
      "golemplains2",
      "goolake",
      "foggyswamp",
      "ancientloft",
      "frozenwall",
      "wispgraveyard",
      "sulfurpools",
      "FBLScene",
      "shipgraveyard",
      "dampcavesimple",
      "rootjungle",
      "skymeadow"
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
        {
          GameObject sun = GameObject.Find("Directional Light (SUN)");
          GameObject amb = GameObject.Find("PP + Amb");
          GameObject probe = GameObject.Find("Reflection Probe");
          if (sun)
            sun.SetActive(false);
          if (amb)
            amb.SetActive(false);
          if (probe)
            probe.SetActive(false);
          GameObject probe2 = GameObject.Find("Reflection Probe");
          if (probe2)
            probe.SetActive(false);
        }

        GameObject newWeather = Instantiate(eclipseWeather, new Vector3(0, 0, 0), Quaternion.identity);
        Light moonLight = newWeather.transform.GetChild(1).GetComponent<Light>();
        moonLight.intensity = 1f;
        moonLight.shadowStrength = 0.5f;
        newWeather.transform.GetChild(2).GetComponent<SetAmbientLight>().ApplyLighting();
        newWeather.transform.GetChild(0).GetComponent<ReflectionProbe>().Reset();
        newWeather.transform.GetChild(3).GetChild(2).gameObject.SetActive(true);

        if (sceneName == "snowyforest")
        {
          moonLight.intensity = 0.25f;
          moonLight.shadowStrength = 0.1f;
        }

        if (sceneName == "rootjungle")
        {
          newWeather.transform.GetChild(0).gameObject.SetActive(false);
          GameObject groveWeather = GameObject.Find("HOLDER: Weather Set 1");
          if (groveWeather)
            groveWeather.gameObject.SetActive(false);
        }

        if (sceneName == "skymeadow")
        {
          GameObject moon = GameObject.Find("ShatteredMoonMesh");
          if (moon)
          {
            GameOb
          }
          moon.GetComponent<MeshRenderer>().sharedMaterials = new Material[2] { opaqueMat, moonMat };
        }

        NetworkServer.Spawn(newWeather);
      }
      orig(self);
    }

  }
}