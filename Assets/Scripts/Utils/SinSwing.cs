using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinSwing : MonoBehaviour
{
    // Start is called before the first frame update
    
    [SerializeField]
    private float t;
    public float speed;
    public float distance;
    public Vector3 origin;
    public Vector3 worldDirection;
    public Vector3 localDirection;
    public enum Axes
    {
        XAxis,
        YAxis,
        ZAxis,
        ByVector
    } 
    public Axes swingAxis;
    private float velocity = 0;
    void Start()
    {
        origin = transform.position;
    }

    void Update()
    {

        switch(swingAxis){
            case Axes.XAxis:
                localDirection = Vector3.right;
                break;
            case Axes.YAxis:
                localDirection = Vector3.up;
                break;
            case Axes.ZAxis:
                localDirection = Vector3.forward;
                break;
            case Axes.ByVector:
                break;
        }
        worldDirection = transform.TransformDirection(localDirection);

        t += Time.deltaTime * speed;
        t %= 2 * Mathf.PI;
        velocity = Mathf.Cos(t);
        float s = velocity * Time.deltaTime * distance;
        //float s = Mathf.Sin(t) * distance;
        transform.position += s * worldDirection;
        
        
    }
}
