using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class NetworkingController : MonoBehaviourPunCallbacks
{
    //Variables
    public List <RoomInfo> rooms;
    private string roomName;
    public int inRoom = 0;
    private GameObject personCam;

    public GameObject eventSystem;

    public GameObject ClientPerson;
    public GameObject menuGroup;
    
    public int usingVR;


    //Photon and unity Functions
    void Start()
    {
        usingVR = XRDevice.isPresent ? usingVR = 1 : usingVR = 0;
        //usingVR = 1;
        VRCheck();
        transform.GetComponent<UI>().setupCanvas();
        Connect();
    }

    void Connect() //Connect To Photon server via AppID ('UsingSettings')
    {
    	Debug.Log("Connecting to master server");
    	PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
    	Debug.Log("Connected to master server");
    	PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        //Debug.Log("InLobby: "+ PhotonNetwork.InLobby.ToString() );
        
    }

    public override void OnJoinRandomFailed(short returncode, string message) //No rooms are visible or available
    {
    	Debug.Log("Could not join any room, reason: " + returncode.ToString());
    	CreatePhotonRoom();
    }

    void OnCreatePhotonRoomFailed()
    {
        Debug.Log("Failed to create room, trying again");
        CreatePhotonRoom();
    }

    public override void OnRoomListUpdate(List <RoomInfo> roomList)
    {
        rooms = roomList;
        for(int i = 0; i < rooms.Count; i++)
        {
            Debug.Log("RoomListUpdate: " + rooms[i].ToString() );
        }
        /*
        for(int i = 0; i < roomList.Count; i++)
        {
            if(UpdateRoom(rooms, roomList[i]))
            {
                rooms.Remove(roomList[i]);
            }else{
                rooms.Add(roomList[i]);
            }
        }
        */
        transform.GetComponent<UI>().DisplayButtons(rooms);
    }


    //On joined room destroy buttons and instantiates player prefab
    public override void OnJoinedRoom() 
    {
    	inRoom = 1;
    	//Debug.Log("Joined Room, inroom " + inRoom.ToString() );
        SpawnPerson();
        transform.GetComponent<UI>().DisplayButtons(rooms);
        if(PhotonNetwork.IsMasterClient)
        {
            // FIXME:
            //    Should not use a hard-coded gallery name.
            //    Should not rearrange slots here. (Fix the gallery and/or art slots in the scene.)
            RoomSettings room = AppSettings.GetAppSettings().galleries["Test 2D and 3D"];
            room.Slots[5] = room.Slots[0];
            room.Slots.Remove(0);
            ClientPerson.GetComponent<SerilazingArt>().ExportArt(room);
        }
    }
    public override void OnLeftRoom()
    {
        inRoom = 0;
        transform.GetComponent<UI>().DisplayButtons(rooms);
    }
    public override void OnPlayerEnteredRoom(Player newplayer)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("New Player: " + newplayer.ToString() );
        }
        
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
    	Debug.Log("Disconnected from server, reason: " + cause.ToString());
    }




    //Other Functions
    void VRCheck()
    {
        eventSystem.GetComponent<OVRInputModule>().enabled = false;
        eventSystem.GetComponent<StandaloneInputModule>().enabled = false;
        transform.GetComponent<UI>().canvas.GetComponent<OVRRaycaster>().enabled = false;
        transform.GetComponent<UI>().canvas.GetComponent<GraphicRaycaster>().enabled = false;
        if(usingVR == 1)
        {
            transform.GetComponent<UI>().VRStandByCameraRig.gameObject.SetActive(true);
            eventSystem.GetComponent<OVRInputModule>().enabled = true;
            transform.GetComponent<UI>().canvas.GetComponent<OVRRaycaster>().enabled = true;
        }else
        {
            Debug.Log("Not using VR");
            transform.GetComponent<UI>().WASDStandby.gameObject.SetActive(true);
            transform.GetComponent<UI>().canvas.GetComponent<GraphicRaycaster>().enabled = true;
            eventSystem.GetComponent<StandaloneInputModule>().enabled = true;
        }
    }

    bool UpdateRoom(List <RoomInfo> rooms, RoomInfo room)
    {
        for(int i = 0; i < rooms.Count; i++)
        {
            if(rooms[i] == room)
            {
                return true;
            }
        }
        return false;
    }
    public void CreatePhotonRoom()
    {
    	RoomOptions options = new RoomOptions() {IsVisible = true, IsOpen = true, MaxPlayers = 10};
        roomName = "Room " + Random.Range(0,10000);
        PhotonNetwork.CreateRoom(roomName, options);
    	Debug.Log("Created a new Photon Room in Lobby: ");// + PhotonNetwork.lobby.ToString());
    }

    void SpawnPerson() //Spawn person and activating movement script and main camera locally
    {
        if(usingVR == 0)
        {
    	    ClientPerson = PhotonNetwork.Instantiate("Person", Vector3.zero, Quaternion.identity, 0);

            GameObject.Find("BackgroundMusic").transform.SetParent(ClientPerson.transform);

            ClientPerson.GetComponent<Movement>().enabled = true;
            ClientPerson.GetComponent<Menu>().enabled = true;
            ClientPerson.GetComponent<VoiceChat>().enabled = true;
            //ClientPerson.GetComponent<Menu>().standbyCam = transform.GetComponent<UI>().WASDStandby.gameObject;
            personCam = ClientPerson.transform.Find("Main Camera").gameObject;
            personCam.SetActive(true);
            ClientPerson.transform.Find("Face").gameObject.SetActive(false);
            
        }else
        {
            ClientPerson = PhotonNetwork.Instantiate("VRPerson", new Vector3(0,0,0), Quaternion.identity,0);

            GameObject.Find("BackgroundMusic").transform.SetParent(ClientPerson.transform);

            ClientPerson.transform.Find("Capsule").gameObject.SetActive(false);
            ClientPerson.GetComponent<Menu>().enabled = true;
            ClientPerson.GetComponent<VoiceChat>().enabled = true;
            //ClientPerson.GetComponent<Menu>().standbyCam = transform.GetComponent<UI>().VRStandByCameraRig.gameObject;
            personCam = ClientPerson.transform.Find("TrackingSpace").gameObject;
            personCam.SetActive(true);
            ClientPerson.transform.Find("Face").gameObject.SetActive(false);
            ClientPerson.GetComponent<Teleportation>().enabled = true;
        }

        if(PhotonNetwork.IsMasterClient)
        {
           GameObject Hat = PhotonNetwork.Instantiate("ArtistHat", new Vector3(0,0,0), Quaternion.identity,0); 
           Hat.transform.SetParent(ClientPerson.transform, false);
           Hat.transform.position = new Vector3(0,0.7f,0);
        }
        ClientPerson.GetComponent<Menu>().menuGroup = menuGroup;
        ClientPerson.GetComponent<Menu>().menuGroup.SetActive(false);
        //transform.GetComponent<UI>().WASDStandby.gameObject.SetActive(false);
        //transform.GetComponent<UI>().VRStandByCameraRig.gameObject.SetActive(false);


    }    

    void LeaveRoom()
    {

    }

}
