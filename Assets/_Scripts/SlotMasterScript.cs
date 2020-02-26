using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMasterScript : MonoBehaviour
{

    private int slots = 0;
    //public RoomSettings roomSettings;

    // Start is called before the first frame update
    void Start()
    {
        string filename = "bil.png";
        foreach (Transform child in transform)
        {
            Debug.Log(slots);
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
            ArtRegistry.GetArtRegistry().AddArt(artMeta);

            //Sprite sprite = IMG2Sprite.instance.LoadNewSprite(filename);
            //Debug.Log(sprite);

            //child.GetComponent<SpriteRenderer>().sprite = sprite;
            slots++;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
