using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TacticsFieldPositionController : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        print("Valid");
        throw new System.NotImplementedException();
    }
}
