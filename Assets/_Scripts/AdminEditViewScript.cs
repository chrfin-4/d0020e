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

public class AdminEditViewScript : MonoBehaviour
{

    public GameObject Environment;
	public GameObject UIButton; // TODO Get this from UI script instead
	private EditSettings Settings;
	private Canvas EditCanvas;
	private GameObject ActiveSlot;
	
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
		
		EditCanvas = Environment.transform.Find("EditCanvas2").GetComponent<Canvas>();
		EditCanvas.worldCamera = gameObject.GetComponent<Camera>();
		
        GameObject ArtSlots = Environment.transform.Find("ArtSlots").gameObject;
        ArtSlots.GetComponent<SlotMasterScript>().StartEditMode(Settings);
        //ArtSlots.GetComponent<SlotMasterScript>().AddHighlightListeners(ArtSlotEvent);

        //foreach (Transform artSlot in ArtSlots.transform) {
        //    Transform highlight = artSlot.Find("Highlight");
        //    Debug.Log(artSlot);
        //    artSlot.GetComponent<PaintingSlotScript>().SetEditMode(true);
        //    artSlot.GetComponent<PaintingSlotScript>().AddHighlightListener(ArtSlotEvent);
            //highlight.GetComponent<ArtSlotListener>().ev.AddListener(ArtSlotEvent);
            //GameObject highlight = Instantiate(Highlight, artSlot.transform.position, artSlot.transform.rotation);
            //highlight.transform.position = artSlot.transform.position;
            //highlight.SetActive(true);
            //Debug.Log("Setting " + highlight + " active");
            //highlight.gameObject.SetActive(true);
            //highlight.gameObject.GetComponent<MeshRenderer>().SetActive(true);
            //highlight.GetComponent<ArtSlotListener>().ev.AddListener(ClickOnEvent);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //GameObject ArtSlots = Environment.transform.Find("ArtSlots").gameObject;
        //foreach (Transform child in ArtSlots.transform)
        //{
        //    Sprite sprite = IMG2Sprite.instance.LoadNewSprite("bil.png");
        //    child.GetComponent<SpriteRenderer>().sprite = sprite;
        //}
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
				if (ActiveSlot == null)
					ev.artSlot.GetComponent<PaintingSlotScript>().ResetHighlightColor();
				break;
			}
		};
    }

	void SlotClickedOn(GameObject artSlot)
	{
		if (ActiveSlot == artSlot) // Deselect slot
		{
			ActiveSlot = null;
			EditCanvas.gameObject.SetActive(false);
		}
		else // Select slot
		{
			ActiveSlot = artSlot;
			EditCanvas.gameObject.SetActive(true);
		}
	}
	
}
