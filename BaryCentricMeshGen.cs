using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BaryCentricMeshGen : MonoBehaviour
{
    [Tooltip("The wireframe material to switch to")]
   [SerializeField]
   Material mat;

    void OnEnable()
    {
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();

        Mesh original = mf.sharedMesh;
        Mesh wireMesh = GenerateBarycentricMesh(original);
        mf.mesh = wireMesh;
        mr.material = mat;
    }

    Mesh GenerateBarycentricMesh(Mesh input)
    {
        var triangles = input.triangles;
        var vertices = input.vertices;
        var uvs = input.uv;

        List<Vector3> newVerts = new();
        List<Color> bary = new();
        List<int> newTris = new();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            newVerts.Add(vertices[triangles[i]]);
            newVerts.Add(vertices[triangles[i + 1]]);
            newVerts.Add(vertices[triangles[i + 2]]);

            bary.Add(new Color(1, 0, 0));
            bary.Add(new Color(0, 1, 0));
            bary.Add(new Color(0, 0, 1));

            newTris.Add(i);
            newTris.Add(i + 1);
            newTris.Add(i + 2);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(newVerts);
        mesh.SetTriangles(newTris, 0);
        mesh.SetColors(bary);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
