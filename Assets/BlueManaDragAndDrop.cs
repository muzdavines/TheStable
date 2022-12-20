using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;

public class BlueManaDragAndDrop : MonoBehaviour, IDragHandler, IEndDragHandler {
    public Vector2 startPosition;
    private RectTransform rectTransform;
    private bool isValidDrop = false;

    void Start() {
        // Store the initial position of the UI element and get a reference to its RectTransform component
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        // Store the initial position of the UI element when the drag operation begins
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData) {
        // Update the position of the UI element based on the current mouse position
        rectTransform.anchoredPosition += eventData.delta;
    }
    

    public void OnEndDrag(PointerEventData eventData) {
        print("ONENDDRAG");
        // Check if the UI element is being released over another UI element that is set up to accept drops
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        isValidDrop = false;
        foreach (var result in results) {
            if (result.gameObject.GetComponent<IDropHandler>() != null) {
                if (result.gameObject == gameObject) {
                    continue;
                }
                print(result.gameObject.name + "TRUE");
                isValidDrop = true;
                break;
            }
        }

        // If the UI element was not released on a valid drop location, snap it back to its initial position
        if (!isValidDrop) {
            print("NOT VALID");
            rectTransform.anchoredPosition = startPosition;
            LayoutElement l = GetComponent<LayoutElement>();
            l.minHeight += .001f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInParent<VerticalLayoutGroup>().GetComponent<RectTransform>());

        }
    }
}