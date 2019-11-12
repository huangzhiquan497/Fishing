using UnityEngine;
using System.Collections;

public class Module_FishTypeBomb2 : MonoBehaviour {
      
    void Start()
    {
        HitProcessor.AddFunc_Odd(Func_GetFishOddAdditive, Func_GetFishOddAdditive);
    }


    HitProcessor.OperatorOddFix Func_GetFishOddAdditive(Player killer, Bullet b, Fish f, Fish fCauser)
    {
        if (fCauser.HittableTypeS == "SameTypeBomb")//触发者是同类炸弹.他所触发的小鱼,即使没有带FishEx_OddsMulti,也需要乘倍
        {
            FishEx_OddsMulti cpOddMulti = fCauser.GetComponent<FishEx_OddsMulti>();

            if (cpOddMulti == null || cpOddMulti.OddsMulti == 1)
                return null;

            return new HitProcessor.OperatorOddFix(HitProcessor.Operator.LastModule, cpOddMulti.OddsMulti);
        }
        else if (f.HittableTypeS == "SameTypeBomb")//有可能出现causer是SameTypeBombEx,因为SameTypeBombEx会是SameTypeBomb的触发者
        {
            FishEx_OddsMulti cpOddMulti = f.GetComponent<FishEx_OddsMulti>();

            if (cpOddMulti == null || cpOddMulti.OddsMulti == 1)
                return null;

            return new HitProcessor.OperatorOddFix(HitProcessor.Operator.LastModule, cpOddMulti.OddsMulti);
        }

        return null;
        
    }
}
