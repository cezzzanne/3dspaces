using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
 

namespace Spaces{
    public class MovePlaceableObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    
    public bool buttonPressed;

    public GameObject PlacementController;
    
    public void OnPointerDown(PointerEventData eventData){
        bool isForward = gameObject.name.ToCharArray()[0] == '1';
        bool isReversed = gameObject.name.ToCharArray()[1] != '1';
        PlacementController.GetComponent<ItemPlacementController>().SetForwardInput(true, isForward, isReversed);
        buttonPressed = true;
    }
    
    public void OnPointerUp(PointerEventData eventData){
        PlacementController.GetComponent<ItemPlacementController>().SetForwardInput(false, false, false);
        buttonPressed = false;
    }
    }
}