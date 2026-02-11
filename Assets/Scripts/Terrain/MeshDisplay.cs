using UnityEngine;

public class MeshDisplay : MonoBehaviour 
{
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;

	public void DrawMesh(MeshData meshData) 
	{
		meshFilter.sharedMesh = meshData.CreateMesh ();
	}
}