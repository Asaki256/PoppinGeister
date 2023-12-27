using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SettingBackImageController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AnimatedSettingController animatedSettingController;
    // パネルクリック時にCloseメソッドを呼び出す
    public void OnPointerClick(PointerEventData pointerData)
    {
        animatedSettingController.Close();
    }
}
