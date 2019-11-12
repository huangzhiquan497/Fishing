using UnityEngine;
using System.Collections;

public class mobileStart : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        foreach (Player p in GameMain.Singleton.Players)
        {
            p.CanCurrentScoreChange = false;
        }
	}
	
	// Update is called once per frame
    //void Update () {
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        if (Input.mousePosition)
    //        {
    //        }
    //    }
    //}
}
