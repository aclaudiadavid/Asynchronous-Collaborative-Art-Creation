using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantCollider : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnTriggerStay(Collider other)
    {
        Debug.Log("stay");
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("enter");
    }

    public void OnTriggerExit(Collider other)
    {

    }
}
