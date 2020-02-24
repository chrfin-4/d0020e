using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdminEditView : MonoBehaviour
{

    public GameObject Environment;
    public GameObject ArtSlotHighlight;

    private List<GameObject> ArtSlotHighlights = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Vector3 roomPos = Environment.transform.position;
        transform.position = new Vector3(roomPos.x, 30.0f, roomPos.z);
        transform.rotation = Quaternion.Euler(90.0f, 0f, 0f);

        GameObject ArtSlots = Environment.transform.Find("ArtSlots").gameObject;

        foreach (Transform child in ArtSlots.transform)
        {
            Debug.Log(child);
            GameObject ArtSlotHighlightObject = Instantiate(ArtSlotHighlight);
            ArtSlotHighlightObject.transform.position = child.position;

            ArtSlotHighlightObject.GetComponent<ArtSlotListener>().ev.AddListener(ClickOnEvent);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void ClickOnEvent(GameObject artSlot)
    {
        Debug.Log("Event happened on art slot " + artSlot);
    }

}
