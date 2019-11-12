using UnityEngine;
using System.Collections;

public class CanFire : MonoBehaviour 
{
    public bool fingerDown = false;
	// Use this for initialization
	void Start ()
    {
	
	}

    void OnMouseDown()
    {
        fingerDown = true;
       // if (Input.GetMouseButtonDown(0))
        {
            MobileInterface.Player_StartFire();
        }

        if (MobileInterface.GetPlayerScore() == 0)
        { 
            if (Moudle_main.EvtShop != null)
                Moudle_main.EvtShop();
        }
    }
    void OnMouseExit()
    {
       // Debug.Log("离开碰撞框");
       // fingerDown = false;
       // MobileInterface.Player_StopFire();
    }
    void OnMouseUp()
    {
        fingerDown = false;
        //if (Input.GetMouseButtonUp(0))
        {
            MobileInterface.Player_StopFire();
        }
    }
	// Update is called once per frame

	void Update ()
    {
        if (fingerDown)
        {
           // if (Input.GetMouseButton(0))
            {
                Vector3 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (worldpos.y < -340)
                    return;
                MobileInterface.Player_Aim(worldpos);
            }
        }

	}
}
