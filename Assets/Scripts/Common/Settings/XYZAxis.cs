﻿using UnityEngine;

namespace Elektronik.Common.Settings
{
    public class XYZAxis : MonoBehaviour
    {
        public float lengthOfAxis = 0.05f;
        static Material lineMaterial;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt(ZWrite, 0);
            }
        }

        // Will be called after all regular rendering is done
        public void OnRenderObject()
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            // Draw lines
            GL.Begin(GL.LINES);
            //Draw X axis
            GL.Color(Color.red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(lengthOfAxis, 0.0f, 0.0f);
            //Draw Y axis
            GL.Color(Color.green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0.0f, -lengthOfAxis, 0.0f);
            //Draw Z axis
            GL.Color(Color.blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0.0f, 0.0f, lengthOfAxis);
            GL.End();
            GL.PopMatrix();
        }
    }
}