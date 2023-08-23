using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneListEditorWindow : EditorWindow
{
    const string _buttonPath = "Assets/SceneListButtonContainer.uxml";
    const string _baseUIToolkitPath = "Assets/SceneListUIToolkit.uxml";
    [MenuItem("Tools/Scene List")]
    static void CreateMenu()
    {
        var window = GetWindow<SceneListEditorWindow>();
        window.titleContent = new GUIContent("Scene List");
    }

    private void CreateGUI()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_baseUIToolkitPath);
        var root = tree.Instantiate();
        rootVisualElement.Add(root);

        var treeButton = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_buttonPath);
        var Pinned = rootVisualElement.Q<ScrollView>("Pinned");
        var Unpinned = rootVisualElement.Q<ScrollView>("Unpinned");

        rootVisualElement.Add(Pinned);
        rootVisualElement.Add(Unpinned);

        var sceneAssets = FindAssetsOfType<SceneAsset>();
        List<TemplateContainer> pinnedSaveData = new();
        for (int i = 0; i < sceneAssets.Length; i++)
        {

            var item = sceneAssets[i];
            string path = AssetDatabase.GetAssetPath(item);
            var buttonContainer = treeButton.Instantiate();
            Unpinned.Add(buttonContainer);

            var sceneNameButton = buttonContainer.Q<Button>("SceneName");
            sceneNameButton.text = item.name;
            sceneNameButton.clicked += SceneName_clicked;

            var pin = buttonContainer.Q<Button>("Pin");
            pin.clicked += Pin_clicked;

            var go = buttonContainer.Q<Button>("Go");
            go.clicked += Go_clicked;

            var pathPinned = EditorPrefs.GetString("PinnedScene" + path);
            if (!string.IsNullOrEmpty(pathPinned) && !string.IsNullOrWhiteSpace(pathPinned))
            {
                if (pathPinned.Equals(path, System.StringComparison.Ordinal))
                    pinnedSaveData.Add(buttonContainer);
            }


            void SceneName_clicked()
            {
                var cacheSceneName = sceneNameButton;
                var cachePath = path;
                EditorSceneManager.OpenScene(cachePath);
            }
            void Pin_clicked()
            {
                var cacheParentButton = buttonContainer;
                var cachePath = path;
                var cachePin = pin;
                if (IsPinned()) // remove from pin
                {
                    Pinned.Remove(cacheParentButton);
                    Unpinned.Insert(0, cacheParentButton);
                    cachePin.text = "Pin";
                    EditorPrefs.DeleteKey("PinnedScene" + path);

                }
                else // add to pin
                {
                    Unpinned.Remove(cacheParentButton);
                    Pinned.Add(cacheParentButton);
                    cachePin.text = "Unpin";
                    EditorPrefs.SetString("PinnedScene" + path, path);
                }

                bool IsPinned() => Pinned.Contains(cachePin);

            }

            void Go_clicked()
            {
                var cachePath = path;
                var cacheGo = go;
                var cacheSceneAsset = item;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = cacheSceneAsset;

            }
        }

        for (int i = 0; i < pinnedSaveData.Count; i++)
        {
            Unpinned.Remove(pinnedSaveData[i]);
            Pinned.Add(pinnedSaveData[i]);
            var pin = pinnedSaveData[i].Q<Button>("Pin");
            pin.text = "Unpin";
        }
        
    }

    private T[] FindAssetsOfType<T>() where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> list = new List<T>();
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (asset != null)
            {
                list.Add(asset);
            }
        }

        return list.ToArray();
    }


}

