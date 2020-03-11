using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum HighlightEventType
{
    ClickOn,
    MouseEnter,
    MouseExit
}

// This class (with type param) required to retrieve GameObject on callback
public class HighlightClickedOnEvent : UnityEvent<(HighlightEventType, GameObject)> {}

// TODO change name
public class ArtSlotListener : MonoBehaviour
{
    public HighlightClickedOnEvent ev = new HighlightClickedOnEvent();

    void OnMouseDown() =>
        ev.Invoke((HighlightEventType.ClickOn, transform.parent.gameObject));

    void OnMouseEnter() =>
        ev.Invoke((HighlightEventType.MouseEnter, transform.parent.gameObject));

    void OnMouseExit() =>
        ev.Invoke((HighlightEventType.MouseExit, transform.parent.gameObject));

}
