using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

// The EyeLinkWebLinkUtil class provides common functions for communicating with WebLink via C# in Unity
// The class allows you to:
// 1) Open sockets with WebLink for communication with an EyeLink system
// 2) Open an interest area set file and log interest area information to that file for analysis with Data Viewer
// 3) Get positions of objects in EyeLink units (to facilitate logging interest area information)
// 4) Send Messages to the EyeLink Data File (EDF File), e.g., for marking events and for conveying information to Data Viewer for analysis
// 5) Send commands to the EyeLink Host PC (e.g., to change its parameters, perform screen drawing, etc.)
// 6) Access EyeLink sample data in real time (e.g., for use in gameplay)
// 7) Close sockets to WebLink

public class EyeLinkWebLinkUtil : MonoBehaviour
{
	// This variable will store the zero time point for dynamic interest area instance information
	public static int iasZeroPoint;

	// Get the current date/time and reformat to replace :, /, and space with _
	public static DateTime localDateTime = DateTime.Now;
	public static string localDateTimeString = Convert.ToString(localDateTime).Replace("/", "_").Replace(":", "_").Replace(" ", "_");

	// Path where the interest area set (IAS) file will be found (relative to where the participant's EDF file will be saved)
	public static string iasFilepath = "../../../EyeLinkWebLinkUnity_Brickbreaker_UN_BUILD/";

	// The name of the interest area set (IAS) file for the participant.  It uses the current date/time in the filename
	// to prevent overwriting files from previous participants
	public static string iasFilename = "myIAS_" + localDateTimeString + ".ias";

	// Open an interest area set (IAS) file
	public static StreamWriter writer = File.CreateText(EyeLinkWebLinkUtil.iasFilename);

	// These variables help with connection to WebLink and are described/initialized in initWebLinkConnection
	public static string IP;  
	public static int portForSending;  
	public static int portForReceiving; 
	public static IPEndPoint remoteEndPointForSending;
	public static IPEndPoint remoteEndPointForReceiving;
	public static UdpClient client;
	public static UdpClient udpServer;
	public static byte[] receivedData;

	// These specify the screen width and height in pixels
	public static int screenResX = 1920;
	public static int screenResY = 1080;

	// These will store the eye sample data that is retrieved from WebLink
	public static float eyeX = 0.0F;
	public static float eyeY = 0.0F;
	public static float eyePupil = 0.0F;

	// This timeOffset variable can be used to shift the start/end timepoints for each dynamic interest area instance by a static amount
	// They are mainly there for debugging -- the experiment uses the times reported by Unity for start/end timepoint specification
	public static int timeOffset = 0;

	// Set Dummy Mode to true if you want to run without WebLink, using the keyboard to control the paddle instead of eye position (as in the original Brickbreaker)
	public static bool dummyMode = false;

	// This method is run at the beginning of the game session.  It connects to WebLink and sends a message pointing to the session's IAS file
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void initWebLinkConnection()
    {
		if (dummyMode == false)
		{
			try
			{
				// Set up IP and port information for communication with WebLink
				// These values should match the values used in the WebLink project, under  
				// Experiment Configuration -> External Message Listener Settings and Online Sample Streaming
				Debug.Log("Attempting Connection to WebLink");
				IP = "127.0.0.1";
				portForSending = 3467;
				portForReceiving = 3468;

				// Set up an end point for sending EyeLink communication (via WebLink connection)
				remoteEndPointForSending = new IPEndPoint(IPAddress.Parse(IP), portForSending);
				// Set up an end point for received EyeLink data (via WebLink connection)
				remoteEndPointForReceiving = new IPEndPoint(IPAddress.Parse(IP), portForReceiving);
				// Set up a client for sending EyeLink communication (via WebLink connection)
				client = new UdpClient();
				// Set up a client for receiving EyeLink data (via WebLink conncetion)
				udpServer = new UdpClient(portForReceiving);
				// Set the receive size to 1 for a WebLink project that streams every sample (so we only have the latest sample in the receive buffer)
				udpServer.Client.ReceiveBufferSize = 1;
				udpServer.Client.ReceiveTimeout = 3;

				// Print the name of the IAS file for debugging
				Debug.Log("!V IAREA FILE " + iasFilepath + iasFilename);

				// Send a message pointing to the interest area set (IAS) file, which will store positions of game objects over time for analysis
				// The time of the message is the 0 time point point for the IAS file (for any dynamic interest area timeudpServers), so we also log the current time
				// Any interest areas logged to that file will be available in Data Viewer for analysis
				// See the section of the Data Viewer User Manual "Protocol for EyeLink Data to Viewer Integration -> Interest Area Commands"
				iasZeroPoint = Convert.ToInt32(Time.realtimeSinceStartup * 1000);
				byte[] data = Encoding.UTF8.GetBytes("!V IAREA FILE " + iasFilepath + iasFilename);
				client.Send(data, data.Length, remoteEndPointForSending);
			}
			catch
			{
				dummyMode = true;
				Debug.Log("Running in Dummy Mode");
			}		
		}
		else
		{
			Debug.Log("Running in Dummy Mode");
		}
	}


	// This method closes the sockets used for communication with WebLink
	public static void closeWebLinkConnections()
	{
		client.Close();
		udpServer.Close();
		Debug.Log("WebLink Connections Closed");
	}


	// This method gets the game object's position and returns it in EyeLink coordinates (pixels), where
	// 0,0 corresponds to the top-left corner of the screen and values increase as position moves right and down from the left/top edges
	public static Rect getScreenRectFromGameObject(GameObject gameObject)
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


	// This method gets the latest eye sample
	public static List<float> getSampleData()
	{
		try
		{
			receivedData = udpServer.Receive(ref remoteEndPointForReceiving);
		}
		catch
		{
			return new List<float>();
		}

		// Get the latest sample string (if available) and split it into a list
		string sampleString = Encoding.UTF8.GetString(receivedData);
		List<string> sampleList = sampleString.Split().ToList();
		// Check that the string received is a sample
		if (sampleList[0] == "Sample")
		{
			// Check if it is a binocular sample; if so, use the right eye data
			if (sampleList[2] == "Both")
			{
				float leftX = float.Parse(sampleList[3]);
				float leftY = float.Parse(sampleList[4]);
				float leftPupil = float.Parse(sampleList[5]);
				float rightX = float.Parse(sampleList[6]);
				float rightY = float.Parse(sampleList[7]);
				float rightPupil = float.Parse(sampleList[8]);
				eyeX = rightX;
				eyeY = rightY;
				eyePupil = rightPupil;
			}
			else
			{
				eyeX = float.Parse(sampleList[3]);
				eyeY = float.Parse(sampleList[4]);
				eyePupil = float.Parse(sampleList[3]);
			}
			var eyeData = new List<float> { eyeX, eyeY, eyePupil };
			// Return the eye data
			return eyeData;
		}

		else 
		{
			return new List<float>();
		}
	}


	// This method writes a line containing interest area information to the interest area set (IAS) file
	public static void writeIASLine(string textToWrite)
	{
		writer.WriteLine(textToWrite);
	}


	// This method writes a message to the EDF
	// This can be used for event marking or for Data Viewer Integration
	// See the Data Viewer User Manual, section "Protocol for EyeLink Data to Viewer Integration" for 
	// expected Data Viewer message formatting information
	public static void sendMessage(string message)
	{
		try
		{
			if (message != "")
			{

			// use utf8 encoding to set up message
			byte[] data = Encoding.UTF8.GetBytes(message);

			// send the message
			client.Send(data, data.Length, remoteEndPointForSending);
			}
		}
		catch (Exception err)
		{
			print(err.ToString());
		}
	}


	// This method writes a command to the Host PC
	public static void sendCommand(string commandText)
	{
		try
		{
			if (commandText != "")
			{

				// use utf8 encoding to set up message
				byte[] data = Encoding.UTF8.GetBytes("eyecmd " + commandText + " endcmd");

				// send the message
				client.Send(data, data.Length, remoteEndPointForSending);
			}
		}
		catch (Exception err)
		{
			print(err.ToString());
		}
	}

}