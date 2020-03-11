using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlotMasterScript : MonoBehaviour
{

    private int slots = 0;
    private bool EditMode = false;
    //private EditSettings Settings;
    //public RoomSettings roomSettings;

    // Start is called before the first frame update
    void Start()
    {
        string filename = "bil.png"; // TODO
        foreach (Transform child in transform)
        {
            /*Debug.Log(slots);
              ArtMetaData artMeta = new ArtMetaData(
              "title",
              "artist",
              filename,
              ArtType.Painting
              );
              child.GetComponent<PaintingSlotScript>().Settings = new SlotSettings(
              slots,
              artMeta
              );
              if (!ArtRegistry.GetArtRegistry().HasArt(artMeta.Checksum))
              ArtRegistry.GetArtRegistry().AddArt(artMeta);

            //Sprite sprite = IMG2Sprite.instance.LoadNewSprite(filename);
            //Debug.Log(sprite);

            //child.GetComponent<SpriteRenderer>().sprite = sprite;
            slots++;*/
        }
    }

    public void StartEditMode(UnityAction<(HighlightEventType, GameObject)> call)
    {
        EditMode = true;
        int i = 0;
        foreach (Transform artSlot in transform)
        {
            PaintingSlotScript slotScript = artSlot.GetComponent<PaintingSlotScript>();
            //slotScript.UIButton = settings.UIButton;
            slotScript.SetEditMode(true);
            slotScript.AddHighlightListener(call);
            //slotScript.setCamera(Settings.EditViewCamera);
            slotScript.Number = i++;
        }
    }

    public void StopEditMode()
    {
        EditMode = false;
        foreach (Transform artSlot in transform)
        {
            PaintingSlotScript slotScript = artSlot.GetComponent<PaintingSlotScript>();
            slotScript.SetEditMode(false);
        }
    }

    public void AddHighlightListeners(UnityAction<(HighlightEventType, GameObject)> call)
    {
        foreach (Transform artSlot in transform)
        {
            artSlot.GetComponent<PaintingSlotScript>().AddHighlightListener(call);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
