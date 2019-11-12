using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Test_Probability : MonoBehaviour
{
    /// <summary>
    /// 测试 命中率偏移 与 挑战次数 的关系
    /// </summary>
    void Test_HitRatioBias()
    {
        int hit = 0;
        int numRnd = 1000;
        ulong biasTotal = 0;
        int timesBiasCacl = 10000;
        for (int j = 0; j != timesBiasCacl; j++)
        {

            for (int i = 0; i != numRnd; ++i)
            {
                if (Random.Range(0, 10000) < 1000)
                    ++hit;
            }

            biasTotal += (ulong)Mathf.Abs(hit - numRnd / 10);
            hit = 0;
        }

        Debug.Log("biasAvg = " + ((float)biasTotal / timesBiasCacl));
    }

    private List<GameObject> GosPot;
    void Test_FundWin()
    {
        if (GosPot != null)
        {
            foreach (GameObject go in GosPot)
                Destroy(go);
            GosPot.Clear();
        }
        else
        {
            GosPot = new List<GameObject>();
        }
        int fund = 10000;
        float viewOffsetX = 0F;
        for (int i = 0; i != 10000; ++i)
        {
            if (Random.Range(0, 2) == 0)
                fund += 1;
            else
                fund -= 1;

            if (i % 10 == 0)
            {
                GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pot.transform.position = new Vector3(++viewOffsetX, (fund - 10000), 0F);
                GosPot.Add(pot);
            }
        }
        //Debug.Log("fund = " + fund);
    } 

 
    void Test_FundWinAwaysWin()
    {

        //for (int j = 0; j != 100; ++j )
        {
            int fundOri = 10000;//原资产,你继续进行的条件
            int fund = fundOri; 

            int numWinFund = 10000; 
            ulong numLoseTime = 0;
            ulong numWinTime = 0;
            for (int j = 0; j != numWinFund; ++j)
            {
                fund = fundOri;
                while (true)
                {
                    if (Random.Range(0, 2) == 0)
                        fund += 1;
                    else
                        fund -= 1;

                    if (fund <= fundOri - 10)
                    {
                        ++numLoseTime;
                        break;
                    }

                    if (fund >= fundOri + 10)
                    {
                        ++numWinTime;
                        break;
                    }
 
                } 
            }

            Debug.Log("numWinFund=" + numWinFund + "   numWinTime=" + numWinTime + "   numLoseTime=" + numLoseTime);
            //Debug.Log("use Time = " + (winFundNeedTimeTotal / (ulong)numWinFund) + "   numFailTime = " + numFailTime);
        } 
    }


    public class FishSim
    {
        public FishSim(int odds) { Odds = odds;  }
        public int Odds = 2;
    }

    /// <summary>
    /// 测试gameOdds内的函数ishitInone
    /// </summary>
    void Test_GameOddsFunction_IsHitInOne()
    {
        float tarHitRate = 0.0002061856f;
        ulong numTar = 10000000;
        ulong numHitted = 0;
        for (ulong i = 0; i != numTar; ++i)
        {
            if (GameOdds.IsHitInOne(tarHitRate))
                ++numHitted;
        }

        Debug.Log("tarHItRate = " + tarHitRate + "   realHitRate = " + numHitted);

    }

    /// <summary>
    /// 测试鱼死亡几率 是否符合目标赔率
    /// </summary>
    void Test_FishDieRatio()
    { 
        ulong scoreUsed = 0;
        ulong scoreGetted = 0;
        ulong oneShootUse = 100;

        long ScoreInit = 50000000;
        long ScoreHaving = ScoreInit; 

        uint NumScoreInitX2 = 0;
        uint NumScoreZero = 0;

        //for (int numScoreTest = 0; numScoreTest != 100; ++numScoreTest)
        //{
            for (int i = 0; i != 10000000; ++i)//发射数目
            {
                //生成鱼
                FishOddsData fishFirst = new FishOddsData(0, 2);//Random.Range(2, 3) );
                List<FishOddsData> otherFish = new List<FishOddsData>();
                int numRndHitFish = 1;// Random.Range(0, 3);
                for (int j = 0; j != numRndHitFish; ++j)
                {
                    otherFish.Add(new FishOddsData(0, 50));// Random.Range(2, 3) ));
                }
                //allFishList.Add(new FishOddsData(2));
                //allFishList.Add(new FishOddsData(300)); 

                scoreUsed += oneShootUse;
                ScoreHaving -= (long)oneShootUse;
                List<FishOddsData> fishKilled = GameOdds.OneBulletKillFish((int)oneShootUse, fishFirst, otherFish);
                ulong curGainTotal = 0;
                foreach (FishOddsData fod in fishKilled)
                    curGainTotal += oneShootUse * (ulong)fod.Odds;

                scoreGetted += curGainTotal;
                ScoreHaving += (long)curGainTotal;

                //if (ScoreHaving >= ScoreInit * 2)
                //{
                //    //Debug.Log("ScoreHave >= 600000");
                //    ++NumScoreInitX2;
                //    break;
                //}
                //if (ScoreHaving <= 0)
                //{
                //    ++NumScoreZero;
                //    break;
                //}
            }
        //    ScoreHaving = ScoreInit;
        //}

        Debug.Log(string.Format("x2 = {0:d} zero = {1:d}", NumScoreInitX2, NumScoreZero));
        Debug.Log(string.Format("scoreUsed = {0:d}  scoreGetted =  {1:d}  SceneWin =  {2:d}  ScoreHaving = {3:d}", scoreUsed, scoreGetted, (long)(scoreUsed - scoreGetted), ScoreHaving));
    }


    /// <summary>
    ///  一子弹获得分值
    /// </summary>
    /// <returns>获得分值</returns>
    int _OneBulletGetScore(int bulletScore,FishSim fishFirst, List<FishSim> allFish)
    { 
        List<FishSim> fishDieList = new List<FishSim>();
        //鱼死亡   
        //排除第一条鱼外的鱼数目 
        //bool hitOnlyOne = allFish.Count == 1;//只击中一条鱼
 
        int oddsTotal = 0;
        foreach (FishSim f in allFish)
            oddsTotal += f.Odds;
        //第一条死亡几率
        //float firstDieRatio = (1F - GameOdds.GainRatio) * fishFirst.Odds / oddsTotal  / fishFirst.Odds;
        float firstDieRatio = (1F - GameOdds.GainRatio) * (fishFirst.Odds+oddsTotal) / (2F*oddsTotal *fishFirst.Odds);
        //Debug.Log("firstDieRatio = " + firstDieRatio + "    odds =" + fishFirst.Odds);
        if (GameOdds.IsHitInOne(firstDieRatio))//第一条鱼命中
        {
            fishDieList.Add(fishFirst);
            //Debug.Log("firstDieRatio = " + firstDieRatio+"    odds ="+fishFirst.Odds);
            //求其他鱼死是否死亡
            //先排除第一条鱼
            List<FishSim> fishOthers = new List<FishSim>();
            for (int i = 0; i != allFish.Count; ++i)
            {
                FishSim fo = allFish[i];
                if (fishFirst != fo )
                    fishOthers.Add(fo);
            }
 
            //逐条计算几率
            foreach (FishSim f in fishOthers)
            {
                //float dieRatio = (1F  - GameOdds.GainRatio - firstDieRatio * fishFirst.Odds) / (firstDieRatio * f.Odds * fishOthers.Count);
                //float dieRatio = (1F - GameOdds.GainRatio - firstDieRatio * fishFirst.Odds) / (firstDieRatio * oddsTotal);
                float dieRatio = (1F - GameOdds.GainRatio - firstDieRatio * fishFirst.Odds) / (firstDieRatio */* f.Odds **/ (oddsTotal - fishFirst.Odds));
                //Debug.Log("otherDieRatio = " + dieRatio + "    odds =" + f.Odds);
                if (GameOdds.IsHitInOne(dieRatio))
                    fishDieList.Add(f);
            }


        }
 
        int scoreTotalGetted = 0;
        foreach (FishSim fDie in fishDieList)
        {
            int numGetted = fDie.Odds * bulletScore;
            scoreTotalGetted += numGetted; 
        }
        return scoreTotalGetted;
    }

    void TestId()
    {
        byte dat = 0;
        for(int i = 0; i != 300; ++i)
        {
            if (i > 250)
                Debug.Log(dat);
            ++dat;
        }
    }


    //测试起伏几率
    void Test_WinProperation()
    {
        const int numTestOri = 10000;
        int numTest = numTestOri; //测试次数\
        const int scoreOri = 100;
        const int fishOdd = 100;

        int numPassTest = 0;
        while (numTest-- > 0)
        {
            
            //一次测试
            int score = scoreOri;
            int numTest2 = scoreOri;
            while (numTest2-- > 0 && score-- > 0)
            { 
                if (GameOdds.IsHitInOne2(1F / fishOdd))//打死一百倍鱼
                {
                    score += fishOdd;
                    //Debug.Log("hitted");
                }


                //if (score >= scoreOri + fishOdd * 10)
                //{
                //    ++numPassTest;
                //    break;
                //}
            }
            if (score >= scoreOri +   100)
            {
                ++numPassTest;
            }
        }


        Debug.Log("p = " + ((float)numPassTest / numTestOri));

    }

    /// <summary>
    /// 测试频率和概率关系
    /// </summary>
    void Test_PropAndFreque()
    {
        const int NumTest = 1000;
        int numTestCurrent = 0;
        int numHitted = 0;
        while (numTestCurrent++ < NumTest)
        {
            if (GameOdds.IsHitInOne2(0.1F))
            {
                ++numHitted;
            }
        }
        Debug.Log("hit rate = " + ((float)numHitted / NumTest));
    }
    void OnGUI()
    {
        if (GUILayout.Button("Test_PropAndFreque"))
        {
            Test_PropAndFreque();
        }

        //if (GUILayout.Button("Output GainRatio"))
        //{
        //    Debug.Log("Current GainRatio = " + GameOdds.GainRatio);
        //}
    }
}
