using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Realtime;

public class VoiceChat : MonoBehaviourPunCallbacks
{

    Recorder rec = null;
    Speaker listener;
    AudioSource aSource;

    bool audioSetupBool = true;

    private void Start()
    {
        listener = this.transform.gameObject.GetComponent<Speaker>();
        aSource = this.transform.gameObject.GetComponent<AudioSource>();
    }

    private void Update() {
        if(audioSetupBool)
        {
            Debug.Log("Audio Setup");
            audioSetup();
        }
        if(Input.GetKeyDown(KeyCode.L) || OVRInput.Get(OVRInput.Button.One))
        {
            Mute();
            Debug.Log("listener: " + listener.enabled.ToString() );

        }
    }

    void Mute()
    {
        if(listener.enabled)
        {
            listener.enabled = false;
            (GetComponent("Recorder") as Recorder).TransmitEnabled = false;
            aSource.enabled = false;
        }
        else
        {
            listener.enabled = true;
            (GetComponent("Recorder") as Recorder).TransmitEnabled = true;
            aSource.enabled = true;
        }
    }

    void audioSetup()
    {
        audioSetupBool = false;
        (GetComponent("Recorder") as Recorder).TransmitEnabled = true;
        (GetComponent("Recorder") as Recorder).DebugEchoMode = false;

    }
    
}
