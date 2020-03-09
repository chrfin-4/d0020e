using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NetworkingController : MonoBehaviourPunCallbacks
{
    //Variables
    public List <RoomInfo> rooms;
    private string roomName;
    public int inRoom = 0;
    private GameObject personCam;

    public GameObject eventSystem;

    private GameObject ClientPerson;
    
    public int usingVR;


    //Photon and unity Functions
    void Start()
    {
        usingVR = 0;
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
        Debug.Log("InLobby: "+ PhotonNetwork.InLobby.ToString() );
        transform.GetComponent<UI>().DisplayRooms(rooms);
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
        transform.GetComponent<UI>().DisplayRooms(rooms);
    }


    //On joined room destroy buttons and instantiates player prefab
    public override void OnJoinedRoom() 
    {
    	Debug.Log("Joined Room");
        inRoom = 1;
    	SpawnPerson();
        if(PhotonNetwork.IsMasterClient)
        {
            RoomSettings room = AppSettings.GetAppSettings().galleries["Test 2D and 3D"];
            ClientPerson.GetComponent<SerilazingArt>().ExportArt(room);
        }
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
            ClientPerson.GetComponent<Movement>().enabled = true;
            ClientPerson.GetComponent<Menu>().standbyCam = transform.GetComponent<UI>().WASDStandby.gameObject;
            personCam = ClientPerson.transform.Find("Main Camera").gameObject;
            personCam.SetActive(true);
            ClientPerson.transform.Find("Face").gameObject.SetActive(false);
            personCam.GetComponent<AudioListener>().enabled = true;
            
        }else
        {
            ClientPerson = PhotonNetwork.Instantiate("VRPerson", new Vector3(0,0,0), Quaternion.identity,0);
            ClientPerson.transform.Find("Capsule").gameObject.SetActive(false);
            personCam = ClientPerson.transform.Find("TrackingSpace").gameObject;
            personCam.SetActive(true);
            ClientPerson.transform.Find("Face").gameObject.SetActive(false);
            ClientPerson.GetComponent<Teleportation>().enabled = true;
            personCam.GetComponent<AudioListener>().enabled = true;
        }

        if(PhotonNetwork.IsMasterClient)
        {
           GameObject Hat = PhotonNetwork.Instantiate("ArtistHat", new Vector3(0,0,0), Quaternion.identity,0); 
           Hat.transform.SetParent(ClientPerson.transform, false);
           Hat.transform.position = new Vector3(0,0.7f,0);
        }
        transform.GetComponent<UI>().WASDStandby.gameObject.SetActive(false);
        transform.GetComponent<UI>().VRStandByCameraRig.gameObject.SetActive(false);


    }    

    void LeaveRoom()
    {

    }

}
