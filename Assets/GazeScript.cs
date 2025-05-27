using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gazePosition.Set(0, 0, Z_OFFSET); //Default position centered on camera
        focalRect = GetScreenRectFromGameObject(focalPlane); //Creates a rectangle out of the focal plane object
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = gazePosition; //Move gaze to current position
    }

    private const float MAX_X = 5.132F; //Maximum X relative to camera before exiting view on the focal plane
    private const float MIN_X = -5.132F; //Minimum X relative to camera before exiting view on the focal plane
    private const float MAX_Y = 2.88675F; //Maximum Y relative to camera before exiting view on the focal plane
    private const float MIN_Y = -2.88675F; //Minimum Y relative to camera before exiting view on the focal plane
    private const int Z_OFFSET = 3; //Z offset relative to camera to remain within focal plane
    private Vector3 gazePosition = new(); //Position of gaze relative to camera

    [SerializeField] GameObject focalPlane; //Focal plane object
    private Rect focalRect; //Rectangle describing the bounds of the focal plane object in screen space

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
        List<float> gazeCoords = EyeLinkWebLinkUtil.getSampleData(); //Gets the coordinates list from the utility class
        if (gazeCoords.Count > 0 && gazeCoords[0] != -32768 && gazeCoords[1] != -32768) //Checks for success and that coordinates are valid
        {
            float pctX = gazeCoords[0] / (focalRect.x + focalRect.width); //Sets value to percentage of focal plane width
            float pctY = gazeCoords[1] / (focalRect.y + focalRect.height); //Sets value to percentage of focal plane height
            GetGazePosition(pctX, pctY);
        }
    }

    // This method gets the game object's position and returns it in EyeLink coordinates (pixels), where
    // 0,0 corresponds to the top-left corner of the screen and values increase as position moves right and down from the left/top edges
    private static Rect GetScreenRectFromGameObject(GameObject gameObject)
    {

        Vector3 cen = gameObject.GetComponent<Renderer>().bounds.center;
        Vector3 ext = gameObject.GetComponent<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
        {
            Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
            Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z))
        };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        // set left/right/top/bottom in EyeLink coords (0,0 means top left rather than Unity's 0,0 bottom left)
        float left = min.x;
        float top = screenResY - max.y;
        float right = max.x;
        float bottom = screenResY - min.y;
        // return the left, top, width, and height values
        return new Rect((int)Math.Round(left), (int)Math.Round(top), (int)Math.Round(right - left), (int)Math.Round(bottom - top));
    }
}
