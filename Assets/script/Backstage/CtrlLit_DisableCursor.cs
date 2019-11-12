using UnityEngine;
using System.Collections;

public class CtrlLit_DisableCursor : MonoBehaviour {


    void OnEnable()
    {
        BackstageMain.Singleton.Cursor.gameObject.SetActiveRecursively(false);
    }

    void OnDisable()
    {
        BackstageMain.Singleton.Cursor.gameObject.SetActiveRecursively(true);
    }
   
}
