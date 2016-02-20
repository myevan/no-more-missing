using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class HelpMissingScriptMenu : MonoBehaviour
{
    [MenuItem("Help/Prepare For Missing Scripts")]
    public static void PrepareForMissingScripts()
    {
        var prefabList = CollectPrefabList();
        foreach (var prefab in prefabList)
        {
            bool isMarked = false;
            var behaviours = prefab.GetComponents<MonoBehaviour>();
            foreach (var behaviour in behaviours)
            {
                var serializedObject = new SerializedObject(behaviour);
                var nameProp = serializedObject.FindProperty("m_Name");
                var className = behaviour.GetType().Name;
                if (nameProp.stringValue != className)
                {
                    nameProp.stringValue = className;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log(string.Format("mark_class:{0} prefab:{1}", className, prefab.name));

                    isMarked = true;
                }
            }

            if (isMarked)
            {
                EditorUtility.SetDirty(prefab);
            }
        }

        AssetDatabase.SaveAssets();
        EditorApplication.SaveAssets();
    }

    [MenuItem("Help/Fix Missing Scripts")]
    public static void FixMissingScripts()
    {
        var newBehaviourLineDict = new Dictionary<string, string>();

        var brokenPrefabList = CollectBrokenPrefabList();
        var totalBehaviourDict = CollectTotalBehaviourDict();
        var behaviourTypeList = CollectBehaviourTypeList(brokenPrefabList, totalBehaviourDict);
        foreach (var behaviourType in behaviourTypeList)
        {
            var tempObject = MakeObjectWithBehaviour(behaviourType);
            var tempBehaviourLineDict = LoadBehaviourLineDict(tempObject);
            foreach (var pair in tempBehaviourLineDict)
            {
                if (newBehaviourLineDict.ContainsKey(pair.Key))
                    continue;

                newBehaviourLineDict.Add(pair.Key, pair.Value);
            }
        }

        foreach (var prefab in brokenPrefabList)
        {
            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            FixBrokenObjectAsset(prefabPath, newBehaviourLineDict);
        }

        return;

    }

    private static List<GameObject> CollectPrefabList()
    {
        var prefabList = new List<GameObject>();
        var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            //if (gameObject.transform.parent != null) continue;

            if (!IsPrefab(gameObject))
                continue;

            prefabList.Add(gameObject);
        }
        return prefabList;
    }

    private static bool IsPrefab(GameObject obj)
    {
        if (obj == null)
            return false;

        if (PrefabUtility.GetPrefabParent(obj) != null)
            return false;

        if (PrefabUtility.GetPrefabObject(obj) == null)
            return false;

        return true;
    }

    static List<GameObject> CollectBrokenPrefabList()
    {
        var brokenPrefabList = new List<GameObject>();
        var allPrefabList = CollectPrefabList();
        foreach (var prefab in allPrefabList)
        {
            if (HasBrokenComponent(prefab))
                brokenPrefabList.Add(prefab);
        }
        return brokenPrefabList;
    }

    static bool HasBrokenComponent(GameObject gameObject)
    {
        var components = gameObject.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null)
                return true;
        }

        return false;
    }

    private static Dictionary<string, System.Type> CollectTotalBehaviourDict()
    {
        var behaviourTypeDict = new Dictionary<string, System.Type>();
        var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
        foreach (var script in scripts)
        {
            var scriptClass = script.GetClass();
            if (scriptClass == null)
                continue;

            if (!scriptClass.IsSubclassOf(typeof(MonoBehaviour)))
                continue;

            var scriptPath = AssetDatabase.GetAssetPath(script);
            if (!scriptPath.StartsWith("Assets/"))
                continue;

            if (scriptPath.Contains("Editor"))
                continue;

            if (!behaviourTypeDict.ContainsKey(scriptClass.Name))
            {
                behaviourTypeDict.Add(scriptClass.Name, script.GetClass());
            }
        }

        return behaviourTypeDict;
    }

    private static List<System.Type> CollectBehaviourTypeList(List<GameObject> prefabList, Dictionary<string, System.Type> scriptClassDict)
    {
        var behaviourTypeList = new List<System.Type>();
        foreach (var prefab in prefabList)
        {
            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            Debug.Log("broken_prefab:" + prefabPath);
            var scriptLineDict = LoadBehaviourLineDict(prefabPath);
            foreach (var className in scriptLineDict.Keys)
            {
                System.Type behaviourType;
                if (!scriptClassDict.TryGetValue(className, out behaviourType))
                {
                    continue;
                }

                if (behaviourTypeList.Contains(behaviourType))
                {
                    continue;
                }

                behaviourTypeList.Add(behaviourType);
            }
        }
        return behaviourTypeList;
    }

    private static Dictionary<string, string> LoadBehaviourLineDict(string prefabPath)
    {
        var behaviourLineDict = new Dictionary<string, string>();
        var projectAbsPath = Path.GetDirectoryName(Application.dataPath);
        var prefabAbsPath = projectAbsPath + "/" + prefabPath;
        using (var streamReader = new StreamReader(prefabAbsPath))
        {
            string scriptLine = "";

            var text = streamReader.ReadToEnd();
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("---"))
                {
                    scriptLine = "";
                }

                if (line.StartsWith("  m_Script:"))
                {
                    scriptLine = line;
                }

                if (scriptLine.Length > 0)
                {
                    if (line.StartsWith("  m_Name:"))
                    {
                        var tokens = line.Split(':');
                        var className = tokens[1].Trim();
                        if (!behaviourLineDict.ContainsKey(className))
                            behaviourLineDict.Add(className, scriptLine);
                    }
                }

            }
        }

        return behaviourLineDict;
    }

    private static GameObject MakeObjectWithBehaviour(System.Type behaviourType)
    {
        var tempName = "__" + behaviourType.Name;
        var tempObject = new GameObject(tempName);
        var newComponent = tempObject.AddComponent(behaviourType);

        var serializedObject = new SerializedObject(newComponent);
        var nameProp = serializedObject.FindProperty("m_Name");
        nameProp.stringValue = behaviourType.Name;
        serializedObject.ApplyModifiedProperties();
        return tempObject;
    }

    private static Dictionary<string, string> LoadBehaviourLineDict(GameObject tempObject)
    {
        var tempRelPath = string.Format("Assets/{0}.prefab", tempObject.name);
        var tempPrefab = PrefabUtility.CreatePrefab(
            tempRelPath,
            tempObject, 
            ReplacePrefabOptions.ReplaceNameBased);

        var scriptDict = new Dictionary<string, string>();

        var projectAbsPath = Path.GetDirectoryName(Application.dataPath);
        var tempAbsPath = projectAbsPath + "/" + tempRelPath;
        using (var streamReader = new StreamReader(tempAbsPath))
        {
            string scriptLine = "";

            var text = streamReader.ReadToEnd();
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("---"))
                {
                    scriptLine = "";
                }

                if (line.StartsWith("  m_Script:"))
                {
                    scriptLine = line;
                }

                if (scriptLine.Length > 0)
                {
                    if (line.StartsWith("  m_Name:"))
                    {
                        var tokens = line.Split(':');
                        var className = tokens[1].Trim();
                        if (className.Length > 0 && !scriptDict.ContainsKey(className))
                        {
                            scriptDict.Add(className, scriptLine);
                        }
                    }
                }

            }
        }

        DestroyImmediate(tempObject);
        AssetDatabase.DeleteAsset(tempRelPath);
        return scriptDict;
    }

    private static void FixBrokenObjectAsset(string assetRelPath, Dictionary<string, string> behaviourLineDict)
    {
        var newLines = new List<string>();

        var projectAbsPath = Path.GetDirectoryName(Application.dataPath);
        var assetAbsPath = projectAbsPath + "/" + assetRelPath;
        using (var streamReader = new StreamReader(assetAbsPath))
        {
            int scriptLineIndex = -1;

            var text = streamReader.ReadToEnd();
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("---"))
                {
                    scriptLineIndex = -1;
                }

                if (line.StartsWith("  m_Script:"))
                {
                    scriptLineIndex = newLines.Count;
                }

                if (scriptLineIndex >= 0)
                {
                    if (line.StartsWith("  m_Name:"))
                    {
                        var tokens = line.Split(':');
                        var className = tokens[1].Trim();

                        string newScriptLine;
                        if (behaviourLineDict.TryGetValue(className, out newScriptLine))
                        {
                            var oldScriptLine = newLines[scriptLineIndex];
                            if (oldScriptLine != newScriptLine)
                            {
                                Debug.Log("fix:" + className);
                                newLines[scriptLineIndex] = newScriptLine;
                            }
                        }
                    }
                }

                newLines.Add(line);
            }
        }

        using (var streamWriter = new StreamWriter(assetAbsPath))
        {
            streamWriter.Write(string.Join("\n", newLines.ToArray()));
        }
    }

}
