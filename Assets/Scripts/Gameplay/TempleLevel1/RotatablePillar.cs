using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatablePillar : MonoBehaviour
{
    public GameObject InteractionText;
    public bool canRotate = true;
    
    // Start is called before the first frame update
    void Start()
    {
        InteractionText = GameObject.Find("UI").transform.Find("Canvas/HUD/RotateKeyPrompt").gameObject;
    }


    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player"){
            InteractionText.SetActive(true);
            RotatePillar.instance.currentPillar = transform;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            InteractionText.SetActive(false);
            RotatePillar.instance.currentPillar = null;
        }
    }

    private void OnValidate() {
        //InteractionText = GameObject.Find("UI").transform.Find("Canvas/HUD/RotateKeyPrompt").gameObject;
        //if (!GetComponent<CapsuleCollider>()) {
        //    CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        //    collider.center = new Vector3(0f, -5f, 0f);
        //    collider.radius = 4f;
        //    collider.height = 10f;
        //    collider.isTrigger = true;
        //}
    }
}
