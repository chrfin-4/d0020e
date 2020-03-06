using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour {

	private bool menuToggle = true;

	public GameObject playerCam;
	public GameObject standbyCam;
	public Movement moveScript;
	public Teleportation teleportation;

	void Start(){
		Debug.Log("StandbyCam: " + standbyCam.ToString() );
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.M) || OVRInput.Get(OVRInput.Button.Two)){
			ToggleMenu();
			Debug.Log("Togglar menu");
		}
	}

	void ToggleMenu()
	{
		if(menuToggle)
		{
			Cursor.visible = true;
			playerCam.SetActive(false);
			standbyCam.SetActive(true);
			if(teleportation != null)
			{
				teleportation.enabled = false;
			}if(moveScript != null)
			{
				moveScript.enabled = false;
			}
		}else
		{
			Cursor.visible = false;
			playerCam.SetActive(true);
			standbyCam.SetActive(false);
			if(teleportation != null)
			{
				teleportation.enabled = true;
			}if(moveScript != null)
			{
				moveScript.enabled = true;
			}
		}
		menuToggle = !menuToggle;
	}
}