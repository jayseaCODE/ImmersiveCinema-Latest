#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5
#define UNITY_FEATURE_UGUI
#endif

using System.Collections.Generic;
using UnityEngine.Serialization;
#if UNITY_FEATURE_UGUI
using UnityEngine;

[AddComponentMenu("AVPro Windows Media/uGUI Movie")]
public class AVProWindowsMediaUGUIComponent : UnityEngine.UI.MaskableGraphic
{
	[SerializeField]
	public AVProWindowsMediaMovie m_movie;

	[SerializeField]
	public Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

	public bool _setNativeSize = false;

	[SerializeField]
	public Texture _defaultTexture;

	private int _lastWidth;
	private int _lastHeight;

	protected AVProWindowsMediaUGUIComponent()
	{ }


	/// <summary>
	/// Returns the texture used to draw this Graphic.
	/// </summary>
	public override Texture mainTexture
	{
		get
		{
			Texture result = Texture2D.whiteTexture;
			if (HasValidTexture())
			{
				result = m_movie.OutputTexture;
			}
			else
			{
				if (_defaultTexture != null)
				{
					result = _defaultTexture;
				}
			}
			return result;
		}
	}

	public bool HasValidTexture()
	{
		return (m_movie != null && m_movie.OutputTexture != null);
	}

	void Update()
	{
		if (mainTexture == null)
			return;

		if (_setNativeSize)
			SetNativeSize();
		if (HasValidTexture())
		{
			if (mainTexture.width != _lastWidth ||
			    mainTexture.height != _lastHeight)
			{
				_lastWidth = mainTexture.width;
				_lastHeight = mainTexture.height;
				SetVerticesDirty();
			}
		}
		SetMaterialDirty();
	}

	/// <summary>
	/// Texture to be used.
	/// </summary>
	public AVProWindowsMediaMovie texture
	{
		get
		{
			return m_movie;
		}
		set
		{
			if (m_movie == value)
				return;

			m_movie = value;
			//SetVerticesDirty();
			SetMaterialDirty();
		}
	}

	/// <summary>
	/// UV rectangle used by the texture.
	/// </summary>
	public Rect uvRect
	{
		get
		{
			return m_UVRect;
		}
		set
		{
			if (m_UVRect == value)
				return;
			m_UVRect = value;
			SetVerticesDirty();
		}
	}

	/// <summary>
	/// Adjust the scale of the Graphic to make it pixel-perfect.
	/// </summary>

	[ContextMenu("Set Native Size")]
	public override void SetNativeSize()
	{
		Texture tex = mainTexture;
		if (tex != null)
		{
			int w = Mathf.RoundToInt(tex.width * uvRect.width);
			int h = Mathf.RoundToInt(tex.height * uvRect.height);
			rectTransform.anchorMax = rectTransform.anchorMin;
			rectTransform.sizeDelta = new Vector2(w, h);
		}
	}

	/// <summary>
	/// Update all renderer data.
	/// </summary>
	protected override void OnFillVBO(List<UIVertex> vbo)
	{
		Texture tex = mainTexture;

		int texWidth = 4;
		int texHeight = 4;
		bool flipY = false;

		if (HasValidTexture())
		{
			flipY = m_movie.MovieInstance.RequiresFlipY;
		}

		if (tex != null)
		{
			texWidth = tex.width;
			texHeight = tex.height;
		}

		Vector4 v = Vector4.zero;

		int w = Mathf.RoundToInt(tex.width * uvRect.width);
		int h = Mathf.RoundToInt(tex.height * uvRect.height);

		float paddedW = ((w & 1) == 0) ? w : w + 1;
		float paddedH = ((h & 1) == 0) ? h : h + 1;

		v.x = 0f;
		v.y = 0f;
		v.z = w / paddedW;
		v.w = h / paddedH;

		v.x -= rectTransform.pivot.x;
		v.y -= rectTransform.pivot.y;
		v.z -= rectTransform.pivot.x;
		v.w -= rectTransform.pivot.y;

		v.x *= rectTransform.rect.width;
		v.y *= rectTransform.rect.height;
		v.z *= rectTransform.rect.width;
		v.w *= rectTransform.rect.height;

		vbo.Clear();

		var vert = UIVertex.simpleVert;

		vert.position = new Vector2(v.x, v.y);
		vert.uv0 = new Vector2(m_UVRect.xMin, m_UVRect.yMin);
		if (flipY)
		{
			vert.uv0 = new Vector2(m_UVRect.xMin, 1.0f - m_UVRect.yMin);
		}
		vert.color = color;
		vbo.Add(vert);

		vert.position = new Vector2(v.x, v.w);
		vert.uv0 = new Vector2(m_UVRect.xMin, m_UVRect.yMax);
		if (flipY)
		{
			vert.uv0 = new Vector2(m_UVRect.xMin, 1.0f - m_UVRect.yMax);
		}
		vert.color = color;
		vbo.Add(vert);

		vert.position = new Vector2(v.z, v.w);
		vert.uv0 = new Vector2(m_UVRect.xMax, m_UVRect.yMax);
		if (flipY)
		{
			vert.uv0 = new Vector2(m_UVRect.xMax, 1.0f - m_UVRect.yMax);
		}
		vert.color = color;
		vbo.Add(vert);

		vert.position = new Vector2(v.z, v.y);
		vert.uv0 = new Vector2(m_UVRect.xMax, m_UVRect.yMin);
		if (flipY)
		{
			vert.uv0 = new Vector2(m_UVRect.xMax, 1.0f - m_UVRect.yMin);
		}
		vert.color = color;
		vbo.Add(vert);
	}
}

#endif