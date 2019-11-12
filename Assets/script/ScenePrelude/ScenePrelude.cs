using UnityEngine;
using System.Collections;

public class ScenePrelude : MonoBehaviour
{
    
    public Event_Generic Evt_PreludeEnd;

    public virtual void Go() { }

    public virtual void Pause() { }
    public virtual void Resume() { }

}
