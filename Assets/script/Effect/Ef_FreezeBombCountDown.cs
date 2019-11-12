using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ef_FreezeBombCountDown : MonoBehaviour {

    public Vector3 LocalPos_CountDownNum = new Vector3(0.56F, 0.03F, -0.03F);
    public tk2dTextMesh Prefab_TextCountDown;

    private List<tk2dTextMesh> mViewingCountDownNum;

    public void Awake()
    {
        GameMain.EvtFreezeBombActive += Handle_FreezeAllFishBegin;
        //GameMain.EvtFreezeAllFishEnd += Handle_FreezeAllFishEnd;
        mViewingCountDownNum = new List<tk2dTextMesh>();
    }

    IEnumerator _Coro_CountdowningAndDestroyText()
    {
        float countDownTime = Defines.FreezeBombTime;
        while (countDownTime>0f)
        {
            foreach (tk2dTextMesh text in mViewingCountDownNum)
            {
                text.text = ((int)countDownTime).ToString();
                text.Commit();
            }

            countDownTime -= 1F;
            yield return new WaitForSeconds(1F);
        }
        
        //É¾³ýgameobject
        foreach (tk2dTextMesh text in mViewingCountDownNum)
        {
            Destroy(text.gameObject);
        }
        mViewingCountDownNum.Clear();
    }
    void Handle_FreezeAllFishBegin()
    {
        Player[] players = GameMain.Singleton.Players;
        for (int i = 0; i != players.Length; ++i)
        {
            if (players[i] == null)
                continue;

            tk2dTextMesh text_countDown = Instantiate(Prefab_TextCountDown) as tk2dTextMesh;
            

            text_countDown.transform.parent = players[i].transform;
            text_countDown.transform.localPosition = LocalPos_CountDownNum;
            text_countDown.transform.localRotation = Quaternion.identity;
            mViewingCountDownNum.Add(text_countDown);
        }

        StartCoroutine(_Coro_CountdowningAndDestroyText());
    }

    //void Handle_FreezeAllFishEnd()
    //{

    //}
}
