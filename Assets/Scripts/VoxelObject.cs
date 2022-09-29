using UnityEngine;

public class VoxelObject : MonoBehaviour
{
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Rigidbody rigidbody;
	private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

	public void SetColor(Color color)
	{
		meshRenderer.material.SetColor(BaseColor, color);
	}

	public void Explode(Vector3 origin, float force)
	{
		rigidbody.AddForceAtPosition(Vector3.up * force, origin, ForceMode.Impulse);
	}
}