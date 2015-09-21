using UnityEngine;
using System.Collections;
using System.IO;

public class SliderManager : MonoBehaviour 
{
    private UnityEngine.UI.Slider slider;
    private RectTransform sliderRect;
    private bool thumbnailEnabled;
    private Texture2D image;

	// Use this for initialization
	void Start () 
    {
        slider = gameObject.GetComponent<UnityEngine.UI.Slider>();
        sliderRect = gameObject.GetComponent<RectTransform>();
        thumbnailEnabled = false;
        image = LoadPNG(@"Assets/screens/0.png");
	}
	
	void OnGUI ()
    {
        if (thumbnailEnabled) drawThumbnail();
        else Debug.Log("GOODBYE WORLD");
    }

    public void ThumbnailOn ()
    {
        thumbnailEnabled = true;
    }

    public void ThumbnailOff ()
    {
        thumbnailEnabled = false;
    }

    private void drawThumbnail ()
    {
        GUI.DrawTexture(new Rect(10, 10, 60, 60), image);
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}
