using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour {

	private bool menuToggle = false;

	public GameObject playerCam;
	public GameObject menuGroup;
	public Movement moveScript;
	public Teleportation teleportation;
	private Transform oldMusicLocation;
	private GameObject backgroundMusic;

	void Start(){
		//Debug.Log("StandbyCam: " + standbyCam.ToString() );
		backgroundMusic = GameObject.Find("BackgroundMusic");
	}

	void FixedUpdate(){
		if( Input.GetKeyDown(KeyCode.M) || OVRInput.GetDown(OVRInput.Button.Two) ){
			ToggleMenu();
		}
	}

	void ToggleMenu()
	{
		menuToggle = menuToggle == true ? menuToggle = false : menuToggle = true;

		//Debug.Log("teleportation enabled: " + GetComponent<Teleportation>().enabled.ToString() );
		//Debug.Log("StandbyCam: " + menuGroup.ToString() );
		if(menuToggle)
		{
			Cursor.visible = true;
			menuGroup.SetActive(true);
			playerCam.SetActive(false);
			oldMusicLocation = backgroundMusic.transform.parent.transform;
            backgroundMusic.transform.SetParent(menuGroup.transform);

			if(teleportation != null)
			{
				//menuGroup.transform.Find("OVRCameraRig").GetComponent<OVRSystemPrefMetrics/OVRSystemPerfMetricsTcpServer>().enabled = true;
				GetComponent<Teleportation>().enabled = false;
				//GetComponent<OVRManager>().enabled = false;
				//GetComponent<OVRCameraRig>().enabled = false;
			}
			if(moveScript != null)
			{
				GetComponent<Movement>().enabled = false;
			}
		}else
		{
			Cursor.visible = false;
            backgroundMusic.transform.SetParent(oldMusicLocation);
			menuGroup.SetActive(false);
			playerCam.SetActive(true);

			if(teleportation != null)
			{
				//menuGroup.transform.Find("OVRCameraRig").GetComponent<OVRSystemPrefMetrics>().enabled = true;
				GetComponent<Teleportation>().enabled = true;
				//GetComponent<OVRManager>().enabled = true;
				//GetComponent<OVRCameraRig>().enabled = true;
			}
			if(moveScript != null)
			{
				GetComponent<Movement>().enabled = true;
			}
		}
		
	}
}