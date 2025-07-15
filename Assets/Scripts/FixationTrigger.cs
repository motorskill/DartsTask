using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        if (!withinTrigger && Time.time - lastDingTS > timeBetweenDings && Time.time - timeLeftRegion > timeBeforeDing) //Checks if gaze is out of fixation region, AND the sound delay has elapsed, AND the gaze has been out of the region for the required amount of time
        {
            // outOfFixationDing.Play();
            lastDingTS = Time.time;
            Debug.Log(debugGazeOut);
        }
    }

    private bool withinTrigger = true; //Is the gaze currently within the fixation
    private float lastDingTS = 0; //Timestamp of last sound effect
    private float timeLeftRegion = 0; //Timestamp of last time gaze left fixation
    private float timeLastCheck = 0; //Timestamp of last debugGazeStay message
    private AudioSource outOfFixationDing;

    [SerializeField] float timeBetweenDings; //Delay between each sound effect
    [SerializeField] float timeBeforeDing; //Grace period before sound effect
    [SerializeField] string triggerTag; //What tag should the other object have to count for this trigger
    [SerializeField] string debugGazeExit; //Message on gaze exiting
    [SerializeField] string debugGazeEnter; //Message on gaze entering
    [SerializeField] string debugGazeOut; //Message on gaze remaining out of fixation
    [SerializeField] bool enableDebugGazeStay; //Toggle message for gaze remaining within fixation
    [SerializeField] string debugGazeStay; //Message on gaze remaining within fixation

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
            lastDingTS = 0;
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
