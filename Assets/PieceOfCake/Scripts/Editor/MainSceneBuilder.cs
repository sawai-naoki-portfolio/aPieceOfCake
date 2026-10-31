using PieceOfCake.Game.App;
using PieceOfCake.Game.Content;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace PieceOfCake.EditorTools
{
    /// <summary>
    /// Builds the single game scene from scratch: camera, sun, post-processing volume, UI document
    /// and the <see cref="GameBootstrap"/> with all references assigned. Registers it in the build.
    /// </summary>
    internal static class MainSceneBuilder
    {
        [MenuItem(ProjectPaths.MenuRoot + "Rebuild Main Scene", priority = 2)]
        public static void Rebuild()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ArtCatalogBuilder.Rebuild();
            var panelSettings = EnsurePanelSettings();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = new Vector3(0f, 6f, -22f);

            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.55f;
            sunGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            var volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProjectPaths.VolumeProfile);
            if (volumeProfile != null)
            {
                var volume = new GameObject("Global Volume").AddComponent<Volume>();
                volume.isGlobal = true;
                volume.sharedProfile = volumeProfile;
            }

            var app = new GameObject("Game");
            var document = app.AddComponent<UIDocument>();
            // Assign through serialization: the UIDocument property setters do not persist in edit mode.
            var documentSo = new SerializedObject(document);
            documentSo.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
            documentSo.FindProperty("sourceAsset").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ProjectPaths.UiDocument);
            documentSo.ApplyModifiedPropertiesWithoutUndo();
            var bootstrap = app.AddComponent<GameBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("levelFile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(ProjectPaths.Levels);
            so.FindProperty("art").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameArt>(ProjectPaths.GameArt);
            so.FindProperty("uiDocument").objectReferenceValue = document;
            so.FindProperty("worldCamera").objectReferenceValue = camera;
            so.FindProperty("sun").objectReferenceValue = sun;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetFolders.Ensure(ProjectPaths.Scenes);
            EditorSceneManager.SaveScene(scene, ProjectPaths.MainScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ProjectPaths.MainScene, true) };
            Debug.Log("[PieceOfCake] Main scene rebuilt and set as the only build scene.");
        }

        private static PanelSettings EnsurePanelSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(ProjectPaths.PanelSettings);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, ProjectPaths.PanelSettings);
            }

            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ProjectPaths.RuntimeTheme);
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1280, 720);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
