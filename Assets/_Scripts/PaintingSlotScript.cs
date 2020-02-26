using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Settings.;

public class PaintingSlotScript : MonoBehaviour
{
    public SlotSettings Settings;
    private bool a = false;

    void Update()
    {
        if (!a)
        {
            a = true;
            Debug.Log(Settings.SlotNumber);
        }
    }

}
