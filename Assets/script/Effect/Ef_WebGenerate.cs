using UnityEngine;
using System.Collections;

public class Ef_WebGenerate : MonoBehaviour {
    public WebDatas Prefab_WebDataNormal;
    public WebDatas Prefab_WebDataLizi;

    public ColorSet Prefab_WebColorNormal;
	// Use this for initialization
	void Awake () {
        GameMain.EvtBulletDestroy += Handle_BulletDestroy;
	}

    void Handle_BulletDestroy(Bullet b)
    {
        /// 1.使用webData 来区分网
        /// 2.使用FishOddsMulti来区分离子炮
        bool isLizi = b.FishOddsMulti != 2 ? false : true;
        WebDatas wdToIteration = isLizi ? Prefab_WebDataLizi : Prefab_WebDataNormal;
        
        WebScoreScaleRatio useWebData = null;

       
        for (int i = 0; i != wdToIteration._WebDatas.Length; ++i)
        {
            if (b.Score <= wdToIteration._WebDatas[i].StartScore)
            {
                useWebData = wdToIteration._WebDatas[i];
                break;
            }
        } 

        if (useWebData == null)
            useWebData = new WebScoreScaleRatio();//默认值

        CreateWeb(b, useWebData, isLizi);
    }


    void CreateWeb(Bullet b, WebScoreScaleRatio webData,bool isLizi)
    {
        GameObject goWebBoom = Instantiate(webData.PrefabWebBoom) as GameObject;
        goWebBoom.transform.parent = transform;

        Ef_WebBubble efBubble = goWebBoom.GetComponent<Ef_WebBubble>();
        if (efBubble != null)
        {
            efBubble.ScaleTarget = webData.BubbleScale;
        } 

        Ef_WebBoom[] efWebs = goWebBoom.GetComponentsInChildren<Ef_WebBoom>();
        foreach (Ef_WebBoom efWeb in efWebs)
        {
            efWeb.Prefab_GoSpriteWeb = webData.PrefabWeb;
            efWeb.NameSprite = webData.NameSprite;
            //Debug.Log(webData.NameSprite);
            efWeb.ScaleTarget = webData.Scale;
            efWeb.transform.localPosition *= webData.PositionScale;
            if(!isLizi)
                efWeb.ColorInitialize = Prefab_WebColorNormal.Colors[b.Owner.Idx % Prefab_WebColorNormal.Colors.Length];
        }

        Transform tsWeb = goWebBoom.transform;
        Transform tsBullet = b.transform;
        tsWeb.position = new Vector3(tsBullet.position.x, tsBullet.position.y, Defines.GlobleDepth_Web);
        tsWeb.rotation = tsBullet.rotation;
    }


}
