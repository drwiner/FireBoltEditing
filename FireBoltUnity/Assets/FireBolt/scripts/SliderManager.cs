using UnityEngine;
using System.Collections;
using System.IO;

public class SliderManager : MonoBehaviour 
{
    private UnityEngine.UI.Slider slider;
    private UnityEngine.UI.RawImage thumb;
    private RectTransform sliderRect;
    private bool thumbnailEnabled;
    private Texture2D image;
    private ElPresidente elP;

	// Use this for initialization
	void Start () 
    {
        slider = gameObject.GetComponent<UnityEngine.UI.Slider>();
        sliderRect = gameObject.GetComponent<RectTransform>();
        thumbnailEnabled = false;
        image = LoadPNG(@"Assets/screens/0.png");

        GameObject elPGO = GameObject.Find("FireBolt");
        elP = elPGO.GetComponent<ElPresidente>();

        GameObject thumbGO = GameObject.Find("Thumbnail");
        thumb = thumbGO.GetComponent<UnityEngine.UI.RawImage>();
	}
	
	void Update ()
    {
        if (thumbnailEnabled) drawThumbnail();
    }

    public void ThumbnailOn ()
    {
        thumbnailEnabled = true;
        thumb.color = new Color(255, 255, 255, 255);
    }

    public void ThumbnailOff ()
    {
        thumbnailEnabled = false;
        thumb.color = new Color(255, 255, 255, 0);
    }

    private void drawThumbnail ()
    {
        float width = sliderRect.rect.width;
        float deltaX = width * slider.value;
        float x = deltaX + sliderRect.position.x - (width / 2);
        thumb.texture = image;
        thumb.rectTransform.position = new Vector3(x, sliderRect.position.y + (thumb.rectTransform.rect.height / 1.5f), thumb.rectTransform.position.z);
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }
}
