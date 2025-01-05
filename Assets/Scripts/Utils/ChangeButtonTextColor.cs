using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text Text;

    private void Start() {
        if (Text == null) {
            Text = GetComponent<Text>();
            if (Text == null) {
                Debug.LogError("Text component not found on GameObject: " + gameObject.name);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (Text != null) {
            Text.color = Color.cyan;
        } else {
            Debug.LogError("Text component is null in OnPointerEnter on GameObject: " + gameObject.name);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (Text != null) {
            Text.color = Color.white;
        } else {
            Debug.LogError("Text component is null in OnPointerExit on GameObject: " + gameObject.name);
        }
    }
}