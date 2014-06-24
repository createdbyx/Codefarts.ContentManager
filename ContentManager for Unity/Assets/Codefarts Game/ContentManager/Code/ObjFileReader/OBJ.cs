using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

/// <summary>
/// The obj.
/// </summary>
public class OBJ
{
    /// <summary>
    /// The obj path.
    /// </summary>
    public string objPath;

    /* OBJ file tags */

    /// <summary>
    /// The o.
    /// </summary>
    private const string O = "o";

    /// <summary>
    /// The g.
    /// </summary>
    private const string G = "g";

    /// <summary>
    /// The v.
    /// </summary>
    private const string V = "v";

    /// <summary>
    /// The vt.
    /// </summary>
    private const string VT = "vt";

    /// <summary>
    /// The vn.
    /// </summary>
    private const string VN = "vn";

    /// <summary>
    /// The f.
    /// </summary>
    private const string F = "f";

    /// <summary>
    /// The mtl.
    /// </summary>
    private const string MTL = "mtllib";

    /// <summary>
    /// The uml.
    /// </summary>
    private const string UML = "usemtl";

    /* MTL file tags */

    /// <summary>
    /// The nml.
    /// </summary>
    private const string NML = "newmtl";

    /// <summary>
    /// The ns.
    /// </summary>
    private const string NS = "Ns"; // Shininess

    /// <summary>
    /// The ka.
    /// </summary>
    private const string KA = "Ka"; // Ambient component (not supported)

    /// <summary>
    /// The kd.
    /// </summary>
    private const string KD = "Kd"; // Diffuse component

    /// <summary>
    /// The ks.
    /// </summary>
    private const string KS = "Ks"; // Specular component

    /// <summary>
    /// The d.
    /// </summary>
    private const string D = "d"; // Transparency (not supported)

    /// <summary>
    /// The tr.
    /// </summary>
    private const string TR = "Tr"; // Same as 'd'

    /// <summary>
    /// The illum.
    /// </summary>
    private const string ILLUM = "illum"; // Illumination model. 1 - diffuse, 2 - specular

    /// <summary>
    /// The ma p_ ka.
    /// </summary>
    private const string MAP_KA = "map_Ka"; // Ambient texture

    /// <summary>
    /// The ma p_ kd.
    /// </summary>
    private const string MAP_KD = "map_Kd"; // Diffuse texture

    /// <summary>
    /// The ma p_ ks.
    /// </summary>
    private const string MAP_KS = "map_Ks"; // Specular texture

    /// <summary>
    /// The ma p_ ke.
    /// </summary>
    private const string MAP_KE = "map_Ke"; // Emissive texture

    /// <summary>
    /// The ma p_ bump.
    /// </summary>
    private const string MAP_BUMP = "map_bump"; // Bump map texture

    /// <summary>
    /// The bump.
    /// </summary>
    private const string BUMP = "bump"; // Bump map texture

    /// <summary>
    /// The basepath.
    /// </summary>
    private string basepath;

    /// <summary>
    /// The mtllib.
    /// </summary>
    private string mtllib;

    /// <summary>
    /// The load.
    /// </summary>
    /// <param name="path">
    ///     The path.
    /// </param>
    /// <param name="buffer"></param>
    /// <param name="materials"></param>
    /// <returns>
    /// The <see cref="GeometryBuffer"/>.
    /// </returns>
    public void Load(string path, out GeometryBuffer buffer, out Dictionary<string, Material> materials)
    {
         buffer = new GeometryBuffer();  

        this.basepath = path.IndexOf("/") == -1 ? string.Empty : path.Substring(0, path.LastIndexOf("/") + 1);

        // WWW loader = new WWW(path);
        // yield return loader;
        var fileData = File.ReadAllText(path);
        this.SetGeometryData(buffer, fileData);

        if (this.hasMaterials)
        {
            // loader = new WWW(basepath + mtllib);
            Debug.Log("base path = " + this.basepath);
            Debug.Log("MTL path = " + (this.basepath + this.mtllib));

            // yield return loader;
            // if (loader.error != null) {
            // 	Debug.LogError(loader.error);
            // 	}
            // else {
            this.SetMaterialData(fileData);

            // 	}

            // foreach(MaterialData m in materialData) {
            // if(m.diffuseTexPath != null) {
            // WWW texloader = GetTextureLoader(m, m.diffuseTexPath);
            // yield return texloader;
            // if (texloader.error != null) {
            // Debug.LogError(texloader.error);
            // } else {
            // m.diffuseTex = texloader.texture;
            // }
            // }
            // if(m.bumpTexPath != null) {
            // WWW texloader = GetTextureLoader(m, m.bumpTexPath);
            // yield return texloader;
            // if (texloader.error != null) {
            // Debug.LogError(texloader.error);
            // } else {
            // m.bumpTex = texloader.texture;
            // }
            // }
            // }
            foreach (var m in materialData)
            {
                if (m.diffuseTexPath != null)
                {
                    m.diffuseTex = this.GetTextureLoader(m, m.diffuseTexPath);
                }

                if (m.bumpTexPath != null)
                {
                    m.bumpTex = this.GetTextureLoader(m, m.bumpTexPath);
                }
            }
        }

          materials = new Dictionary<string, Material>();
           if (this.hasMaterials)
        {
            foreach (var data in this.materialData)
            {
                if (materials.ContainsKey(data.name))
                {
                    Debug.LogWarning("duplicate material found: " + data.name + ". ignored repeated occurrences");
                    continue;
                }

                materials.Add(data.name, this.GetMaterial(data));
            }
        }
        else
        {
            materials.Add("default", new Material(Shader.Find("VertexLit")));
        }

      //  Build(buffer);

       // return buffer;
    }

    /// <summary>
    /// The get texture loader.
    /// </summary>
    /// <param name="m">
    /// The m.
    /// </param>
    /// <param name="texpath">
    /// The texpath.
    /// </param>
    /// <returns>
    /// The <see cref="Texture2D"/>.
    /// </returns>
    private Texture2D GetTextureLoader(MaterialData m, string texpath)
    {
        char[] separators = { '/', '\\' };
        var components = texpath.Split(separators);
        var filename = components[components.Length - 1];
        var ext = Path.GetExtension(filename).ToLower();
        if (ext != ".png" && ext != ".jpg")
        {
            Debug.LogWarning("maybe unsupported texture format:" + ext);
        }

        var tex = new Texture2D(4, 4);
        tex.LoadImage(File.ReadAllBytes(basepath + filename));

        // WWW texloader = new WWW(basepath + filename);
        Debug.Log("texture path for material(" + m.name + ") = " + (basepath + filename));
        return tex;
    }

    // private WWW GetTextureLoader(MaterialData m, string texpath) {
    // char[] separators = {'/', '\\'};
    // string[] components = texpath.Split(separators);
    // string filename = components[components.Length-1];
    // string ext = Path.GetExtension(filename).ToLower();
    // if (ext != ".png" && ext != ".jpg") {
    // Debug.LogWarning("maybe unsupported texture format:"+ext);
    // }
    // WWW texloader = new WWW(basepath + filename);
    // Debug.Log("texture path for material("+m.name+") = "+(basepath + filename));
    // return texloader;
    // }

    /// <summary>
    /// The get face indices by one face line.
    /// </summary>
    /// <param name="buffer">
    /// The buffer.
    /// </param>
    /// <param name="faces">
    /// The faces.
    /// </param>
    /// <param name="p">
    /// The p.
    /// </param>
    /// <param name="isFaceIndexPlus">
    /// The is face index plus.
    /// </param>
    private void GetFaceIndicesByOneFaceLine(GeometryBuffer buffer, FaceIndices[] faces, string[] p, bool isFaceIndexPlus)
    {
        if (isFaceIndexPlus)
        {
            for (var j = 1; j < p.Length; j++)
            {
                var c = p[j].Trim().Split("/".ToCharArray());
                var fi = new FaceIndices();

                // vertex
                var vi = ci(c[0]);
                fi.vi = vi - 1;

                // uv
                if (c.Length > 1 && c[1] != string.Empty)
                {
                    var vu = ci(c[1]);
                    fi.vu = vu - 1;
                }

                // normal
                if (c.Length > 2 && c[2] != string.Empty)
                {
                    var vn = ci(c[2]);
                    fi.vn = vn - 1;
                }
                else
                {
                    fi.vn = -1;
                }

                faces[j - 1] = fi;
            }
        }
        else
        { // for minus index
            var vertexCount = buffer.vertices.Count;
            var uvCount = buffer.uvs.Count;
            for (var j = 1; j < p.Length; j++)
            {
                var c = p[j].Trim().Split("/".ToCharArray());
                var fi = new FaceIndices();

                // vertex
                var vi = ci(c[0]);
                fi.vi = vertexCount + vi;

                // uv
                if (c.Length > 1 && c[1] != string.Empty)
                {
                    var vu = ci(c[1]);
                    fi.vu = uvCount + vu;
                }

                // normal
                if (c.Length > 2 && c[2] != string.Empty)
                {
                    var vn = ci(c[2]);
                    fi.vn = vertexCount + vn;
                }
                else
                {
                    fi.vn = -1;
                }

                faces[j - 1] = fi;
            }
        }
    }

    /// <summary>
    /// The set geometry data.
    /// </summary>
    /// <param name="buffer">
    /// The buffer.
    /// </param>
    /// <param name="data">
    /// The data.
    /// </param>
    private void SetGeometryData(GeometryBuffer buffer, string data)
    {
        var lines = data.Split("\n".ToCharArray());
        var regexWhitespaces = new Regex(@"\s+");
        var isFirstInGroup = true;
        var isFaceIndexPlus = true;
        for (var i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();

            if (l.IndexOf("#") != -1)
            { // comment line
                continue;
            }

            var p = regexWhitespaces.Split(l);
            switch (p[0])
            {
                case O:
                    buffer.PushObject(p[1].Trim());
                    isFirstInGroup = true;
                    break;
                case G:
                    string groupName = null;
                    if (p.Length >= 2)
                    {
                        groupName = p[1].Trim();
                    }

                    isFirstInGroup = true;
                    buffer.PushGroup(groupName);
                    break;
                case V:
                    buffer.PushVertex(new Vector3(cf(p[1]), cf(p[2]), cf(p[3])));
                    break;
                case VT:
                    buffer.PushUV(new Vector2(cf(p[1]), cf(p[2])));
                    break;
                case VN:
                    buffer.PushNormal(new Vector3(cf(p[1]), cf(p[2]), cf(p[3])));
                    break;
                case F:
                    var faces = new FaceIndices[p.Length - 1];
                    if (isFirstInGroup)
                    {
                        isFirstInGroup = false;
                        var c = p[1].Trim().Split("/".ToCharArray());
                        isFaceIndexPlus = ci(c[0]) >= 0;
                    }

                    GetFaceIndicesByOneFaceLine(buffer, faces, p, isFaceIndexPlus);
                    if (p.Length == 4)
                    {
                        buffer.PushFace(faces[0]);
                        buffer.PushFace(faces[1]);
                        buffer.PushFace(faces[2]);
                    }
                    else if (p.Length == 5)
                    {
                        buffer.PushFace(faces[0]);
                        buffer.PushFace(faces[1]);
                        buffer.PushFace(faces[3]);
                        buffer.PushFace(faces[3]);
                        buffer.PushFace(faces[1]);
                        buffer.PushFace(faces[2]);
                    }
                    else
                    {
                        Debug.LogWarning("face vertex count :" + (p.Length - 1) + " larger than 4:");
                    }

                    break;
                case MTL:
                    mtllib = l.Substring(p[0].Length + 1).Trim();
                    break;
                case UML:
                    buffer.PushMaterialName(p[1].Trim());
                    break;
            }
        }

        // buffer.Trace();
    }

    /// <summary>
    /// The cf.
    /// </summary>
    /// <param name="v">
    /// The v.
    /// </param>
    /// <returns>
    /// The <see cref="float"/>.
    /// </returns>
    private float cf(string v)
    {
        try
        {
            return float.Parse(v);
        }
        catch (Exception e)
        {
            // print(e);
            return 0;
        }
    }

    /// <summary>
    /// The ci.
    /// </summary>
    /// <param name="v">
    /// The v.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    private int ci(string v)
    {
        try
        {
            return int.Parse(v);
        }
        catch (Exception e)
        {
            // 	print(e);
            return 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether has materials.
    /// </summary>
    private bool hasMaterials
    {
        get
        {
            return mtllib != null;
        }
    }

    /* ############## MATERIALS */

    /// <summary>
    /// The material data.
    /// </summary>
    private List<MaterialData> materialData;

    /// <summary>
    /// The material data.
    /// </summary>
    private class MaterialData
    {
        /// <summary>
        /// The name.
        /// </summary>
        public string name;

        /// <summary>
        /// The ambient.
        /// </summary>
        public Color ambient;

        /// <summary>
        /// The diffuse.
        /// </summary>
        public Color diffuse;

        /// <summary>
        /// The specular.
        /// </summary>
        public Color specular;

        /// <summary>
        /// The shininess.
        /// </summary>
        public float shininess;

        /// <summary>
        /// The alpha.
        /// </summary>
        public float alpha;

        /// <summary>
        /// The illum type.
        /// </summary>
        public int illumType;

        /// <summary>
        /// The diffuse tex path.
        /// </summary>
        public string diffuseTexPath;

        /// <summary>
        /// The bump tex path.
        /// </summary>
        public string bumpTexPath;

        /// <summary>
        /// The diffuse tex.
        /// </summary>
        public Texture2D diffuseTex;

        /// <summary>
        /// The bump tex.
        /// </summary>
        public Texture2D bumpTex;
    }

    /// <summary>
    /// The set material data.
    /// </summary>
    /// <param name="data">
    /// The data.
    /// </param>
    private void SetMaterialData(string data)
    {
        var lines = data.Split("\n".ToCharArray());

        materialData = new List<MaterialData>();
        var current = new MaterialData();
        var regexWhitespaces = new Regex(@"\s+");

        for (var i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();

            if (l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));
            var p = regexWhitespaces.Split(l);
            if (p[0].Trim() == string.Empty) continue;

            switch (p[0])
            {
                case NML:
                    current = new MaterialData();
                    current.name = p[1].Trim();
                    materialData.Add(current);
                    break;
                case KA:
                    current.ambient = gc(p);
                    break;
                case KD:
                    current.diffuse = gc(p);
                    break;
                case KS:
                    current.specular = gc(p);
                    break;
                case NS:
                    current.shininess = cf(p[1]) / 1000;
                    break;
                case D:
                case TR:
                    current.alpha = cf(p[1]);
                    break;
                case MAP_KD:
                    current.diffuseTexPath = p[p.Length - 1].Trim();
                    break;
                case MAP_BUMP:
                case BUMP:
                    BumpParameter(current, p);
                    break;
                case ILLUM:
                    current.illumType = ci(p[1]);
                    break;
                default:
                    Debug.Log("this line was not processed :" + l);
                    break;
            }
        }
    }

    /// <summary>
    /// The get material.
    /// </summary>
    /// <param name="materialInformation">
    /// The md.
    /// </param>
    /// <returns>
    /// The <see cref="Material"/>.
    /// </returns>
    private Material GetMaterial(MaterialData materialInformation)
    {
        Material material;

        if (materialInformation.illumType == 2)
        {
            var shaderName = (materialInformation.bumpTex != null) ? "Bumped Specular" : "Specular";
            material = new Material(Shader.Find(shaderName));
            material.SetColor("_SpecColor", materialInformation.specular);
            material.SetFloat("_Shininess", materialInformation.shininess);
        }
        else
        {
            var shaderName = (materialInformation.bumpTex != null) ? "Bumped Diffuse" : "Diffuse";
            material = new Material(Shader.Find(shaderName));
        }

        if (materialInformation.diffuseTex != null)
        {
            material.SetTexture("_MainTex", materialInformation.diffuseTex);
        }
        else
        {
            material.SetColor("_Color", materialInformation.diffuse);
        }

        if (materialInformation.bumpTex != null)
        {
            material.SetTexture("_BumpMap", materialInformation.bumpTex);
        }

        material.name = materialInformation.name;

        return material;
    }

    /// <summary>
    /// The bump param def.
    /// </summary>
    private class BumpParamDef
    {
        /// <summary>
        /// The option name.
        /// </summary>
        public string optionName;

        /// <summary>
        /// The value type.
        /// </summary>
        public string valueType;

        /// <summary>
        /// The value num min.
        /// </summary>
        public int valueNumMin;

        /// <summary>
        /// The value num max.
        /// </summary>
        public int valueNumMax;

        /// <summary>
        /// Initializes a new instance of the <see cref="BumpParamDef"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="numMin">
        /// The num min.
        /// </param>
        /// <param name="numMax">
        /// The num max.
        /// </param>
        public BumpParamDef(string name, string type, int numMin, int numMax)
        {
            this.optionName = name;
            this.valueType = type;
            this.valueNumMin = numMin;
            this.valueNumMax = numMax;
        }
    }

    /// <summary>
    /// The bump parameter.
    /// </summary>
    /// <param name="m">
    /// The m.
    /// </param>
    /// <param name="p">
    /// The p.
    /// </param>
    private void BumpParameter(MaterialData m, string[] p)
    {
        var regexNumber = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");

        var bumpParams = new Dictionary<string, BumpParamDef>();
        bumpParams.Add("bm", new BumpParamDef("bm", "string", 1, 1));
        bumpParams.Add("clamp", new BumpParamDef("clamp", "string", 1, 1));
        bumpParams.Add("blendu", new BumpParamDef("blendu", "string", 1, 1));
        bumpParams.Add("blendv", new BumpParamDef("blendv", "string", 1, 1));
        bumpParams.Add("imfchan", new BumpParamDef("imfchan", "string", 1, 1));
        bumpParams.Add("mm", new BumpParamDef("mm", "string", 1, 1));
        bumpParams.Add("o", new BumpParamDef("o", "number", 1, 3));
        bumpParams.Add("s", new BumpParamDef("s", "number", 1, 3));
        bumpParams.Add("t", new BumpParamDef("t", "number", 1, 3));
        bumpParams.Add("texres", new BumpParamDef("texres", "string", 1, 1));
        var pos = 1;
        string filename = null;
        while (pos < p.Length)
        {
            if (!p[pos].StartsWith("-"))
            {
                filename = p[pos];
                pos++;
                continue;
            }

            // option processing
            var optionName = p[pos].Substring(1);
            pos++;
            if (!bumpParams.ContainsKey(optionName))
            {
                continue;
            }

            var def = bumpParams[optionName];
            var args = new ArrayList();
            var i = 0;
            var isOptionNotEnough = false;
            for (; i < def.valueNumMin; i++, pos++)
            {
                if (pos >= p.Length)
                {
                    isOptionNotEnough = true;
                    break;
                }

                if (def.valueType == "number")
                {
                    var match = regexNumber.Match(p[pos]);
                    if (!match.Success)
                    {
                        isOptionNotEnough = true;
                        break;
                    }
                }

                args.Add(p[pos]);
            }

            if (isOptionNotEnough)
            {
                Debug.Log("bump variable value not enough for option:" + optionName + " of material:" + m.name);
                continue;
            }

            for (; i < def.valueNumMax && pos < p.Length; i++, pos++)
            {
                if (def.valueType == "number")
                {
                    var match = regexNumber.Match(p[pos]);
                    if (!match.Success)
                    {
                        break;
                    }
                }

                args.Add(p[pos]);
            }

            // TODO: some processing of options
            Debug.Log("found option: " + optionName + " of material: " + m.name + " args: " + string.Concat(args.ToArray()));
        }

        if (filename != null)
        {
            m.bumpTexPath = filename;
        }
    }

    /// <summary>
    /// The gc.
    /// </summary>
    /// <param name="p">
    /// The p.
    /// </param>
    /// <returns>
    /// The <see cref="Color"/>.
    /// </returns>
    private Color gc(string[] p)
    {
        return new Color(cf(p[1]), cf(p[2]), cf(p[3]));
    }

    /*
    /// <summary>
    /// The build.
    /// </summary>
    /// <param name="buffer">
    /// The buffer.
    /// </param>
    private void Build(GeometryBuffer buffer)
    {
        var materials = new Dictionary<string, Material>();

        if (this.hasMaterials)
        {
            foreach (var data in this.materialData)
            {
                if (materials.ContainsKey(data.name))
                {
                    Debug.LogWarning("duplicate material found: " + data.name + ". ignored repeated occurrences");
                    continue;
                }

                materials.Add(data.name, this.GetMaterial(data));
            }
        }
        else
        {
            materials.Add("default", new Material(Shader.Find("VertexLit")));
        }

        var childObjects = new GameObject[buffer.numObjects];

        if (buffer.numObjects == 1)
        {
            gameObject.AddComponent(typeof(MeshFilter));
            gameObject.AddComponent(typeof(MeshRenderer));
            childObjects[0] = gameObject;
        }
        else if (buffer.numObjects > 1)
        {
            for (var i = 0; i < buffer.numObjects; i++)
            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                go.AddComponent(typeof(MeshFilter));
                go.AddComponent(typeof(MeshRenderer));
                childObjects[i] = go;
            }
        }

        buffer.PopulateMeshes(childObjects, materials);
    }*/
}
