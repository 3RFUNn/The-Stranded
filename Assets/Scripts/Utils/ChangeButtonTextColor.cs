using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonTextColor : MonoBehaviour
{
    public Color HoveringColor, NormalColor;
    public Text Text;

    private void Start() {
        Text = GetComponent<Text>();
    }
    public void OnMouseEnter() {
        Text.color = HoveringColor;
    }

    public void OnMouseLeave() {
        Text.color = NormalColor;
    }
}