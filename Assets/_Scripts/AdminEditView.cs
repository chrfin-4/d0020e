using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdminEditView : MonoBehaviour
{

    public GameObject Environment;
    //public GameObject Highlight;

    //private List<GameObject> ArtSlotHighlights = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Vector3 roomPos = Environment.transform.position;
        transform.position = new Vector3(roomPos.x, 30.0f, roomPos.z);
        transform.rotation = Quaternion.Euler(90.0f, 0f, 0f);

    }

    void OnEnable()
    {
        GameObject ArtSlots = Environment.transform.Find("ArtSlots").gameObject;

        foreach (Transform artSlot in ArtSlots.transform) {
            Transform highlight = artSlot.Find("Highlight");
            Debug.Log(artSlot);
            artSlot.GetComponent<PaintingSlotScript>().SetEditMode(true);
            artSlot.GetComponent<PaintingSlotScript>().AddHighlightListener(ArtSlotEvent);
            //highlight.GetComponent<ArtSlotListener>().ev.AddListener(ArtSlotEvent);
            //GameObject highlight = Instantiate(Highlight, artSlot.transform.position, artSlot.transform.rotation);
            //highlight.transform.position = artSlot.transform.position;
            //highlight.SetActive(true);
            //Debug.Log("Setting " + highlight + " active");
            //highlight.gameObject.SetActive(true);
            //highlight.gameObject.GetComponent<MeshRenderer>().SetActive(true);
            //highlight.GetComponent<ArtSlotListener>().ev.AddListener(ClickOnEvent);
        }
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
        if      (ev.type == HighlightEventType.ClickOn)
        {
            Debug.Log("Art slot " + ev.artSlot + " click on");
        }
        else if (ev.type == HighlightEventType.MouseEnter)
        {
            Debug.Log("Art slot " + ev.artSlot + " mouse enter");
            ev.artSlot.GetComponent<PaintingSlotScript>().SetHighlightColor(Color.yellow);
        }
        else if (ev.type == HighlightEventType.MouseExit)
        {
            Debug.Log("Art slot " + ev.artSlot + " mouse exit");
            ev.artSlot.GetComponent<PaintingSlotScript>().ResetHighlightColor();
        }
    }

}
