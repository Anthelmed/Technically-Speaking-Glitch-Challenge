using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class PixelizerSystem : MonoBehaviour
{
	[SerializeField] private ScriptableRenderContext secondaryCameraRenderContext;
	[SerializeField] private Camera secondaryCamera;
	[SerializeField] private Transform gun;
	[SerializeField] private LayerMask pixelizerLayer;
	[SerializeField] private LayerMask enemyLayer;
	[SerializeField] private PixelizerObject pixelizerObjectPrefab;
	[SerializeField] private Vector2 minMaxDeathAnimationDuration = new (0.5f, 1.5f);
	
	private RenderTexture _secondaryCameraRenderTexture;
	private static readonly int Death = Animator.StringToHash("Death");

	private void Start()
	{
		_secondaryCameraRenderTexture = new RenderTexture(1080, 1080, 16);
		secondaryCamera.targetTexture = _secondaryCameraRenderTexture;
	}

	private void Update()
	{
		if (!Mouse.current.leftButton.wasPressedThisFrame) return;
		
		StartCoroutine(Shoot(gun.position, gun.forward));
	}

	
	private IEnumerator Shoot(Vector3 origin, Vector3 direction)
	{
		if (!Physics.Raycast(origin, direction, out var hit, Mathf.Infinity, enemyLayer.value)) yield return null;

		var viewDirection = -secondaryCamera.transform.forward;
		var animator = hit.transform.GetComponent<Animator>();
		
		PlayDeathAnimation(animator);

		var animationDuration = Random.Range(minMaxDeathAnimationDuration.x, minMaxDeathAnimationDuration.y);

		yield return new WaitForSeconds(animationDuration);
		
		SetLayerAllChildren(hit.transform, pixelizerLayer.MaskToLayer());

		yield return new WaitForEndOfFrame();
		
		InstantiatePixelizerObject(hit, viewDirection);

		hit.transform.gameObject.SetActive(false);
	}

	private void PlayDeathAnimation(Animator animator)
	{
		animator.SetBool(Death, true);
	}

	private void InstantiatePixelizerObject(RaycastHit raycastHit, Vector3 viewDirection)
	{
		var position = raycastHit.point;
		var rotation = Quaternion.Euler(viewDirection);
		var distance = raycastHit.distance;
		
		var height = 2.0f * Mathf.Tan(0.5f * secondaryCamera.fieldOfView * Mathf.Deg2Rad) * distance;
		var width = height * Screen.width / Screen.height;
			
		var pixelizerObject = Instantiate(pixelizerObjectPrefab);
		pixelizerObject.transform.SetPositionAndRotation(position, rotation);
		pixelizerObject.transform.localScale = new Vector3(height, height, 0f);
			
		var texture = TakeScreenshotFromSecondaryCamera();
			
		pixelizerObject.SetRenderTexture(texture);
		pixelizerObject.StartAnimation();
	}
	
	void SetLayerAllChildren(Transform root, int layer)
	{
		var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (var child in children)
		{
			child.gameObject.layer = layer;
		}
	}
	
	private RenderTexture TakeScreenshotFromSecondaryCamera()
	{
		var screenTexture = new RenderTexture(_secondaryCameraRenderTexture);
		
		Graphics.CopyTexture(_secondaryCameraRenderTexture, screenTexture);

		return screenTexture;
	}
}
