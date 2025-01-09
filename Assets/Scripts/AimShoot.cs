using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimShoot : MonoBehaviour
{

    private Vector3  CrossVelocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        float horizontalInput = (Input.GetAxis("Horizontal"));
        float verticalInput = (Input.GetAxis("Vertical"));
    }
}
