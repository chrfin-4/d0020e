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


    	void Start()
    	{
    		
    	}

    	void Update(){

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

	        buttons.Add(buttonObject);
	        return buttonObject;
	    }

	    public void DisplayRooms(List <RoomInfo> rooms)
	    {
	        for(int i = 0; i < canvas.transform.childCount; i++ )
	        {
	        	Debug.Log(canvas.transform.GetChild(i).gameObject.ToString());
	            Destroy(canvas.transform.GetChild(i).gameObject );
	        }
	        
	        if(transform.GetComponent<NetworkingController>().inRoom == 0)
	        {
	            //Instantiate(Cube, VRLeftStandby.gameObject.transform.position + new Vector3(20,20,20), Quaternion.identity);
	            // CreateRoomButton("Create Button");
	            GameObject createRoomButtonObject = createButton("Create Room", canvas.transform, new Vector3(80.0f , 60.0f, 0.0f));
	            ((Button)createRoomButtonObject.GetComponent("Button")).onClick.AddListener(() => transform.GetComponent<NetworkingController>().CreatePhotonRoom());
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
	                        GameObject JoinRoomButtonObject = createButton("Join Room " + rooms[i].Name, canvas.transform, new Vector3(80.0f , 30.0f * (i + 3.0f), 0.0f));
	                        RoomInfo room = rooms[i];
	                        ((Button)JoinRoomButtonObject.GetComponent("Button")).onClick.AddListener(() => PhotonNetwork.JoinRoom(room.Name));
	                        // JoinRoomButton("Join Room " + rooms[i].Name, i);
	                    }
	                }
	            }
	        }
	    }

        //Setting up canvas which is necessary for world gui to work, essential for VR    
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