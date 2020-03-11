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

/*public class OptionMetadataEntry : UnityEngine.UI.OptionData
{
	
}*/

public class EditUI
{
	public Canvas EditCanvas;

	public Button SetArtOnSlotButton;
	public Button SaveMetaButton;
	public Button SaveGalleryButton;

	public Text ArtTypeText;
	public InputField ArtTitleText;
	public InputField ArtistText;
	public InputField GalleryNameText;
	
	public Dropdown ArtDropdown;

	public EditUI(Canvas editCanvas)
	{
		EditCanvas = editCanvas;
		
		SetArtOnSlotButton = EditCanvas.transform.Find("SetArtOnSlotButton").GetComponent<Button>();
		SaveGalleryButton = EditCanvas.transform.Find("SaveGalleryButton").GetComponent<Button>();
		
		ArtDropdown = EditCanvas.transform.Find("ArtDropdown").GetComponent<Dropdown>();
		GalleryNameText = EditCanvas.transform.Find("GalleryName").GetComponent<InputField>();
		
		Transform metadata = EditCanvas.transform.Find("Metadata");
		SaveMetaButton = metadata.Find("SaveMetaButton").GetComponent<Button>();
		ArtTypeText = metadata.Find("Type").Find("Value").GetComponent<Text>();
		ArtTitleText = metadata.Find("ArtTitle").Find("InputField").GetComponent<InputField>();
		ArtistText = metadata.Find("Artist").Find("InputField").GetComponent<InputField>();
	}

}

public class AdminEditViewScript : MonoBehaviour
{

    public GameObject Environment;
	public GameObject UIButton; // TODO Get this from UI script instead
	public EditUI EditUI;
	private RoomSettings Gallery;
	//private Canvas EditCanvas;
	
	private List<ArtMetaData> MetadataList;
	
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
			EditUI.EditCanvas.gameObject.SetActive(_activeSlot != null);
		}
	}
	
	private ArtMetaData Metadata;
	
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
		Gallery = new RoomSettings();
		
		//EditCanvas = transform.Find("EditCanvas2").GetComponent<Canvas>();
		EditUI = new EditUI(transform.Find("EditCanvas2").GetComponent<Canvas>());
		//EditCanvas.gameObject.SetActive(true);
		EditUI.EditCanvas.worldCamera = gameObject.GetComponent<Camera>();

		EditUI.SetArtOnSlotButton.onClick.AddListener(SetArtOnSlot);
		EditUI.SaveMetaButton.onClick.AddListener(SaveMetadata);

		MetadataList = AppSettings.GetAppSettings().ArtRegistry.GetAll();
		EditUI.ArtDropdown.AddOptions(MetadataList.Map(a => a.ArtTitle));

		EditUI.SaveGalleryButton.onClick.AddListener(SaveGallery);
		
		EditUI.ArtDropdown.onValueChanged.AddListener(ViewMetadata);
		
		

        GameObject ArtSlots = Environment.transform.Find("ArtSlots").gameObject;
        ArtSlots.GetComponent<SlotMasterScript>().StartEditMode(ArtSlotEvent);
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
		int slotNumber = ActiveSlot.GetComponent<PaintingSlotScript>().Number;
		Gallery.Slots[slotNumber] = new SlotSettings(slotNumber, Metadata);
		Debug.Log("Hello guys.");
		Debug.Log(Gallery.Slots.Count);
	}
	
	void SaveMetadata()
	{
		Debug.Log("Can't fool me twice");
	}
	
	void SaveGallery()
	{
		string galleryName = EditUI.GalleryNameText.text;
		AppSettings.GetAppSettings().galleries[galleryName] = Gallery;
		AppSettings.GetAppSettings().Save();
		Debug.Log("Saved the room " + galleryName);
	}
	
	void ViewMetadata(int i)
	{
		Metadata = MetadataList[i];
		EditUI.ArtTypeText.text = Metadata.Type.ToString();
		EditUI.ArtTitleText.text = Metadata.ArtTitle;
		EditUI.ArtistText.text = Metadata.ArtistName;
	}
	
}
