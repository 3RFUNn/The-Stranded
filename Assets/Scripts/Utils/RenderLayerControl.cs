using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderLayerControl : MonoBehaviour
{
    public LayerMask OnlyOutline;
    public LayerMask OnlyInterior;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<BoxCollider>().bounds.Contains(GamePhaseManager.instance.Player.transform.position)) {
            Camera.main.cullingMask = OnlyInterior;
        }
        else {
            Camera.main.cullingMask = OnlyOutline;
        }
    }


    private void OnTriggerEnter(Collider other) {
        if(other.tag=="Player"){
            Camera.main.cullingMask = OnlyInterior;
        }

    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            Camera.main.cullingMask = OnlyOutline;
        }
    }
}
