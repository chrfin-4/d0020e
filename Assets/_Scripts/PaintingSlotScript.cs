using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaintingSlotScript : MonoBehaviour
{
	
    public SlotSettings Settings;
    //public HighlightClickedOnEvent ev = new HighlightClickedOnEvent();
    //private bool EditMode = false;
    public GameObject Highlight;
    public GameObject EditCanvas;
	
	public GameObject UIButton {get; set;}

    private Color HighlightStartColor;

    void Start()
    {
        Highlight = transform.Find("Highlight").gameObject;
        HighlightStartColor = Highlight.GetComponent<Renderer>().material.color;
		
		EditCanvas = Highlight.transform.Find("EditCanvas").gameObject;
		//EditCanvas.GetComponent<Canvas>.EventCamera = ;
		
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

    //public void SetEditMode(bool state)
    //{
    //    EditMode = state;
    //    Highlight.gameObject.SetActive(EditMode);
    //}
	
	public void SetEditMode(bool state)
	{
		Highlight.SetActive(state);
		//AddHighlightListener(state);
		if (state)
		{
			Vector3 artPos = transform.position;
			EditCanvas.transform.position = new Vector3(artPos.x - 4.0f, artPos.y + 3.0f, artPos.z);
			EditCanvas.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
			SetArtButton();
		}
	}
	
	public void setCamera(Camera camera) =>
		EditCanvas.GetComponent<Canvas>().worldCamera = camera;

    public void AddHighlightListener(UnityAction<(HighlightEventType, GameObject)> call) =>
        Highlight.GetComponent<ArtSlotListener>().ev.AddListener(call);

	void SetArtButton()
    {
        GameObject uiButtonInstance = Instantiate(UIButton);
        Button button = uiButtonInstance.GetComponent<Button>();
        //var buttonTextChild = buttonObject.transform.GetChild(0);
        //Text buttonTextChildComponent = (Text) buttonTextChild.GetComponent("Text");
        //buttonTextChildComponent.text = "Set Art";
		uiButtonInstance.transform.Find("Text").GetComponent<Text>().text = "Set Art";
        button.transform.SetParent(EditCanvas.transform, false);
        //button.onClick.AddListener(() => {});

        //Vector3 pos = new Vector3(0.0f , 0.0f, 0.0f);
        uiButtonInstance.transform.position = Vector3.zero;

        //buttons.Add(uiButtonInstance);
    }
	
}
