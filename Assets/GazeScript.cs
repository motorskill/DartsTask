using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gazePosition.Set(0, 0, Z_OFFSET); //Default position centered on camera
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = gazePosition; //Move gaze to current position
    }

    private const double MAX_X = 5.132; //Maximum X relative to camera before exiting view on the focal plane
    private const double MIN_X = -5.132; //Minimum X relative to camera before exiting view on the focal plane
    private const double MAX_Y = 2.88675; //Maximum Y relative to camera before exiting view on the focal plane
    private const double MIN_Y = -2.88675; //Minimum Y relative to camera before exiting view on the focal plane
    private const int Z_OFFSET = 3; //Z offset relative to camera to remain within focal plane
    private Vector3 gazePosition = new(); //Position of gaze relative to camera

    public void GetGazePosition(int screenPctX, int screenPctY)
    {
        double fullX = (MIN_X * -1) + MAX_X; //Adjust to 0 - (MAX * 2) scale
        fullX = fullX * screenPctX; //Adjust based on screen percentage
        fullX -= MIN_X; //Return to MIN - MAX scale
        double fullY = (MIN_Y * -1) - MAX_Y; //Adjust to 0 - MAX scale
        fullY = fullY * screenPctY; //Adjust based on screen percentage
        fullY -= MIN_Y; //Return to MIN - MAX scale
        gazePosition.Set((float)fullX, (float)fullY, Z_OFFSET); //Set new position
    }
}
