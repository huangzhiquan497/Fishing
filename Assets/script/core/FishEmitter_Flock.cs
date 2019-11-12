using UnityEngine;
using System.Collections;

public class FishEmitter_Flock : MonoBehaviour {

    public Fish Prefab_Fish;


    //public float AngleRndInit = 30F;//初始化角度(正负30度)
    public float Radius = 384F;//生成半径
    public int NumMax = 9;//最多生成个数
    public int NumMin = 4;//最少生成个数

    //void Awake()
    //{
    //    //if (GameMain.EvtStopGenerateFish != null)
    //    //    Debug.Log("s GameMain.EvtStopGenerateFish.length = " + GameMain.EvtStopGenerateFish.GetInvocationList().Length);
    //    GameMain.EvtStopGenerateFish += Handle_StopGenerateFish;
       
    //}
    //void OnDestroy()
    //{
    //    //Debug.Log("destroy");
        
    //    if(GameMain.EvtStopGenerateFish != null)
    //        Debug.Log("GameMain.EvtStopGenerateFish.length = " + GameMain.EvtStopGenerateFish.GetInvocationList().Length);
    //}

    //void Handle_StopGenerateFish()
    //{
    //    //StopCoroutine("_Coro_Generate");
    //    //GameMain.EvtStopGenerateFish -= Handle_StopGenerateFish;
    //    //Destroy(gameObject);
    //}

    //public void Generate()
    //{
    //    StartCoroutine("_Coro_Generate"); 
    //}

    public void Generate()
    {
        int num = Random.Range(NumMin, NumMax + 1);
        int generatedNum = 0;
        
        //yield return 0;//为取得transform.localPosition
        Swimmer leader = null;

        //Swimmer[] allSwimmer = new Swimmer[num];
        while (generatedNum < num)
        {
            Swimmer s = null;
            if (generatedNum == 0)
            {
                GameObject goLeader = new GameObject("领队_" + Prefab_Fish.name);
                
                leader = goLeader.AddComponent<Swimmer>();
                s = leader;
                if (GameMain.EvtLeaderInstance != null)
                    GameMain.EvtLeaderInstance(leader);

                goLeader.AddComponent<DestroyWhenSwimmerOut>();
                Prefab_Fish.swimmer.CopyDataTo(leader); 
            }
            else
            {
                Fish f = Instantiate(Prefab_Fish) as Fish;
                s = f.swimmer;
                f.name = Prefab_Fish.name;
                //删除所有其他ai
                f.ClearAI();
                FishAI_Flock aiFollow = f.gameObject.AddComponent<FishAI_Flock>();
                aiFollow.SetLeader(leader);
            }

            s.gameObject.AddComponent<FishDimenSetWhenEnterWorld>();

            //allSwimmer[generatedNum] = s; 
            s.SetLiveDimension(Radius / s.BoundCircleRadius * 2F);
            Transform tsSwimmer = s.transform;
            tsSwimmer.parent = GameMain.Singleton.FishGenerator.transform;
            Vector3 localPos = Random.insideUnitCircle*(Radius - s.BoundCircleRadius);
            localPos = transform.localPosition + localPos;
            localPos.z =   Defines.GMDepth_Fish+ GameMain.Singleton.FishGenerator.ApplyFishDepth();
            tsSwimmer.localPosition = localPos;

            tsSwimmer.localRotation = transform.localRotation/* * Quaternion.AngleAxis(Random.Range(-AngleRndInit,AngleRndInit),Vector3.forward)*/;
            //f.transform.right = Vector3.zero - transform.localPosition;
            s.Go();

            ++generatedNum;
            
            //yield return 0;//不连续创建
        }

        //int fishLiveDimSetted = 0;
        //while (fishLiveDimSetted < num)
        //{
        //    for (int i = 0; i != num;++i )
        //    {
        //        if (allSwimmer[i] != null && allSwimmer[i].IsInWorld())
        //        {
        //            allSwimmer[i].SetLiveDimension(Defines.ClearFishRadius);
        //            allSwimmer[i] = null;
        //            ++fishLiveDimSetted;
        //        }
        //    }
        //    yield return new WaitForSeconds(0.3F);
        //}
        //GameMain.EvtStopGenerateFish -= Handle_StopGenerateFish;
        Destroy(gameObject);
    }

}
