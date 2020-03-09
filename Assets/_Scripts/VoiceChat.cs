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
    public Speaker listener;
    AudioSource aSource;

    bool audioSetupBool = true;

    private void Start()
    {
        listener = this.transform.gameObject.GetComponent<Speaker>();
        aSource = this.transform.gameObject.GetComponent<AudioSource>();
        audioSetup();
    }


    public void Mute()
    {
        Debug.Log("Togglar mute: " + listener.enabled.ToString() );
        if(listener.enabled)
        {
            listener.enabled = false;
            (GetComponent("Recorder") as Recorder).TransmitEnabled = false;
            GetComponent<AudioSource>().mute = true;
            aSource.enabled = false;
        }
        else
        {
            listener.enabled = true;
            (GetComponent("Recorder") as Recorder).TransmitEnabled = true;
            GetComponent<AudioSource>().mute = false;
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
