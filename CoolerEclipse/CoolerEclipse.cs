using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;

namespace CoolerEclipse
{
  [BepInPlugin("com.Nuxlar.CoolerEclipse", "CoolerEclipse", "1.1.1")]

  public class CoolerEclipse : BaseUnityPlugin
  {
    GameObject eclipseWeather = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/eclipseworld/Weather, Eclipse.prefab").WaitForCompletion();
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
      "rootjungle",
      "skymeadow",
      "moon2"
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
          if (sun && sceneName != "moon2" && sceneName != "skymeadow")
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
        moonLight.shadowStrength = 0.25f;
        SetAmbientLight ambLight = newWeather.transform.GetChild(2).GetComponent<SetAmbientLight>();
        ambLight.ambientIntensity = 0.75f;
        ambLight.ApplyLighting();
        if (sceneName == "blackbeach" || sceneName == "moon2")
          newWeather.transform.GetChild(2).GetComponent<PostProcessVolume>().priority = 9999f;
        newWeather.transform.GetChild(0).GetComponent<ReflectionProbe>().Reset();
        newWeather.transform.GetChild(3).GetChild(2).gameObject.SetActive(true);

        if (sceneName.Contains("blackbeach") || sceneName.Contains("golemplains"))
        {
          ambLight.ambientIntensity = 1f;
          ambLight.ApplyLighting();
        }

        if (sceneName == "snowyforest")
        {
          ambLight.ambientIntensity = 0.5f;
          ambLight.ApplyLighting();
          GameObject trees = GameObject.Find("Treecards");
          if (trees)
            trees.SetActive(false);
        }

        if (sceneName == "frozenwall")
        {
          moonLight.intensity = 0.25f;
          ambLight.ambientIntensity = 0.25f;
          ambLight.ApplyLighting();
        }

        if (sceneName == "rootjungle")
        {
          newWeather.transform.GetChild(0).gameObject.SetActive(false);
          GameObject groveWeather = GameObject.Find("HOLDER: Weather Set 1");
          if (groveWeather)
            groveWeather.gameObject.SetActive(false);
        }

        if (sceneName.Contains("skymeadow"))
        {
          newWeather.transform.GetChild(1).gameObject.SetActive(false);
          GameObject sun = GameObject.Find("Directional Light (SUN)");
          sun.GetComponent<Light>().color = moonLight.color;
          ambLight.ambientIntensity = 1f;
          ambLight.ApplyLighting();
          GameObject moon = GameObject.Find("ShatteredMoonMesh");
          if (moon)
            moon.GetComponent<MeshRenderer>().sortingOrder = -1;
        }

        if (sceneName == "moon2")
        {
          newWeather.transform.GetChild(1).gameObject.SetActive(false);
          GameObject sun = GameObject.Find("Directional Light (SUN)");
          sun.GetComponent<Light>().color = moonLight.color;
          ambLight.ambientIntensity = 0.5f;
          ambLight.ApplyLighting();
          GameObject planet = GameObject.Find("Moon");
          if (planet)
            planet.GetComponent<MeshRenderer>().sortingOrder = -1;
        }

        NetworkServer.Spawn(newWeather);
      }
      orig(self);
    }

  }
}