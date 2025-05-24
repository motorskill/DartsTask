using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixationTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        outOfFixationDing = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!withinTrigger && Time.time - lastDingTS > timeBetweenDings && Time.time - timeLeftRegion > .1)
        {
            outOfFixationDing.Play();
            lastDingTS = Time.time;
            Debug.Log(debugGazeOut);
        }
    }

    private bool withinTrigger = true;
    private float lastDingTS = 0;
    private float timeLeftRegion = 0;
    private float timeLastCheck = 0;
    private AudioSource outOfFixationDing;

    [SerializeField] float timeBetweenDings;
    [SerializeField] string triggerTag;
    [SerializeField] string debugGazeExit;
    [SerializeField] string debugGazeEnter;
    [SerializeField] string debugGazeOut;
    [SerializeField] bool enableDebugGazeStay;
    [SerializeField] string debugGazeStay;

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Test Exit");
        if (other.CompareTag(triggerTag))
        {
            withinTrigger = false;
            timeLeftRegion = Time.time;
            Debug.Log(debugGazeExit);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Test Enter");
        if (other.CompareTag(triggerTag))
        {
            withinTrigger = true;
            Debug.Log(debugGazeEnter);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (enableDebugGazeStay)
        {
            if (other.CompareTag(triggerTag) && Time.time - timeLastCheck > 1)
            {
                Debug.Log(debugGazeStay);
                timeLastCheck = Time.time;
            }
        }
    }
}
