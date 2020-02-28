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

	void Start(){
		Debug.Log("StandbyCam: " + standbyCam.ToString() );
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.M)){
			ToggleMenu();
			Debug.Log("Togglar menu");
		}
	}

	void ToggleMenu()
	{
		if(menuToggle)
		{
			playerCam.SetActive(false);
			standbyCam.SetActive(true);
			moveScript.enabled = false;
		}else
		{
			playerCam.SetActive(true);
			standbyCam.SetActive(false);
			moveScript.enabled = true;
		}
		menuToggle = !menuToggle;
	}
}