using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Line : MonoBehaviour {
	public List<Vector3> Points = new List<Vector3>();
	static Material lineMaterial;
	static void CreateLineMaterial() {
		if( lineMaterial == null ) {
			lineMaterial = 
				new Material( 
				          @"Shader ""Lines/Colored Blended"" {
							SubShader {
							Tags { ""RenderType""=""Opaque"" }
							Pass {
							ZWrite On
							ZTest LEqual
							Cull Off
							Fog { Mode Off }
							BindChannels {
							Bind ""vertex"", vertex Bind ""color"", color
							} } } }");
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
	
	void Start () 
	{
		Debug.Log (Points.ToString ());
	}

	public void DrawLines() {
		CreateLineMaterial();
		GL.PushMatrix ();
		lineMaterial.SetPass( 0 );
		GL.LoadOrtho ();
		GL.Color (Color.green);
		for (int i = 0; i < Points.Count; i++)
			GL.Vertex3 (Points [i].x, Points[i].y, -0.5f );
		GL.End();
		GL.PopMatrix ();
		}
	void OnPostRender() {
		DrawLines ();
	}
}
