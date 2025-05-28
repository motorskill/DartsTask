using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gazePosition.Set(0, 0, Z_OFFSET); //Default position centered on camera
        EyeLinkManager.GetScreenResXY(out screenXMax, out screenYMax);
        //Get screen X and Y max from something
    }

    // Update is called once per frame
    void Update()
    {
        GetGazeXYPct();
        transform.localPosition = gazePosition; //Move gaze to current position
    }

    private const float MAX_X = 5.132F; //Maximum X relative to camera before exiting view on the focal plane
    private const float MIN_X = -5.132F; //Minimum X relative to camera before exiting view on the focal plane
    private const float MAX_Y = 2.88675F; //Maximum Y relative to camera before exiting view on the focal plane
    private const float MIN_Y = -2.88675F; //Minimum Y relative to camera before exiting view on the focal plane
    private const int Z_OFFSET = 5; //Z offset relative to camera to remain within focal plane

    private int screenXMax = 1919; //These are probably correct, I would just prefer to get this dynamically
    private int screenYMax = 1079;

    private Vector3 gazePosition = new(); //Position of gaze relative to camera

    [SerializeField] private EyeLinkManager EyeLinkManager;
    //[SerializeField] private GameObject focalPlane; //Focal plane object
    //private Rect focalRect; //Rectangle describing the bounds of the focal plane object in screen space

    public void GetGazePosition(float screenPctX, float screenPctY)
    {
        float fullX = (MIN_X * -1) + MAX_X; //Adjust to 0 - (MAX * 2) scale
        fullX = fullX * screenPctX; //Adjust based on screen percentage
        fullX -= MIN_X; //Return to MIN - MAX scale

        float fullY = (MIN_Y * -1) - MAX_Y; //Adjust to 0 - MAX scale
        fullY = fullY * screenPctY; //Adjust based on screen percentage
        fullY -= MIN_Y; //Return to MIN - MAX scale

        gazePosition.Set(fullX, fullY, Z_OFFSET); //Set new position
    }

    private void GetGazeXYPct()
    {
        try //Get rid of error messages with this one simple trick!
        {
            FSAMPLE eyeData = EyeLinkManager.currentEyeTrackingData; //Or something that gets the correct data

            float pctX = eyeData.gx[0] / screenXMax; //Sets value to percentage of screen width
            float pctY = eyeData.gy[0] / screenYMax; //Sets value to percentage of screen height

            GetGazePosition(pctX, pctY);
        }
        catch { }
    }

    //// This method gets the game object's position and returns it in EyeLink coordinates (pixels), where
    //// 0,0 corresponds to the top-left corner of the screen and values increase as position moves right and down from the left/top edges
    //private static Rect GetScreenRectFromGameObject(GameObject gameObject)
    //{

    //    Vector3 cen = gameObject.GetComponent<Renderer>().bounds.center;
    //    Vector3 ext = gameObject.GetComponent<Renderer>().bounds.extents;
    //    Vector2[] extentPoints = new Vector2[8]
    //    {
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
    //        Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z))
    //    };
    //    Vector2 min = extentPoints[0];
    //    Vector2 max = extentPoints[0];
    //    foreach (Vector2 v in extentPoints)
    //    {
    //        min = Vector2.Min(min, v);
    //        max = Vector2.Max(max, v);
    //    }

    //    // set left/right/top/bottom in EyeLink coords (0,0 means top left rather than Unity's 0,0 bottom left)
    //    float left = min.x;
    //    float top = screenResY - max.y;
    //    float right = max.x;
    //    float bottom = screenResY - min.y;
    //    // return the left, top, width, and height values
    //    return new Rect((int)Math.Round(left), (int)Math.Round(top), (int)Math.Round(right - left), (int)Math.Round(bottom - top));
    //}
}
