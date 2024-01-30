using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BGPanelController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AnimatedPanelController animatedPanelController;
    // パネルクリック時にCloseメソッドを呼び出す
    public void OnPointerClick(PointerEventData pointerData)
    {
        animatedPanelController.Close();
    }
}
