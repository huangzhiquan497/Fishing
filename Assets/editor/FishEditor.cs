using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(Fish))]
[CanEditMultipleObjects]
public class FishEditor : Editor {
    private SerializedProperty mHittableTypeProp;
    private string[] mHittableTypeStrs;
    void OnEnable()
    {
        StringSet nameSet = AssetDatabase.LoadAssetAtPath("Assets/prefab/Fish/_defs/HittableType_Fish.prefab", typeof(StringSet)) as StringSet;
        if (nameSet != null)
            mHittableTypeStrs = nameSet.Texts;
        mHittableTypeProp = serializedObject.FindProperty("HittableTypeS");
    }
    public override void OnInspectorGUI()
    {
        //serializedObject.Update();

        DrawDefaultInspector();
        //if (!mHittableTypeProp.hasMultipleDifferentValues && mHittableTypeStrs != null)
        //{

        //    int selectIdx = -1;
        //    for (int i = 0; i != mHittableTypeStrs.Length; ++i)
        //    {
        //        if (mHittableTypeStrs[i] == mHittableTypeProp.stringValue)
        //        {
        //            selectIdx = i;
        //            break;
        //        }
        //    }
        //    //Debug.Log(selectIdx);
        //    string[] hittableTypeStrsFixed;
        //    if (selectIdx == -1)
        //    {
        //        hittableTypeStrsFixed = new string[mHittableTypeStrs.Length + 1];
        //        for (int i = 0; i != mHittableTypeStrs.Length; ++i)
        //        {
        //            hittableTypeStrsFixed[i] = mHittableTypeStrs[i];
        //        }
        //        hittableTypeStrsFixed[mHittableTypeStrs.Length] = "unset";
        //        selectIdx = mHittableTypeStrs.Length;//��idxΪ���
        //    }
        //    else
        //    {
        //        hittableTypeStrsFixed = mHittableTypeStrs;
        //    }


        //    int newSelectIdx = EditorGUILayout.Popup("HittableType", selectIdx, hittableTypeStrsFixed);
        //    if (newSelectIdx != selectIdx)
        //    {
        //        mHittableTypeProp.stringValue = mHittableTypeStrs[newSelectIdx];
        //    }
        //}
        //else
        //{
        //    EditorGUILayout.Popup("HittableType", 0, new string[]{"��"});
        //}
        

        if (!AssetDatabase.Contains(target)&&GUILayout.Button("ת��Ϊ�����"))
        {
            ConvertToFishGenerateWhenEnterWorld(target as Fish);
        }
        //serializedObject.ApplyModifiedProperties();
    }

    static void ConvertToFishGenerateWhenEnterWorld(Fish fishInScene)
    {
        if (AssetDatabase.Contains(fishInScene))
            return;
        //ɾ���Ӷ���
        //int a = fishInScene.transform.childCount;
        int fishChildCount = fishInScene.transform.childCount;
        for (int i = 0; i != fishChildCount; ++i)
            DestroyImmediate(fishInScene.transform.GetChild(0).gameObject);

        //����gameobject���ֲ�����prefab
        Fish f = (Fish)AssetDatabase.LoadAssetAtPath("assets/prefab/fish/" + fishInScene.gameObject.name + ".prefab", typeof(Fish));
        if (f == null)
            f = (Fish)AssetDatabase.LoadAssetAtPath("assets/prefab/FishQueue/" + fishInScene.gameObject.name + ".prefab", typeof(Fish));

        //����FishGenerateWhenEnterWorld
        FishGenerateWhenEnterWorld fishGenerate = fishInScene.gameObject.AddComponent<FishGenerateWhenEnterWorld>();
        if (f != null)
        {
            fishGenerate.Prefab_Fish = f;
        }
        else
        {
            Debug.LogWarning("[����]����ԴĿ¼���Ҳ�������prefab,���ֶ������ϷŽ�prefab_Fish������.", fishInScene.gameObject);
        }

        //���������
        Component[] cmps = fishInScene.GetComponents<Component>();

        foreach (Component c in cmps)
        {
            if (c.GetType() != typeof(Transform) && c.GetType() != typeof(FishGenerateWhenEnterWorld))
            {
                DestroyImmediate(c);
            }
        }
    }
	
	[MenuItem("HappyFishes/Fish/CreateFishAnimation")]
	static void Menu_CreateFishAnimation()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Fish f = go.GetComponent<Fish>();
            if (f != null && f.Prefab_GoAniSwim != null)
            {
				GameObject goAni = Instantiate(f.Prefab_GoAniSwim) as GameObject;
				goAni.transform.parent = f.transform;
				goAni.transform.localPosition = Vector3.zero;
				goAni.transform.localRotation = Quaternion.identity;
				
             
            }
        }
        
    }
    [MenuItem("HappyFishes/Fish/Fish -> �����")]
    static void Menu_ConvertToFishGenerateWhenEnterWorld()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Fish f = go.GetComponent<Fish>();
            if (f != null)
                ConvertToFishGenerateWhenEnterWorld(f);
        }
        
    }

    [MenuItem("HappyFishes/ScaleTo384")]
    static void Menu_TranSwimPropertyScal384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Swimmer s = go.GetComponent<Swimmer>();
            if (s != null)
            {
                s.Speed *= 384F;
                s.BoundCircleRadius *= 384F;
            }
            FishEmitter_Queue fq = go.GetComponent<FishEmitter_Queue>();
            if(fq != null)
            {
                fq.Distance *= 384F;
                fq.Fish_Speed *= 384F;
            } 
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/BoxCollider_ScaleTo384")]
    static void Menu_BoxCollider_ScaleTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {

            BoxCollider bc = go.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.center *= 384F;
                Vector3 boundSize = bc.size;
                boundSize.x *= 384F;
                boundSize.y *= 384F;
                bc.size = boundSize;

            }
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/ParticelShuriken_ScaleTo384")]
    static void Menu_ParticelShuriken_ScaleTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.startSpeed *= 384F;
                ps.startSize *= 384F;

            }
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/SlicedSprite_ScaleTo384")]
    static void Menu_SlicedSprite_ScaleTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            tk2dSlicedSprite ps = go.GetComponent<tk2dSlicedSprite>();
            if (ps != null)
            {
                Vector2 dims = ps.dimensions;
                dims *= 0.85735F;
                ps.dimensions = dims;

            }
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/CursorDimension_ScalTo384")]
    static void Menu_CursorDimension_ScalTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            CursorDimLocation cdl = go.GetComponent<CursorDimLocation>();
            if (cdl != null)
            {
                cdl.Dimension *= 384F;

            }

            CusorDimEx_LanguageAutoSelect cde_las = go.GetComponent<CusorDimEx_LanguageAutoSelect>();
            if (cde_las != null)
            {
                for (int i = 0; i != cde_las.Prefab_DimsByLangauge.Length; ++i)
                {
                    cde_las.Prefab_DimsByLangauge[i] *= 384F;
                }
            }
            LocalPos_LanguageAutoSelect lp_las = go.GetComponent<LocalPos_LanguageAutoSelect>();
            if (lp_las != null)
            {
                for (int i = 0; i != lp_las.LocalPosition.Length; ++i)
                {
                    lp_las.LocalPosition[i] *= 384F;
                }
            }
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/TranLocalPosXYScalTo384")]
    static void Menu_TranPosXYScalTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Vector3 localPos = go.transform.localPosition;
            localPos.x *= 384F;
            localPos.y *= 384F;
            localPos.z *= 384F;
            go.transform.localPosition = localPos;
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/TranLocalScaleScalTo384")]
    static void Menu_TranLocalScaleScalTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Vector3 localScale = go.transform.localScale;
            localScale.x *= 0.85375F;
            localScale.y *= 0.85375F;
            localScale.z *= 0.85375F;
            go.transform.localScale = localScale;
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("HappyFishes/TranPlayerCreatorDataTo384")]
    static void Menu_TranPlayerCreatorDataTo384()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            PlayerCreator pc = go.GetComponent<PlayerCreator>();
            if (pc != null)
            {
                Vector3 tmpPos = pc.CoinStacksLocalPos;
                tmpPos.x *= 384F;
                tmpPos.y *= 384F;
                tmpPos.z *= 10F;
                pc.CoinStacksLocalPos = tmpPos;

                tmpPos = pc.ScoreBGLocalPos;
                tmpPos.x *= 384F;
                tmpPos.y *= 384F;
                tmpPos.z *= 10F;
                pc.ScoreBGLocalPos = tmpPos;
            }
            EditorUtility.SetDirty(go);
        }
    }
}


