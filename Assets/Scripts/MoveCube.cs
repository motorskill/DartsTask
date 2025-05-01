using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.IO;
using System.IO.Ports;
using UnityEngine;
using System.Drawing;
using UnityEngine.UI; // Required for UI
using DG.Tweening;
using System.Runtime.Remoting.Messaging;
using UnityEngine.XR; // Import DOTween namespac

public class MoveCube : MonoBehaviour
{
    [SerializeField] GameObject Cube;
    private Transform CubePosition;
    private Rigidbody CubeRigid;

    // Start is called before the first frame update
    void Start()
    {
        CubePosition = Cube.GetComponent<Transform>();
        CubeRigid = Cube.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float forceLR = Input.GetAxis("Horizontal") * 0.01f;
        float forceUD = Input.GetAxis("Vertical") * 0.01f;
        float newPosX = CubePosition.position.x + forceLR;
        float newPosZ = CubePosition.position.z + forceUD;
        Vector3 newPosition = new Vector3 (newPosX, CubePosition.position.y, newPosZ);
        CubeRigid.MovePosition(newPosition);
    }
}
