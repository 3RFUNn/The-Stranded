using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeButtonTextColor : MonoBehaviour
{
    public Color HoveringColor, NormalColor;
    public TextMeshProUGUI TMP;

    private void Start() {
        TMP = GetComponent<TextMeshProUGUI>();
    }
    public void OnMouseEnter() {
        TMP.color = HoveringColor;
    }

    public void OnMouseLeave() { 
        TMP.color = NormalColor;
    }
}
