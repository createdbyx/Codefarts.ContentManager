using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// The geometry buffer.
/// </summary>
public class GeometryBuffer
{
    /// <summary>
    /// The objects.
    /// </summary>
    private List<ObjectData> objects;

    /// <summary>
    /// The vertices.
    /// </summary>
    public List<Vector3> vertices;

    /// <summary>
    /// The uvs.
    /// </summary>
    public List<Vector2> uvs;

    /// <summary>
    /// The normals.
    /// </summary>
    public List<Vector3> normals;

    /// <summary>
    /// The unnamed group index.
    /// </summary>
    public int unnamedGroupIndex = 1; // naming index for unnamed group. like "Unnamed-1"

    /// <summary>
    /// The current.
    /// </summary>
    private ObjectData current;

    /// <summary>
    /// The object data.
    /// </summary>
    private class ObjectData
    {
        /// <summary>
        /// The name.
        /// </summary>
        public string name;

        /// <summary>
        /// The groups.
        /// </summary>
        public List<GroupData> groups;

        /// <summary>
        /// The all faces.
        /// </summary>
        public List<FaceIndices> allFaces;

        /// <summary>
        /// The normal count.
        /// </summary>
        public int normalCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectData"/> class.
        /// </summary>
        public ObjectData()
        {
            groups = new List<GroupData>();
            allFaces = new List<FaceIndices>();
            normalCount = 0;
        }
    }

    /// <summary>
    /// The curgr.
    /// </summary>
    private GroupData curgr;

    /// <summary>
    /// The group data.
    /// </summary>
    private class GroupData
    {
        /// <summary>
        /// The name.
        /// </summary>
        public string name;

        /// <summary>
        /// The material name.
        /// </summary>
        public string materialName;

        /// <summary>
        /// The faces.
        /// </summary>
        public List<FaceIndices> faces;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupData"/> class.
        /// </summary>
        public GroupData()
        {
            faces = new List<FaceIndices>();
        }

        /// <summary>
        /// Gets a value indicating whether is empty.
        /// </summary>
        public bool isEmpty { get { return faces.Count == 0; } }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeometryBuffer"/> class.
    /// </summary>
    public GeometryBuffer()
    {
        objects = new List<ObjectData>();
        var d = new ObjectData();
        d.name = "default";
        objects.Add(d);
        current = d;

        var g = new GroupData();
        g.name = "default";
        d.groups.Add(g);
        curgr = g;

        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        normals = new List<Vector3>();
    }

    /// <summary>
    /// The push object.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    public void PushObject(string name)
    {
        // Debug.Log("Adding new object " + name + ". Current is empty: " + isEmpty);
        if (isEmpty) objects.Remove(current);

        var n = new ObjectData();
        n.name = name;
        objects.Add(n);

        var g = new GroupData();
        g.name = "default";
        n.groups.Add(g);

        curgr = g;
        current = n;
    }

    /// <summary>
    /// The push group.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    public void PushGroup(string name)
    {
        if (curgr.isEmpty) current.groups.Remove(curgr);
        var g = new GroupData();
        if (name == null)
        {
            name = "Unnamed-" + unnamedGroupIndex;
            unnamedGroupIndex++;
        }

        g.name = name;
        current.groups.Add(g);
        curgr = g;
    }

    /// <summary>
    /// The push material name.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    public void PushMaterialName(string name)
    {
        // Debug.Log("Pushing new material " + name + " with curgr.empty=" + curgr.isEmpty);
        if (!curgr.isEmpty) PushGroup(name);
        if (curgr.name == "default") curgr.name = name;
        curgr.materialName = name;
    }

    /// <summary>
    /// The push vertex.
    /// </summary>
    /// <param name="v">
    /// The v.
    /// </param>
    public void PushVertex(Vector3 v)
    {
        vertices.Add(v);
    }

    /// <summary>
    /// The push uv.
    /// </summary>
    /// <param name="v">
    /// The v.
    /// </param>
    public void PushUV(Vector2 v)
    {
        uvs.Add(v);
    }

    /// <summary>
    /// The push normal.
    /// </summary>
    /// <param name="v">
    /// The v.
    /// </param>
    public void PushNormal(Vector3 v)
    {
        normals.Add(v);
    }

    /// <summary>
    /// The push face.
    /// </summary>
    /// <param name="f">
    /// The f.
    /// </param>
    public void PushFace(FaceIndices f)
    {
        curgr.faces.Add(f);
        current.allFaces.Add(f);
        if (f.vn >= 0)
        {
            current.normalCount++;
        }
    }

    /// <summary>
    /// The trace.
    /// </summary>
    public void Trace()
    {
        Debug.Log("OBJ has " + objects.Count + " object(s)");
        Debug.Log("OBJ has " + vertices.Count + " vertice(s)");
        Debug.Log("OBJ has " + uvs.Count + " uv(s)");
        Debug.Log("OBJ has " + normals.Count + " normal(s)");
        foreach (var od in objects)
        {
            Debug.Log(od.name + " has " + od.groups.Count + " group(s)");
            foreach (var gd in od.groups)
            {
                Debug.Log(od.name + "/" + gd.name + " has " + gd.faces.Count + " faces(s)");
            }
        }

    }

    /// <summary>
    /// Gets the num objects.
    /// </summary>
    public int numObjects { get { return objects.Count; } }

    /// <summary>
    /// Gets a value indicating whether is empty.
    /// </summary>
    public bool isEmpty { get { return vertices.Count == 0; } }

    /// <summary>
    /// Gets a value indicating whether has u vs.
    /// </summary>
    public bool hasUVs { get { return uvs.Count > 0; } }

    /// <summary>
    /// Gets a value indicating whether has normals.
    /// </summary>
    public bool hasNormals { get { return normals.Count > 0; } }

    /// <summary>
    /// The ma x_ vertice s_ limi t_ fo r_ a_ mesh.
    /// </summary>
    public static int MAX_VERTICES_LIMIT_FOR_A_MESH = 64999;

    /// <summary>
    /// The populate meshes.
    /// </summary>
    /// <param name="gs">
    /// The gs.
    /// </param>
    /// <param name="mats">
    /// The mats.
    /// </param>
    public void PopulateMeshes(GameObject[] gs, Dictionary<string, Material> mats)
    {
        if (gs.Length != numObjects) return; // Should not happen unless obj file is corrupt...
        Debug.Log("PopulateMeshes GameObjects count:" + gs.Length);
        for (var i = 0; i < gs.Length; i++)
        {
            var od = objects[i];
            var objectHasNormals = hasNormals && od.normalCount > 0;

            if (od.name != "default") gs[i].name = od.name;
            Debug.Log("PopulateMeshes object name:" + od.name);

            var tvertices = new Vector3[od.allFaces.Count];
            var tuvs = new Vector2[od.allFaces.Count];
            var tnormals = new Vector3[od.allFaces.Count];

            var k = 0;
            foreach (var fi in od.allFaces)
            {
                if (k >= MAX_VERTICES_LIMIT_FOR_A_MESH)
                {
                    Debug.LogWarning("maximum vertex number for a mesh exceeded for object:" + gs[i].name);
                    break;
                }

                tvertices[k] = vertices[fi.vi];
                if (hasUVs) tuvs[k] = uvs[fi.vu];
                if (hasNormals && fi.vn >= 0) tnormals[k] = normals[fi.vn];
                k++;
            }

            var meshFilter = gs[i].GetComponent<MeshFilter>().mesh;
            meshFilter.vertices = tvertices;
            if (this.hasUVs)
            {
                meshFilter.uv = tuvs;
            }

            if (objectHasNormals)
            {
                meshFilter.normals = tnormals;
            }

            if (od.groups.Count == 1)
            {
                Debug.Log("PopulateMeshes only one group: " + od.groups[0].name);
                var gd = od.groups[0];
                var matName = (gd.materialName != null) ? gd.materialName : "default"; // MAYBE: "default" may not enough.
                if (mats.ContainsKey(matName))
                {
                    gs[i].renderer.material = mats[matName];
                    Debug.Log("PopulateMeshes mat:" + matName + " set.");
                }
                else
                {
                    Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");
                }

                var triangles = new int[gd.faces.Count];
                for (var j = 0; j < triangles.Length; j++)
                {
                    triangles[j] = j;
                }

                meshFilter.triangles = triangles;  
            }
            else
            {
                var gl = od.groups.Count;
                var materials = new Material[gl];
                meshFilter.subMeshCount = gl;
                var c = 0;

                Debug.Log("PopulateMeshes group count:" + gl);
                for (var j = 0; j < gl; j++)
                {
                    var matName = (od.groups[j].materialName != null) ? od.groups[j].materialName : "default"; // MAYBE: "default" may not enough.
                    if (mats.ContainsKey(matName))
                    {
                        materials[j] = mats[matName];
                        Debug.Log("PopulateMeshes mat:" + matName + " set.");
                    }
                    else
                    {
                        Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");
                    }

                    var triangles = new int[od.groups[j].faces.Count];
                    var l = od.groups[j].faces.Count + c;
                    var s = 0;
                    for (; c < l; c++, s++)
                    {
                        triangles[s] = c;
                    }

                    meshFilter.SetTriangles(triangles, j);
                }

                gs[i].renderer.materials = materials;
            }

            if (!objectHasNormals)
            {
                meshFilter.RecalculateNormals();
            }
        }
    }
}