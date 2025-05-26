using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshWireframeDrawer : MonoBehaviour
{
    [Header("Wireframe Settings")]
    public Color lineColor = Color.green;
    [Range(0.001f, 0.05f)]
    public float lineRadius = 0.01f;

    private static Mesh _cylinderMesh;
    private static Material _lineMaterial;
    private Mesh _mesh;

    void OnEnable()//here i am just caching all the data i need, like the mesh, line material and a basic cylinder mesh in order to draw as a line,
    //for better optimization, we can disable the mesh and also use a basic stretched plane instead of cylinders
    //for better debug, a billboard plane where the verts were.
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;

        // Cache cylinder mesh
        if (_cylinderMesh == null)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _cylinderMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
        }

        // Create basic unlit material if not already
        if (_lineMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");//the basic builtin shader, can also use unlit, for a larger support
            _lineMaterial = new Material(shader);
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnRenderObject()//here i am  setting the material as the first pass in shader, so that the next draw call will use that
    {
        if (_mesh == null || _cylinderMesh == null || _lineMaterial == null)
            return;

        var vertices = _mesh.vertices;
        var triangles = _mesh.triangles;

        _lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity); // world space drawing

        for (int i = 0; i < triangles.Length; i += 3)//draw each triangle
        {
            DrawEdge(vertices[triangles[i]], vertices[triangles[i + 1]]);//one line per edge
            DrawEdge(vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
            DrawEdge(vertices[triangles[i + 2]], vertices[triangles[i]]);
        }

        GL.PopMatrix();
    }

    void DrawEdge(Vector3 a, Vector3 b)
    {
        Vector3 worldA = transform.TransformPoint(a);//bring the point into world space
        Vector3 worldB = transform.TransformPoint(b);
        Vector3 dir = worldB - worldA;//vec A-B
        float length = dir.magnitude;

        if (length < 0.0001f) return;//not drawing the overlapped lines

        Vector3 mid = (worldA + worldB) * 0.5f;//center point to use as the position
        Quaternion rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);

        Matrix4x4 matrix = Matrix4x4.TRS(mid, rotation, new Vector3(lineRadius, lineRadius, length));//model matrix, mid is translate, rotate is rotation, and we are creating a scale, and I am using basic TRS setup to draw
        _lineMaterial.color = lineColor;
        Graphics.DrawMesh(_cylinderMesh, matrix, _lineMaterial, 0);//
    }
}
