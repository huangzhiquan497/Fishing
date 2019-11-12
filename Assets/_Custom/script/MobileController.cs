using UnityEngine;
using System.Collections;

public class MobileController : MonoBehaviour {
    public tk2dUIItem Btn_Fire;
    public tk2dUIItem Btn_ChangeScore;
    public tk2dUIItem Btn_UpScore;
    public tk2dUI_DirectController DirectContrller;

    private bool mDirectCtrlling = false;
    //private float mPreAngle;
    private HpyInputKey mPreDirectState = HpyInputKey.BS_GameLite;
    //public tk2dButton
	// Use this for initialization
	void Start () {
        Btn_Fire.OnDown += Handle_FireDown;
        Btn_Fire.OnRelease += Handle_FireUp;

        Btn_ChangeScore.OnDown += Handle_ChangeScoreDown;
        Btn_ChangeScore.OnRelease += Handle_ChageScoreUp;

        Btn_UpScore.OnDown += Handle_ChangeUpScoreDown;
        Btn_UpScore.OnRelease += Handle_ChageUpScoreUp;

        tk2dUIItem uiItemdirCtrl = DirectContrller.GetComponent<tk2dUIItem>();
        if (uiItemdirCtrl != null)
        {
            uiItemdirCtrl.OnDown += Handle_DirectCtrlDown;
            uiItemdirCtrl.OnRelease += Handle_DirectCtrlUp;
        }

	}
    void Update()
    {
        if (mDirectCtrlling)
        {
            if (DirectContrller.Strength > 0.5F)
            {
                float currentAngle = -AngleDir(Vector3.up, DirectContrller.Direction, Vector3.forward) * Vector3.Angle(Vector3.up, DirectContrller.Direction);
                HpyInputKey curDirectState = HpyInputKey.BS_GameLite;
                currentAngle = (currentAngle + 360F) % 360F;
                //Debug.Log(currentAngle);
                if (currentAngle > 315F && currentAngle <= 45F)
                {
                    curDirectState = HpyInputKey.Up;
                }
                else if (currentAngle > 45F && currentAngle <= 135F)
                {
                    curDirectState = HpyInputKey.Right;
                }
                else if (currentAngle > 135F && currentAngle <= 225F)
                {
                    curDirectState = HpyInputKey.Down;
                }
                else if (currentAngle > 225F && currentAngle <= 315F)
                {
                    curDirectState = HpyInputKey.Left;
                }
                else
                {
                    curDirectState = HpyInputKey.BS_GameLite;
                }

                if (mPreDirectState != curDirectState)
                {
                    if (mPreDirectState != HpyInputKey.BS_GameLite)
                    {
                        GameMain.EvtInputKey(0, mPreDirectState, false);
                    }

                    if (curDirectState != HpyInputKey.BS_GameLite)
                    {
                        GameMain.EvtInputKey(0, curDirectState, true);
                    }
                }
                mPreDirectState = curDirectState;
                //Debug.Log(AngleDir(Vector3.up,DirectContrller.Direction,Vector3.forward)*Vector3.Angle(Vector3.up, DirectContrller.Direction));
            }
            else//ÔÚÈ¨ÖØ
            {
                if (mPreDirectState != HpyInputKey.BS_GameLite)
                {
                    GameMain.EvtInputKey(0, mPreDirectState, false);
                    mPreDirectState = HpyInputKey.BS_GameLite;
                }
            }
        }
    }
    void Handle_DirectCtrlDown()
    {
        mDirectCtrlling = true;
        
    }
    void Handle_DirectCtrlUp()
    {
        mDirectCtrlling = false;

        if (mPreDirectState != HpyInputKey.BS_GameLite)
        {
            GameMain.EvtInputKey(0, mPreDirectState, false);
        }
        mPreDirectState = HpyInputKey.BS_GameLite;
    }
    
    void Handle_FireDown()
    {
        GameMain.EvtInputKey(0, HpyInputKey.Fire, true);
    }

    void Handle_FireUp()
    {
        GameMain.EvtInputKey(0, HpyInputKey.Fire, false);
    }

    void Handle_ChangeScoreDown()
    {
        GameMain.EvtInputKey(0, HpyInputKey.Advance, true);
    }
    void Handle_ChageScoreUp()
    {
        GameMain.EvtInputKey(0, HpyInputKey.Advance, false);
    }

    void Handle_ChangeUpScoreDown()
    {
        GameMain.EvtInputKey(0, HpyInputKey.ScoreUp, true);
    }
    void Handle_ChageUpScoreUp()
    {
        GameMain.EvtInputKey(0, HpyInputKey.ScoreUp, false);
    }

    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }
}
