using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopupDigitFishDie : MonoBehaviour {
    public tk2dTextMesh Prefab_Digit;
    public float Duration;
    public AnimationCurve Curve_OffsetY;
    public AnimationCurve Curve_Alpha;
    [System.Serializable]
    public class ScaleData
    {
        public Fish Fish_;
        public float Scale = 1F;
    }

    public ScaleData[] ScaleDatas;
    private Dictionary<int, float> mScaleDatasMap;
    void Start()
    {
        GameMain.EvtFishKilled += Handle_FishKilled;
        if (mScaleDatasMap == null)
        {
            mScaleDatasMap = new Dictionary<int, float>();
            foreach (ScaleData sd in ScaleDatas)
            {
                mScaleDatasMap.Add(sd.Fish_.TypeIndex, sd.Scale);
            }
        }
    }

    void Handle_FishKilled(Player killer, Bullet b,Fish f)
    {
		if (f.HittableTypeS == "AreaBomb")
            return;

        //int scorePop;

        //FishEx_OddsMulti oddMultiComponent = f.GetComponent<FishEx_OddsMulti>();
        //if (oddMultiComponent != null)
        //    scorePop = b.Score * f.Odds * b.FishOddsMulti*oddMultiComponent.OddsMulti;
        //else
        //    scorePop = b.Score * f.Odds * b.FishOddsMulti;

        Popup(f.OddBonus * b.Score, f.transform.position, killer.transform, f.TypeIndex);
    }
    public void Popup(int num,Vector3 worldPos,Transform tsParent,int fishTypeIdx)
    {
        if (num == 0)
            return;
        StartCoroutine(_Coro_Popup(num, worldPos, tsParent, fishTypeIdx));
    }
    IEnumerator _Coro_Popup(int num, Vector3 worldPos, Transform tsParent, int fishTypeIdx)
    {
        //tk2dTextMesh digit = Instantiate(Prefab_Digit) as tk2dTextMesh;
        tk2dTextMesh digit = Pool_GameObj.GetObj(Prefab_Digit.gameObject).GetComponent<tk2dTextMesh>();
        digit.gameObject.SetActive(true);
        digit.text = num.ToString();
        digit.Commit();

        Transform ts = digit.transform;
        ts.parent = tsParent;
        ts.localRotation = Quaternion.identity;

        float scale = 1F;
        if (mScaleDatasMap.TryGetValue(fishTypeIdx, out scale))
            ts.localScale = new Vector3(scale, scale, 1F);
        else
            ts.localScale = Vector3.one;

        Vector3 oriPos = worldPos; 
        oriPos.z = Defines.GlobleDepth_DieFishPopDigit;
        ts.position = oriPos;

        
        float elapse = 0F;
        Color c = Prefab_Digit.color;
         
        while (elapse < Duration)
        {
            float prct = elapse / Duration;

            ts.position = oriPos + new Vector3(0F, Curve_OffsetY.Evaluate(prct), 0F);
            c.a = Curve_Alpha.Evaluate(prct);
            digit.color = c;
            digit.Commit();
            elapse += Time.deltaTime;
            yield return 0;
        }

        //Destroy(digit.gameObject);
        digit.gameObject.SetActive(false);
        Pool_GameObj.RecycleGO(Prefab_Digit.gameObject, digit.gameObject);
 
    }


    //private int upNum = 1;
    //void OnGUI()
    //{
    //    string numStr = GUILayout.TextArea(upNum.ToString());
    //    upNum = int.Parse(numStr);
    //    if (GUILayout.Button("Popup Digit"))
    //    {
    //        Popup(upNum, new Vector3(0F, 0F, -9F), GameMain.Singleton.Players[2].transform);
    //    }
    //}
}
