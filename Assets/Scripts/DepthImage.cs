/***************************************************************************************/
//
//  SoftKinetic iisu SDK code samples 
//
//  Project Name      : skeletonBubbleManSample
//  Revision          : 1.0
//  Description       : Tutorial on how to use the Skeleton and Bubbleman. 
//						It is recommended to use this sample to get started, 
//						as it covers the most common uses of iisu: skeleton, 
//						bubbleman and displaying the depthmap + usermask.
//
// DISCLAIMER
// All rights reserved to SOFTKINETIC INTERNATIONAL SA/NV (a company incorporated
// and existing under the laws of Belgium, with its principal place of business at
// Boulevard de la Plainelaan 15, 1050 Brussels (Belgium), registered with the Crossroads
// bank for enterprises under company number 0811 784 189 - “Softkinetic”)
//
// For any question about terms and conditions, please contact: info@softkinetic.com
// Copyright (c) 2007-2011 SoftKinetic SA/NV
//
/****************************************************************************************/
/*
 * My thoughts are that with this depth map capability, we want to demonstrate the usage of the UV map in order
 * to match the depth and color of a given pixel. The registered color iamge will match perfectly onto the depth
 * image. Note that negative values on the UV map mean no matching pixel was found between the depth and the color map.
 * SOURCE.CAMERA.COLOR.REGISTRATION.UV is this registration mapping image between the depth and color images.
 * So to prove this works, we will use the registration mapping image to modify a depth map image to the size of the
 * color map image. Thus pixels with negative registration values will have to be set with a color value (0) that is 
 * black.
 * So color image is 1280x720. Registration mapping image is 320x240, same as depth image.
 * Note that the values outputed in the raw depth image are the distances of each pixel in that depth image in millimeters
 */

using UnityEngine;
using System.Collections;

public class DepthImage : MonoBehaviour
{
    public IisuInputProvider IisuInput;
	//public HandIisuInput Hand1;
	//public HandIisuInput Hand2;

    private ImageConvertor _imageConvertor;

    public Texture2D outputMap;
	
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	
	private float _heightWidthRatio;
	
	private float _timer;
	
    void Awake()
    {
        _imageConvertor = new ImageConvertor(320, 240); //Me: 320x240 (160x120)
		_timer = 0;
		_heightWidthRatio = 120f/160f;
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
			
			if (outputMap == null)
	        {
	            outputMap = new Texture2D(320, 240, TextureFormat.ARGB32, false);
	       	}
			
			//int handLabel1 = -1;
			//int handLabel2 = -1;
			
			//if(Hand1.Detected)
			//	handLabel1 = IisuInput.Hand1Label;
			//if(Hand2.Detected)
			//	handLabel2 = IisuInput.Hand2Label;
			
			//_imageConvertor.generateHandMask(IisuInput.DepthMap, IisuInput.LabelImage, ref DepthMap, handLabel1, handLabel2);
			_imageConvertor.generateHandMask(IisuInput.DepthMap, IisuInput.ColorMap, IisuInput.UVMap, ref outputMap);
			
		}
		else
		{
			_timer += Time.deltaTime;
		}
    }

    void OnGUI()
    {
        if (outputMap != null)
        {
            GUI.DrawTexture(new Rect(Screen.width * NormalizedXCoordinate + Screen.width * NormalizedWidth,
                                     Screen.height * NormalizedYCoordinate + Screen.width * NormalizedWidth * _heightWidthRatio,
                                     Screen.width * NormalizedWidth,
                                     -Screen.width * NormalizedWidth * _heightWidthRatio), outputMap);
        }
    }
}
