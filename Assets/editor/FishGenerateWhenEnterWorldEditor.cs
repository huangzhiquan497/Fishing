using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FishGenerateWhenEnterWorld))]
[CanEditMultipleObjects]
public class FishGenerateWhenEnterWorldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("转换为Fish"))
        {
            ConvertToFish(target as FishGenerateWhenEnterWorld);
        }
    }

    [MenuItem("HappyFishes/Fish/出鱼点 -> Fish")]
    static void Menu_ConvertToFish()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            FishGenerateWhenEnterWorld fp = go.GetComponent<FishGenerateWhenEnterWorld>();
            if (fp != null)
                ConvertToFish(fp);
        }
    }

    static void ConvertToFish(FishGenerateWhenEnterWorld fishPot)
    {
        if (AssetDatabase.Contains(fishPot))
            return;

        if (fishPot.Prefab_Fish == null)
        {
            Debug.LogWarning("[警告]不存在Prefab_Fish,转化失败.", fishPot.gameObject);
            return;
        }
        //删除子对象
        foreach (Transform child in fishPot.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        
        //创建鱼对象
        Fish f = Instantiate(fishPot.Prefab_Fish) as Fish;
 
        f.gameObject.name = fishPot.gameObject.name;
        f.transform.parent = fishPot.transform.parent;
        f.transform.localPosition = fishPot.transform.localPosition;
        f.transform.localRotation = fishPot.transform.localRotation;
        f.transform.localScale = fishPot.transform.localScale;

        Selection.activeGameObject = f.gameObject;
        //删除Fishpot对象
        DestroyImmediate(fishPot.gameObject);

    }
}