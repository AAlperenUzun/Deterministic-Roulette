using System.IO;
using Roulette.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Roulette.Editor
{
    // Generates the whole playable scene from code (camera, lighting, wheel, managers, canvas). Run from the
    // Roulette menu or headless via -executeMethod, so the scene is fully reproducible.
    public static class SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("Roulette/Build Game Scene")]
        public static void BuildGameScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreatePostProcessing();
            CreateLighting();
            CreateEnvironment();
            Transform wheel = CreateWheel();
            CreateManagers(wheel);
            CreateCelebration();
            CreateCanvasAndHud();
            CreateEventSystem();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneBuilder] Built scene at {ScenePath}");
        }

        // Imports the bundled TMP essential resources (font + settings) headlessly.
        public static void ImportTextMeshProEssentials()
        {
            const string marker = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if (File.Exists(marker))
            {
                Debug.Log("[SceneBuilder] TMP essentials already present.");
                EditorApplication.Exit(0);
                return;
            }

            string package = Path.Combine(EditorApplication.applicationContentsPath,
                "Resources/PackageManager/BuiltInPackages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");

            AssetDatabase.importPackageCompleted += _ =>
            {
                AssetDatabase.Refresh();
                Debug.Log("[SceneBuilder] TMP essentials imported.");
                EditorApplication.Exit(0);
            };
            AssetDatabase.importPackageFailed += (_, error) =>
            {
                Debug.LogError($"[SceneBuilder] TMP import failed: {error}");
                EditorApplication.Exit(1);
            };
            AssetDatabase.ImportPackage(package, false);
        }

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.045f, 0.06f);
            cam.fieldOfView = 39f;
            cam.nearClipPlane = 0.05f;
            go.transform.position = new Vector3(0f, 3.66f, -3.0f);
            go.transform.LookAt(new Vector3(0f, -0.68f, 0.55f));

            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
        }

        private static void CreatePostProcessing()
        {
            const string profilePath = "Assets/Settings/RoulettePostFX.asset";
            if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath) != null)
                AssetDatabase.DeleteAsset(profilePath);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.85f);
            bloom.threshold.Override(0.92f);
            bloom.scatter.Override(0.62f);
            bloom.tint.Override(new Color(1f, 0.96f, 0.86f));

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.5f);

            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.12f);
            color.contrast.Override(10f);
            color.saturation.Override(8f);

            EditorUtility.SetDirty(profile);

            var go = new GameObject("Global Volume", typeof(Volume));
            Volume volume = go.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        private static void CreateLighting()
        {
            var keyGo = new GameObject("Key Light", typeof(Light));
            var key = keyGo.GetComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.96f, 0.88f);
            key.intensity = 1.25f;
            key.shadows = LightShadows.Soft;
            keyGo.transform.rotation = Quaternion.Euler(50f, -28f, 0f);

            var spotGo = new GameObject("Wheel Light", typeof(Light));
            var spot = spotGo.GetComponent<Light>();
            spot.type = LightType.Point;
            spot.color = new Color(1f, 0.92f, 0.78f);
            spot.intensity = 14f;
            spot.range = 9f;
            spotGo.transform.position = new Vector3(0f, 2.4f, -0.4f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.18f, 0.15f);
        }

        private static void CreateEnvironment()
        {
            Material floorMat = MaterialFactory.Create(new Color(0.05f, 0.06f, 0.08f), 0.1f, 0.2f);
            Transform floor = Primitive(PrimitiveType.Plane, "Floor", new Vector3(0f, -0.55f, 0f), Vector3.one * 4f, floorMat);
            floor.gameObject.isStatic = true;

            Material pedestalMat = MaterialFactory.Create(MaterialFactory.WoodDark, 0.1f, 0.4f);
            Primitive(PrimitiveType.Cylinder, "Pedestal", new Vector3(0f, -0.52f, 0.3f), new Vector3(1.7f, 0.32f, 1.7f), pedestalMat);
        }

        private static Transform CreateWheel()
        {
            var go = new GameObject("Wheel", typeof(WheelView));
            go.transform.position = new Vector3(0f, 0f, 0.3f);
            go.transform.localScale = Vector3.one * 1.5f;
            return go.transform;
        }

        private static void CreateManagers(Transform wheel)
        {
            new GameObject("GameManager", typeof(GameManager));

            var controllerGo = new GameObject("RouletteController", typeof(RouletteController));
            var controller = controllerGo.GetComponent<RouletteController>();
            var so = new SerializedObject(controller);
            so.FindProperty("_spinAnimator").objectReferenceValue = wheel.GetComponent<WheelView>();
            so.ApplyModifiedProperties();

            new GameObject("AudioManager", typeof(AudioManager));
        }

        private static void CreateCelebration()
        {
            var go = new GameObject("WinCelebration", typeof(ParticleSystem));
            go.transform.position = new Vector3(0f, 2.6f, 0.3f);
            go.AddComponent<WinCelebration>();
        }

        private static void CreateCanvasAndHud()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var hudGo = new GameObject("HUD", typeof(RectTransform), typeof(HudBootstrap));
            var hud = (RectTransform)hudGo.transform;
            hud.SetParent(canvasGo.transform, false);
            hud.anchorMin = Vector2.zero;
            hud.anchorMax = Vector2.one;
            hud.offsetMin = hud.offsetMax = Vector2.zero;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private static Transform Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        private static void RegisterInBuildSettings()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
