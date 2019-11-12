using UnityEngine;
using System.Collections;

public class Test_ViewFishNum : MonoBehaviour {
    public Rect windowRect = new Rect(20, 20, 120, 50);
    //GameMain mGM;

    //// Use this for initialization
    //void Awake()
    //{
    //    mGM = GameMain.Singleton;
    //}

    void OnGUI()
    {
        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "”„ ˝¡ø");


    }

    void DoMyWindow(int windowID)
    {
        GUILayout.TextArea(GameMain.Singleton.NumFishAlive.ToString());

        GUI.DragWindow();
    }
}
