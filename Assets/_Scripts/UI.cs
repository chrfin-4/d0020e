using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class UI : MonoBehaviour
    {
    	public GameObject UIButton;
	    private List <GameObject> buttons = new List<GameObject>();
        public Canvas canvas;

	    public Camera WASDStandby;
	    public GameObject VRStandByCameraRig;

		private bool inSettings = false;

		private Vector3 roomButtonReferencePosition;
		private Vector3 muteButtonPosition;
		private Vector3 LeaveButtonPosition;
		private Vector3 SettingsButtonPosition;
		private Vector3 EditGalleryPosition;

    	void Start()
    	{
    		roomButtonReferencePosition = canvas.transform.position - new Vector3(20f, 10f, 0);
			muteButtonPosition = canvas.transform.position - new Vector3(-20f, 5f, 0);
			LeaveButtonPosition = canvas.transform.position - new Vector3(0f, 10f, 0);
			SettingsButtonPosition = canvas.transform.position - new Vector3(-20f, 10f, 0);
			EditGalleryPosition = canvas.transform.position - new Vector3(0f, -10f, 0f);
    	}

    	// Generic button creation with a buttontext and a Transform parent. Returns the buttonobject for user to add listners
	    public GameObject createButton(string buttonText, Transform parent, Vector3 positionVector){
	        GameObject buttonObject = Instantiate(UIButton);
	        Button button = (Button)buttonObject.GetComponent("Button");
	        var buttonTextChild = buttonObject.transform.GetChild(0);
	        Text buttonTextChildComponent = (Text)buttonTextChild.GetComponent("Text");
	        buttonTextChildComponent.text = buttonText;
	        buttonObject.transform.SetParent(parent, false);
            buttonObject.transform.position = positionVector;
			//Debug.Log("Position: " + buttonObject.transform.position.ToString() );

	        buttons.Add(buttonObject);
	        return buttonObject;
	    }

	    public void DisplayButtons(List <RoomInfo> rooms)
	    {
			Debug.Log("In Settings?: " + inSettings.ToString() );
	        for(int i = 0; i < canvas.transform.childCount; i++ )
	        {
	        	//Debug.Log(canvas.transform.GetChild(i).gameObject.ToString());
	            Destroy(canvas.transform.GetChild(i).gameObject );
	        }
	        //Debug.Log("InRoom: " + transform.GetComponent<NetworkingController>().inRoom.ToString() );
			//
			if(!inSettings)
			{
				GameObject SettingButton = createButton("Settings", canvas.transform, SettingsButtonPosition);
				((Button)SettingButton.GetComponent("Button")).onClick.AddListener(() => ToggleSettings() );
				if(transform.GetComponent<NetworkingController>().inRoom == 0)
				{
					
					//Instantiate(Cube, VRLeftStandby.gameObject.transform.position + new Vector3(20,20,20), Quaternion.identity);
					// CreateRoomButton("Create Button");
					GameObject createRoomButtonObject = createButton("Create Room", canvas.transform, roomButtonReferencePosition);
					((Button)createRoomButtonObject.GetComponent("Button")).onClick.AddListener(() => transform.GetComponent<NetworkingController>().CreatePhotonRoom());

					GameObject EditGallery = createButton("Edit Gallery", canvas.transform, EditGalleryPosition);
					//Få in editmode onclick
					int numberOfRooms = 0;
					if(rooms != null)
					{
						foreach(var room in rooms)
						{
							numberOfRooms += 1;
						}
						if(numberOfRooms != 0)
						{
							for(int i = 0; i < numberOfRooms; i++)
							{  
								Debug.Log( i.ToString() );
								GameObject JoinRoomButtonObject = createButton("Join Room " + rooms[i].Name, canvas.transform, new Vector3(roomButtonReferencePosition.x , roomButtonReferencePosition.y * 2*(i + 0.3f), roomButtonReferencePosition.z));
								RoomInfo room = rooms[i];
								((Button)JoinRoomButtonObject.GetComponent("Button")).onClick.AddListener(() => PhotonNetwork.JoinRoom(room.Name));
								// JoinRoomButton("Join Room " + rooms[i].Name, i);
							}
						}
					}
				}else{
					GameObject LeaveButton = createButton("Leave Room", canvas.transform, LeaveButtonPosition);
					((Button)LeaveButton.GetComponent("Button")).onClick.AddListener(() => PhotonNetwork.LeaveRoom() );
				}
			}else{
				if(transform.GetComponent<NetworkingController>().ClientPerson != null)
				{
					GameObject muteButton = createButton("Toggle Voice Chat", canvas.transform, muteButtonPosition);
					((Button)muteButton.GetComponent("Button")).onClick.AddListener(() => transform.GetComponent<NetworkingController>().ClientPerson.transform.GetComponent<VoiceChat>().Mute() );
				}
				GameObject BackFromSettingsButton = createButton("Back", canvas.transform, SettingsButtonPosition);
				((Button)BackFromSettingsButton.GetComponent("Button")).onClick.AddListener(() => ToggleSettings() );
			}
	    }


        //Setting up canvas which is necessary for world gui to work, essential for VR    
		private void ToggleSettings()
		{
			inSettings = !inSettings;
			DisplayButtons(transform.GetComponent<NetworkingController>().rooms);
		}

	    public void setupCanvas()
	    {
	        if(transform.GetComponent<NetworkingController>().usingVR == 1)
	        {
	            canvas.worldCamera = VRStandByCameraRig.transform.Find("TrackingSpace").transform.Find("CenterEyeAnchor").GetComponent<Camera>();
	        }else
	        {
	            canvas.worldCamera = WASDStandby;
	        }
	    }
    }