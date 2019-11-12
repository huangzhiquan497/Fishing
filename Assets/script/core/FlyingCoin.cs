using UnityEngine;
using System.Collections;

public class FlyingCoin : MonoBehaviour,IPoolObj{

    public float FlySpeed = 1F;
    GameObject mPrefab;
    private tk2dSpriteAnimator mAniSprCoin;
    public GameObject Prefab
    {
        get { return mPrefab; }
        set { mPrefab = value; }

    }
    void Awake()
    {
        mAniSprCoin = GetComponent<tk2dSpriteAnimator>();
        //mAniSprCoin.playAutomatically = false;
    }
    //void Start()
    //{
    //    mAniSprCoin.Play();
    //}
    /// <summary>
    /// 重用
    /// </summary>
    public void On_Reuse(GameObject prefab)
    {
        //mAniSprCoin.clipTime = 0F;
        
        
        gameObject.SetActive(true);
        mAniSprCoin.PlayFrom(mAniSprCoin.DefaultClip, 0);
    }

    /// <summary>
    /// 回收
    /// </summary>
    public void On_Recycle()
    {
        mAniSprCoin.Stop();
        gameObject.SetActive(false);
    }

    public float FlytoPosZero(AnimationCurve scaleCurve)
    {
        Vector3 flyLocalDirect = Vector3.zero - transform.localPosition;
        float flyDistance = flyLocalDirect.magnitude;
        flyLocalDirect.Normalize();

        float useTime = flyDistance / FlySpeed + 0.1F;
        StartCoroutine(_Coro_FlyProcess(scaleCurve, useTime, flyLocalDirect));
        return useTime;

    }
    IEnumerator _Coro_FlyProcess(AnimationCurve scaleCurve, float useTime, Vector3 flyLocalDirect)
    {

        float curTime = 0F;
        while (curTime < useTime)
        {
            transform.localPosition += flyLocalDirect * FlySpeed * Time.deltaTime;
            curTime += Time.deltaTime;
            float scale = scaleCurve.Evaluate(curTime / useTime);
            transform.localScale = new Vector3(scale, scale, 1F);
            yield return 0;
        }

    }
}
