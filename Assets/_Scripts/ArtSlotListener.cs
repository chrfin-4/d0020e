using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This class (with type param) required to retrieve GameObject on callback
public class ClickedOnEvent : UnityEvent<GameObject> {}

public class ArtSlotListener : MonoBehaviour
{

    public ClickedOnEvent ev = new ClickedOnEvent();

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    void OnMouseDown()
    {
        ev.Invoke(gameObject);
    }

}
