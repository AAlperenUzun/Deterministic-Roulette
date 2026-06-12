using System.IO;
using Roulette.Core;
using Roulette.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Roulette.Editor
{
    // Editor-only: renders the procedural wheel to a PNG without entering play mode. Not part of the game.
    public static class PreviewCapture
    {
        [MenuItem("Roulette/Capture Wheel Preview")]
        public static void CaptureWheel()
        {
            RouletteType type = RouletteType.European;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Light", typeof(Light));
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -28f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.24f, 0.21f, 0.17f);

            var wheelGo = new GameObject("Wheel");
            wheelGo.transform.localScale = Vector3.one * 1.5f;
            RouletteWheelBuilder.Build(wheelGo.transform, RouletteWheel.Create(type));

            foreach (TextMeshPro label in Object.FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None))
                label.ForceMeshUpdate();

            var camGo = new GameObject("Cam", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
            cam.fieldOfView = 38f;
            camGo.transform.position = new Vector3(0f, 2.7f, -3.2f);
            camGo.transform.LookAt(new Vector3(0f, 0.05f, 0.35f));

            const int w = 1280, h = 960;
            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            cam.targetTexture = null;

            Directory.CreateDirectory("Temp");
            File.WriteAllBytes("Temp/wheel-preview.png", tex.EncodeToPNG());
            Debug.Log("[PreviewCapture] wrote Temp/wheel-preview.png");

            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
