using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class Test_reboot : MonoBehaviour {
   

	// Update is called once per frame
    void OnGUI()
    {
        if (GUILayout.Button("reboot"))
        {
            win32Api.RebootSystem();
            
        }
    }
}
