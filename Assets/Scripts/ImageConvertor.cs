using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Iisu.Data;
//using System.IO;

/// <summary>
/// Helper class to convert iisu images to Unity images. 
/// </summary>
/*
 * The main goal of the UV map is to look up in the RGB image which color to apply to each pixel (or vertex) in 
 * the depthmap. For every point in the depth map (for every vertex 3D point in Cartesian space), there are U,V
 * values in the UV map. The UV map has the same resolution as the depth map. These U and V serve as coordinates
 * and indicate where in the color map one should go to find the corresponding color for the relevant vertex. 
 * As the color map may have a different (higher) resolution than the depth map, the U and V are given as values
 * between 0 and 1, and need to be properly scaled in order to find the right coordinates in the color map. The 
 * (U,V) origin is in the left upper corner of the color map, U points to the right horizontally, and V points 
 * down vertically. So from coordinate in depth map (X,Y), look up the corresponding (U,V) values at the same (X,Y)
 * indices in de UV map, then use these (U,V) to point to the proper color point in the color map. In order to do this
 * take for example (round[U*(number of horizontal pixels in color map)] , round[V*(number of vertical pixels in color
 * map)]) to point to the corresponding pixel in the color map. One could also work on the RGB image but then you have
 * to realize that depth information is only available for a subset of all RGB points and for the others an interpolation has to be done 
 */
public class ImageConvertor
{
    private Color[] _colored_image;

    private int _width;
    private int _height;

    private byte[] imageRaw;
	private byte[] colorImageRaw;
    private byte[] UVImageRaw;
	
	//these two values are used to map the depth of a hand pixel to a color
	private float minDepth = 10;
	private float maxDepth = 40;

    private float floatConvertor = 1f / 255f;

    public ImageConvertor(int width, int height)//Me: Constructor of ImageConverter class
    {
        _width = width;
        _height = height;
    }

	public ImageConvertor()//Me: Default Constructor of ImageConverter class
    {
        _width = 160;
        _height = 120;
    }

    private void getUVEquivalent(int fromWidth, int fromHeight, int fromU, int fromV, int toWidth, int toHeight, out int toU, out int toV, out int toIndex)
    {
        float uNorm = (float)fromU / (float)fromWidth;
        float vNorm = (float)fromV / (float)fromHeight;

        toU = (int)(uNorm * toWidth);
        toV = (int)(vNorm * toHeight);

        toIndex = toU + toV * toWidth;
    }

    private void getUV(int index, int width, int height, out int u, out int v)
    {
        u = index % width;
        v = index / width;
    }
	
	private Color getColor(int id, int hand1ID, int hand2ID, ushort depthValue)
    {
		//everything belonging to the first hand in green
        if (id == hand1ID)
        {
            return new Color(0, 1f - (0.5f * (Mathf.Clamp(depthValue, minDepth, maxDepth) - minDepth)/(maxDepth - minDepth)), 0);
        }
		//everything belonging to the second hand in blue
        else if (id == hand2ID)
        {
            return new Color(0, 1f - (0.66f * (Mathf.Clamp(depthValue, minDepth, maxDepth) - minDepth)/(maxDepth - minDepth)), 1f);
        }
        else
        {
            return new Color(depthValue * floatConvertor, depthValue * floatConvertor, depthValue * floatConvertor, 1);
        }

    }

	public bool generateHandMask(IImageData depthimage, IImageData colorimage, IImageData UVImage, ref Texture2D destinationImage)//, int hand1ID, int hand2ID)
    {
        if (depthimage == null )//|| idImage == null)
            return false;

		if (depthimage.Raw == IntPtr.Zero )//|| idImage.Raw == IntPtr.Zero)
            return false;

		if (_colored_image == null || _colored_image.Length != depthimage.ImageInfos.BytesRaw / 2)
        {
            _colored_image = new Color[_width * _height];
			imageRaw = new byte[depthimage.ImageInfos.BytesRaw];
			colorImageRaw = new byte[colorimage.ImageInfos.BytesRaw];
        }

        if (UVImageRaw == null || UVImageRaw.Length != UVImage.ImageInfos.BytesRaw)
        {
            UVImageRaw = new byte[UVImage.ImageInfos.BytesRaw];
        }

		uint byte_size = (uint)depthimage.ImageInfos.BytesRaw;
		uint color_byte_size = (uint)colorimage.ImageInfos.BytesRaw;
		//Debug.Log("color byte size "+color_byte_size);
        uint UVImageSize = (uint)UVImage.ImageInfos.BytesRaw;

        // copy image content into managed arrays
		Marshal.Copy(depthimage.Raw, imageRaw, 0, (int)byte_size);
		Marshal.Copy(colorimage.Raw, colorImageRaw, 0, (int)color_byte_size);
        Marshal.Copy(UVImage.Raw, UVImageRaw, 0, (int)UVImageSize);

        int destinationU, destinationV;
        int sourceU, sourceV;
        int sourceIndex;
		int colorIndex;

		int imageWidth = (int)depthimage.ImageInfos.Width;
		int imageHeight = (int)depthimage.ImageInfos.Height;
		int colorimageWidth = (int)colorimage.ImageInfos.Width;
		int colorimageHeight = (int)colorimage.ImageInfos.Height;

        //build up the user mask
        for (int destinationIndex = 0; destinationIndex < _colored_image.Length; ++destinationIndex)
        {
            //get the UV coordinates from the final texture that will be displayed
            getUV(destinationIndex, _width, _height, out destinationU, out destinationV);

            //the resolutions of the depth and label image can differ from the final texture, 
            //so we have to apply some remapping to get the equivalent UV coordinates in the depth and label image.
            getUVEquivalent(_width, _height, destinationU, destinationV, imageWidth, imageHeight, out sourceU, out sourceV, out sourceIndex);

			//Get the UV coordinates from the Registration mapping image
			float u_value = System.BitConverter.ToSingle(UVImageRaw, sourceIndex * 8);
			float v_value = System.BitConverter.ToSingle(UVImageRaw, sourceIndex * 8 + 4);
			//Negative UV values are 
			if ((u_value < 0) & (v_value < 0)) _colored_image[destinationIndex] = new Color(0, 0, 0, 1);
			else {
				colorIndex = (int)((u_value * colorimageWidth) + ((v_value - (1/colorimageHeight)) * colorimageWidth * colorimageHeight));
				if (colorIndex >921590) colorIndex = 921590; //Total number of pixels is 921600 in the 1280x720 color image
				// Extract the byte values B, G, R, A from the stored color raw data
				byte value3 = (colorImageRaw[colorIndex * 4]);
				byte value2 = (colorImageRaw[colorIndex * 4 + 1]);
				byte value = (colorImageRaw[colorIndex * 4 + 2]);
				byte value4 = (colorImageRaw[colorIndex * 4 + 3]);
				_colored_image[destinationIndex] = new Color(value * floatConvertor, value2 * floatConvertor, value3 * floatConvertor, value4 * floatConvertor); //Put the colors in RGBA form to Texture2D
			}
        }

        destinationImage.SetPixels(_colored_image);
        destinationImage.Apply();

        return true;

    }

	//Me:
	public bool generateColorMap(IImageData image, ref Texture2D destinationImage)
	{
		if (image == null)
			return false;
		
		if (image.Raw == IntPtr.Zero)
			return false;
		
		if (_colored_image == null || _colored_image.Length != image.ImageInfos.BytesRaw / 2) //this colored_image_length condition depends on the size of _width,_height that you have set with your screen display image un unity
		{
			_colored_image = new Color[_width * _height]; //The produced colored image from the camera
			imageRaw = new byte[image.ImageInfos.BytesRaw];
		}
		
		uint byte_size = (uint)image.ImageInfos.BytesRaw;

		// copy image content into managed arrays
		Marshal.Copy(image.Raw, imageRaw, 0, (int)byte_size); //Note that one byte is equal to one array index length here

		int destinationU, destinationV;
		int sourceU, sourceV;
		int sourceIndex;
		
		int imageWidth = (int)image.ImageInfos.Width;
		int imageHeight = (int)image.ImageInfos.Height;

		//build up the user mask
		for (int destinationIndex = 0; destinationIndex < _colored_image.Length; ++destinationIndex)
		{
			//get the UV coordinates from the final texture that will be displayed
			getUV(destinationIndex, _width, _height, out destinationU, out destinationV);
			
			//the resolutions of the depth and label image can differ from the final texture, 
			//so we have to apply some remapping to get the equivalent UV coordinates in the depth and label image.
			getUVEquivalent(_width, _height, destinationU, destinationV, imageWidth, imageHeight, out sourceU, out sourceV, out sourceIndex);
			
			// Extract the byte values B, G, R, A from the stored color raw data
			byte value3 = (imageRaw[sourceIndex * 4]);
			byte value2 = (imageRaw[sourceIndex * 4 + 1]);
			byte value = (imageRaw[sourceIndex * 4 + 2]);
			byte value4 = (imageRaw[sourceIndex * 4 + 3]);

			_colored_image[destinationIndex] = new Color(value * floatConvertor, value2 * floatConvertor, value3 * floatConvertor, value4 * floatConvertor); //Put the colors in RGBA form to Texture2D
		}

		destinationImage.SetPixels(_colored_image);
		destinationImage.Apply();
		
		return true;
		
	}
	
}