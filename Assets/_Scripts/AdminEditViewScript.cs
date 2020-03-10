using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EditSettings
{
	public Camera EditViewCamera { get;}
	public UnityAction<(HighlightEventType, GameObject)> EditListenerCall { get;}
	public GameObject UIButton;
	//public Camera EditViewCamera;
	public EditSettings(Camera camera, UnityAction<(HighlightEventType, GameObject)> call, GameObject button)
	{
		EditViewCamera = camera;
		EditListenerCall = call;
		UIButton = button;
	}
}

public class EditUI
{
	public Canvas EditCanvas;

	public Button SetArtOnSlotButton;
	public Button SaveMetaButton;

	public Text ArtTypeText;
	public Text ArtTitleText;
	public Text ArtistText;

	public EditUI(Canvas editCanvas)
	{
		EditCanvas = editCanvas;
		SetArtOnSlotButton = EditCanvas.transform.Find("SetArtOnSlotButton").GetComponent<Button>();
		
		Transform metadata = EditCanvas.transform.Find("Metadata");
		SaveMetaButton = metadata.Find("SaveMetaButton").GetComponent<Button>();
		ArtTypeText = metadata.Find("Type").Find("Value").GetComponent<Text>();
		ArtTitleText = metadata.Find("ArtTitle").Find("InputField").Find("Text").GetComponent<Text>();
		ArtistText = metadata.Find("Artist").Find("InputField").Find("Text").GetComponent<Text>();
	}

}

public class AdminEditViewScript : MonoBehaviour
{

    public GameObject Environment;
	public GameObject UIButton; // TODO Get this from UI script instead
	public EditUI EditingUI;
	private EditSettings Settings;
	//private Canvas EditCanvas;
	
	private GameObject _activeSlot; // XXX Appearently you have to do this for happiness.
	private GameObject ActiveSlot
	{
		get => _activeSlot;
		set
		{
			// De-highlight the slot:
			//		_activeSlot is null if no slot is selected
			//		value is null if we just deselected a slot
			if (_activeSlot != null && value != null)
				_activeSlot.GetComponent<PaintingSlotScript>().ResetHighlightColor();
			
			_activeSlot = value;
			EditingUI.EditCanvas.gameObject.SetActive(_activeSlot != null);
		}
	}
	
	// To easily swap out listener method
	//public void AdminEditListenerCall((HighlightEventType type, GameObject artSlot) ev) =>
	//	ArtSlotEvent(ev);

    //private List<GameObject> ArtSlotHighlights = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Vector3 roomPos = Environment.transform.position;
        transform.position = new Vector3(roomPos.x, 30.0f, roomPos.z);
        transform.rotation = Quaternion.Euler(90.0f, 0f, 0f);
		ActiveSlot = null;
    }

    void OnEnable()
    {
		Settings = new EditSettings(gameObject.GetComponent<Camera>(), ArtSlotEvent, UIButton);
		
		//EditCanvas = transform.Find("EditCanvas2").GetComponent<Canvas>();
		EditingUI = new EditUI(transform.Find("EditCanvas2").GetComponent<Canvas>());
		//EditCanvas.gameObject.SetActive(true);
		EditingUI.EditCanvas.worldCamera = gameObject.GetComponent<Camera>();
		

		EditingUI.SetArtOnSlotButton.onClick.AddListener(SetArtOnSlot);
		EditingUI.SaveMetaButton.onClick.AddListener(SaveMetadata);

        GameObject ArtSlots = Environment.transform.Find("ArtSlots").gameObject;
        ArtSlots.GetComponent<SlotMasterScript>().StartEditMode(Settings);
    }

    void ArtSlotEvent((HighlightEventType type, GameObject artSlot) ev)
    {
		switch (ev.type)
		{
			case HighlightEventType.ClickOn:
			{
				Debug.Log("Art slot " + ev.artSlot + " click on");
				SlotClickedOn(ev.artSlot);
				break;
			}
			
			case HighlightEventType.MouseEnter:
			{
				Debug.Log("Art slot " + ev.artSlot + " mouse enter");
				ev.artSlot.GetComponent<PaintingSlotScript>().SetHighlightColor(Color.yellow);
				break;
			}
			
			case HighlightEventType.MouseExit:
			{
				Debug.Log("Art slot " + ev.artSlot + " mouse exit");
				if (ActiveSlot != ev.artSlot)
					ev.artSlot.GetComponent<PaintingSlotScript>().ResetHighlightColor();
				break;
			}
		};
    }

	void SlotClickedOn(GameObject artSlot)
	{
		ActiveSlot = ActiveSlot != artSlot ? artSlot : null;
		//if (ActiveSlot == artSlot) // Deselect slot
		//{
		//	ActiveSlot = null;
		//}
		//else // Select slot
		//{
		//	ActiveSlot = artSlot;
		//}
	}
	
	
	void SetArtOnSlot()
	{
		Debug.Log("Hello guys.");
	}
	
	void SaveMetadata()
	{
		Debug.Log("Can't fool me twice");
	}
}