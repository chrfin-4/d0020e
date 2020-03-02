using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PaintingSlotScript : MonoBehaviour
{
    public SlotSettings Settings;
    //public HighlightClickedOnEvent ev = new HighlightClickedOnEvent();
    private bool EditMode = false;
    public GameObject Highlight;

    private Color HighlightStartColor;

    void Start()
    {
        Highlight = transform.Find("Highlight").gameObject;
        HighlightStartColor = Highlight.GetComponent<Renderer>().material.color;
        //highlight.GetComponent<ArtSlotListener>().ev.AddListener(HighlightClickedOn);
        // XXX Hacky:
        //      The highlight object needs to be active in the editor,
        //      otherwise it cannot be found when we actually need it to be
        //      active.
        Highlight.SetActive(false);
    }

    public void SetHighlightColor(Color color) =>
        Highlight.GetComponent<Renderer>().material.color = color;

    public void ResetHighlightColor() =>
        Highlight.GetComponent<Renderer>().material.color = HighlightStartColor;

    //void HighlightClickedOn()
    //{
    //    ev.Invoke(gameObject);
    //}

    public void SetEditMode(bool state)
    {
        EditMode = state;
        Highlight.gameObject.SetActive(EditMode);
    }

    public void AddHighlightListener(UnityAction<(HighlightEventType, GameObject)> call) =>
        Highlight.GetComponent<ArtSlotListener>().ev.AddListener(call);

}
