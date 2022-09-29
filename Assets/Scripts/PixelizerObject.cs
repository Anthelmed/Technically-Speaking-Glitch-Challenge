using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PixelizerObject : MonoBehaviour
{
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private float animationSpeed = 1f;
	[SerializeField] private VoxelObject voxelObjectPrefab;
	[SerializeField] private int finalPosterizeSteps = 50;
	[SerializeField] private float explosionForce = 5;

	private List<VoxelObject> _voxelObjects = new();

	private RenderTexture _sourceRenderTexture;
	private float _animationProgression = 0;
	private bool _isAnimationPlaying = false;
	
	private static readonly int MainTex = Shader.PropertyToID("_MainTex");
	private static readonly int AnimationProgression = Shader.PropertyToID("_AnimationProgression");

	private void Update()
	{
		if (!_isAnimationPlaying) return;
		
		if (_animationProgression >= 1)
		{
			StopAnimation();
			return;
		}
		
		_animationProgression = Mathf.Min(1, _animationProgression + Time.deltaTime * animationSpeed);
		
		meshRenderer.material.SetFloat(AnimationProgression, _animationProgression);
	}
	
	private void LateUpdate()
	{
		if (!_isAnimationPlaying) return;
		
		transform.LookAt(Camera.main.transform);
		transform.Rotate(Vector3.up, 180f);
	}
	
	public void SetRenderTexture(RenderTexture renderTexture)
	{
		_sourceRenderTexture = renderTexture;
		meshRenderer.material.SetTexture(MainTex, renderTexture);
	}

	public void StartAnimation()
	{
		_isAnimationPlaying = true;
	}

	private void StopAnimation()
	{
		_isAnimationPlaying = false;
		
		var pixelsInformation = GetTexturePixelsInformation();

		InstantiateVoxels(pixelsInformation);
		meshRenderer.enabled = false;
		ExplodeVoxels();
	}

	private void InstantiateVoxels(List<PixelInformation> pixelsInformation)
	{
		var scale = transform.localScale.x / finalPosterizeSteps;
		
		foreach (var pixelInformation in pixelsInformation)
		{
			var voxelObject = Instantiate(voxelObjectPrefab);
			var offset = new Vector3(pixelInformation.Position.x, pixelInformation.Position.y);
			
			voxelObject.transform.SetPositionAndRotation( transform.position + offset, transform.rotation);
			voxelObject.transform.localScale = Vector3.one * scale;
			voxelObject.SetColor(pixelInformation.Color);
			
			_voxelObjects.Add(voxelObject);
		}
	}

	private void ExplodeVoxels()
	{
		var origin = transform.position;
		origin.y = 0;
		
		foreach (var voxel in _voxelObjects)
		{
			voxel.Explode(origin, explosionForce);
		}
	}
	
	private List<PixelInformation> GetTexturePixelsInformation()
	{
		var pixelsInformation = new List<PixelInformation>();
		var texture = RenderTextureTo2DTexture(_sourceRenderTexture);
		var pixels = texture.GetPixels();
		var scale = transform.localScale.x / finalPosterizeSteps;
		var globalOffset = transform.localScale.x / 2f;
		var localOffset = scale / 2f;
		var step = _sourceRenderTexture.width / finalPosterizeSteps;

		for (var y = 0; y < texture.height; y+=step)
		{
			for (var x = 0; x < texture.width; x+=step)
			{
				var color = pixels[y * texture.width + x];
			
				if (color.a <= 0.01f) continue;
				
				pixelsInformation.Add(new PixelInformation(new Vector2((float)x / step * scale - globalOffset + localOffset,(float)y / step * scale - globalOffset - localOffset), color));
			}
		}

		return pixelsInformation;
	}
	

	public struct PixelInformation
	{
		public Vector2 Position;
		public Color Color;

		public PixelInformation(Vector2 position, Color color)
		{
			Position = position;
			Color = color;
		}
	}
	
	private Texture2D RenderTextureTo2DTexture(RenderTexture renderTexture)
	{
		var texture = new Texture2D(renderTexture.width, renderTexture.height, renderTexture.graphicsFormat, 0, TextureCreationFlags.None);
		RenderTexture.active = renderTexture;
		texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();
        
		RenderTexture.active = null;

		return texture;
	}
}