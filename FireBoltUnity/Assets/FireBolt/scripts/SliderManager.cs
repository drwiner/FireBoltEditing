using UnityEngine;
using System.Collections;
using System.IO;

public class SliderManager : MonoBehaviour 
{
    // The slider UI object.
    private UnityEngine.UI.Slider slider;

    // The slider's transform rectangle.
    private RectTransform sliderRect;

    // The thumbnail UI object.
    private UnityEngine.UI.RawImage thumb;

    // Stores whether the thumbnail is enabled.
    private bool thumbnailEnabled;

    // An array containing keyframe images.
    private Texture2D[] images;

    // Stores the El Presidente script.
    private ElPresidente elPresidente;

    // Stores whether initialization has occurred.
    private bool hasInitialized;

	// Use this for initialization
	void Start () 
    {
        // Get the slider UI object from the slider game object.
        slider = gameObject.GetComponent<UnityEngine.UI.Slider>();

        // Get the slider's transform rectangle from the game object.
        sliderRect = gameObject.GetComponent<RectTransform>();

        // Set the thumbnail to be hidden on initialization.
        thumbnailEnabled = false;

        // Find the thumbnail game object in the scene.
        GameObject thumbGO = GameObject.Find("Thumbnail");

        // Grab the thumbnail UI object from the game object.
        thumb = thumbGO.GetComponent<UnityEngine.UI.RawImage>();

        // Find the FireBolt game object.
        GameObject fireBoltGO = GameObject.Find("FireBolt");

        // Grab the El Presidente script from the FireBolt game object.
        elPresidente = fireBoltGO.GetComponent<ElPresidente>();

        // Remember that initialization has not occurred.
        hasInitialized = false;
	}
	
	void Update ()
    {
        // If keyframes have not been loaded and El Presidente has initialized the keyframes...
        if (!hasInitialized && elPresidente.KeyframesGenerated)
        {
            // Load the keyframe images from file.
            loadImages();

            // Remember that we've initialized.
            hasInitialized = true;
        }

        // If the thumbnail is enabled and keyframes have been loaded, call the draw method.
        if (thumbnailEnabled && hasInitialized) drawThumbnail();
    }

    /// <summary>
    /// Initializes the keyframe array and loads the images into it from file.
    /// </summary>
    private void loadImages ()
    {
        // Create a new array to hold the keyframe images.
        images = new Texture2D[20];

        // Loop through the image array
        for (int i = 0; i < 20; i++)
            // And load each keyframe from file into the array.
            images[i] = LoadPNG(@"Assets/screens/" + (i * 5) + ".png");
    }

    /// <summary>
    /// Toggles the keyframe thumbnail on.
    /// </summary>
    public void ThumbnailOn ()
    {
        // Remember that the thumbnail is toggled on.
        thumbnailEnabled = true;

        // Set the thumbnail object to be visible.
        thumb.color = new Color(255, 255, 255, 255);
    }

    /// <summary>
    /// Toggles the keyframe thumbnail off.
    /// </summary>
    public void ThumbnailOff ()
    {
        // Remember that the thumbnail is toggled off.
        thumbnailEnabled = false;

        // Set the thumbnail object to be invisible.
        thumb.color = new Color(255, 255, 255, 0);
    }

    /// <summary>
    /// Positions and draws the correct keyframe in the thumbnail.
    /// </summary>
    private void drawThumbnail ()
    {
        // Store the width of the slider in Canvas space.
        float width = sliderRect.rect.width;

        // Get the position of the slider's progress relative to its width.
        float deltaX = width * slider.value;

        // Caclulate the position of the slider's progress in Canvas space.
        float x = deltaX + sliderRect.position.x - (width / 2);

        // Set the thumbnail UI object's position.
        thumb.rectTransform.position = new Vector3(x, sliderRect.position.y + (thumb.rectTransform.rect.height / 1.5f), thumb.rectTransform.position.z);

        // Calculate the array position of the current keyframe image to display.
        int position = System.Convert.ToInt32(Mathf.Round((slider.value * 100.0f) / 5.0f));

        // Set the thumbnail UI object's texture to the calculate keyframe.
        thumb.texture = images[position];
    }

    /// <summary>
    /// Loads and returns a .png from file as a Texture2D.
    /// </summary>
    public static Texture2D LoadPNG(string filePath)
    {
        // Create a new Texture2D.
        Texture2D tex = null;

        // Create a byte array to store the file data.
        byte[] fileData;

        // Ensure a file exists at the specified path.
        if (File.Exists(filePath))
        {
            // Read the file data into the byte array.
            fileData = File.ReadAllBytes(filePath);

            // Create a new Texture2D.
            tex = new Texture2D(2, 2);

            // Load the file data into the Texture2D.
            tex.LoadImage(fileData);
        }

        // Return the loaded Texture2D.
        return tex;
    }
}
