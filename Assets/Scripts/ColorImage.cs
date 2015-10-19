using UnityEngine;
using System.Collections;
//using System.IO;

public class ColorImage : MonoBehaviour
{
	public IisuInputProvider IisuInput;
	public Texture2D ColorMap;
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;

	private ImageConvertor _imageConvertor;
	private float _heightWidthRatio;
	private float _timer;

	void Awake()
	{
		_imageConvertor = new ImageConvertor(480, 270); //480, 270 - 640, 480 - 1280x720 - Size of the display
		_timer = 0;
		_heightWidthRatio = 90f/160f;//60f / 80f;
	}
	
	/// <summary>
	/// We get the depth image from iisu, which is in a 16bit grey image format 
	/// The image is converted by the ImageConvertor class to a Unity image, and then applied to the 2D GUI texture
	/// </summary>	
	void Update()
	{
		//we update the depthmap 60fps (30fps)
		if(_timer >= 0.01666f)
		{
			_timer = 0;
			
			if (ColorMap == null)
			{
				ColorMap = new Texture2D(480, 270, TextureFormat.ARGB32, false); //Size should be the same as above
			}
			
			_imageConvertor.generateColorMap(IisuInput.ColorMap, ref ColorMap);
			
		}
		else
		{
			_timer += Time.deltaTime;
		}
	}

	void OnGUI()
	{
		if (ColorMap != null)
		{
			//Debug.Log("Value is "+(-Screen.width * NormalizedWidth));
			//Debug.Log("Next is "+(-Screen.width * NormalizedWidth * _heightWidthRatio));
			GUI.DrawTexture(new Rect(Screen.width * NormalizedXCoordinate + Screen.width * NormalizedWidth,
			                         Screen.height * NormalizedYCoordinate + Screen.width * NormalizedWidth * _heightWidthRatio,
			                         Screen.width * NormalizedWidth,
			                         -Screen.width * NormalizedWidth * _heightWidthRatio), ColorMap);
		}
		//byte[] bytes = ColorMap.EncodeToPNG();
		//File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
	}
}

