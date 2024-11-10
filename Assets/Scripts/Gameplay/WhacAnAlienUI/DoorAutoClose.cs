using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAutoClose : MonoBehaviour
{
    public bool hasUsed;
    public BoxCollider Trigger;
    public BoxCollider BoxCollider;
    public MeshRenderer Renderer;
    // Start is called before the first frame update
    void Start()
    {
        hasUsed = false;
        Renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other) {
        if(!hasUsed && other.gameObject.tag == "Player"){
            hasUsed = true;
            Trigger.enabled = false;
            BoxCollider.enabled = true;
            Renderer.enabled = true;
        }
    }
}
