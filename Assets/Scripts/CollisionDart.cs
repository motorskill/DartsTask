using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDart : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // Stop the dart's movement on collision
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Optionally, disable further physics updates for the dart
        rb.isKinematic = true;

        // Log the collision for debugging
        Debug.Log("Dart collided with: " + collision.gameObject.name);
    }

    // Alternatively, if using triggers
    void OnTriggerEnter(Collider other)
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.isKinematic = true;

        Debug.Log("Dart entered trigger with: " + other.gameObject.name);
    }
}
