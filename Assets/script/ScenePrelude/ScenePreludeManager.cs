using UnityEngine;
using System.Collections;

public class ScenePreludeManager : MonoBehaviour
{
    public ScenePrelude[] Prefab_preludes;
    public bool IsRandomPrelude = true;//是否随机鱼阵
    public int PreludeIdxStart = 0;//开始的idx

    public ScenePrelude DoPrelude()
    {
        ScenePrelude sp;
        if (IsRandomPrelude)
        {
            sp = Instantiate(Prefab_preludes[Random.Range(0, Prefab_preludes.Length)]) as ScenePrelude;
        }
        else
        {
            sp = Instantiate(Prefab_preludes[PreludeIdxStart]) as ScenePrelude;
            PreludeIdxStart = (PreludeIdxStart + 1) % Prefab_preludes.Length;
        }
        


        sp.transform.parent = transform;
        Vector3 localPos = sp.transform.localPosition;
        localPos.z = 0F;
        sp.transform.localPosition = localPos;
        sp.Go();
        return sp;
    }
}
