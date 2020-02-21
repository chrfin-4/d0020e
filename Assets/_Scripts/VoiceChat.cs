using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Realtime;

public class VoiceChat : MonoBehaviourPunCallbacks
{

    Recorder rec;
    AudioListener listener;

    bool audioSetupBool = true;

    private void Start()
    {
        rec = GetComponent("Recorder") as Recorder;
        listener = transform.Find("Main Camera").gameObject.GetComponent<AudioListener>();
    }

    private void Update() {
        if(rec && audioSetupBool)
        {
           audioSetup();
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            //Debug.Log("listener: " + listener.enabled.ToString() );
            Mute();

        }
    }

    void Mute()
    {
        if(listener.enabled)
        {
            listener.enabled = false;
        }
        else
        {
            listener.enabled = true;
        }
    }

    void audioSetup()
    {
        audioSetupBool = false;
        rec.TransmitEnabled = true;
        rec.DebugEchoMode = false;
    }
    
}
