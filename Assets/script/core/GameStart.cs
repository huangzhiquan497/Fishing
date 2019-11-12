using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Security.Cryptography;
public class GameStart : MonoBehaviour {
 
    public Event_Generic EvtGameInitFinish;//��Ϸ��ʼ������

    public float TimeLoadTexture = 5.2F;//������ͼƬ��Դʹ��ʱ��
    public GameStartOneScreen Prefab_GameStartView;
    private int mGameStartFinishedNum = 0;//�����ɵ�gamestart��ʾ��Ŀ

    void Awake()
    {
        //HMACMD5 mHmacMd5 = new HMACMD5(System.Text.Encoding.Unicode.GetBytes("yidingyaochang")); 
        //byte[] pswEncrypted = mHmacMd5.ComputeHash(System.Text.Encoding.Unicode.GetBytes("a"));

        //Debug.Log(pswEncrypted.Length);

 
        Rect worldDim = GameMain.Singleton.WorldDimension;
        GetComponent<Camera>().enabled = true;
        for (int i = 0; i != GameMain.Singleton.ScreenNumUsing; ++i)
        {
            GameStartOneScreen gso = Instantiate(Prefab_GameStartView) as GameStartOneScreen;
            gso.transform.parent = transform;
            gso.transform.localPosition = new Vector3(worldDim.x + worldDim.width * 0.5F * (1F / GameMain.Singleton.ScreenNumUsing + i), 0F, 0F);
            gso.IdxScreen = i;

            //if (i % 2 == 1)//��תͼ��
            //{
            //    for (int Uin = 0; Uin < gso.Rdr_GameIcon.Length; Uin++)
            //    {
            //        Vector3 tmpScale = gso.Rdr_GameIcon[Uin].transform.localScale;
            //        tmpScale.x = -tmpScale.x;
            //        gso.Rdr_GameIcon[Uin].transform.localScale = tmpScale;
            //    }
            //}
        }

    }
 

	// Use this for initialization
	IEnumerator Start () {
        //Ԥ������ͼƬ
        Fish[] prefabFishAll = GameMain.Singleton.FishGenerator.Prefab_FishAll;
        float oneFishUseTime = TimeLoadTexture / prefabFishAll.Length;
        foreach (Fish f in prefabFishAll)
        {
            Fish fishLoaded = Instantiate(f) as Fish;
 
			yield return 0;
            //Fish fishLoaded = Pool_GameObj.GetObj(f.gameObject).GetComponent<Fish>();
            fishLoaded.transform.parent = transform;
            //yield return new WaitForSeconds(oneFishUseTime*0.5F);
            fishLoaded.Clear();
            yield return new WaitForSeconds(oneFishUseTime);
        }


        
	}


    void Msg_GameInitFinish()
    {
        ++mGameStartFinishedNum;
        if (mGameStartFinishedNum == GameMain.Singleton.ScreenNumUsing)
        {
            Destroy(gameObject);
            if (EvtGameInitFinish != null)
                EvtGameInitFinish();
        }
    }
	 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(gameObject);
            if (EvtGameInitFinish != null)
                EvtGameInitFinish();
            
        }
    }
}
