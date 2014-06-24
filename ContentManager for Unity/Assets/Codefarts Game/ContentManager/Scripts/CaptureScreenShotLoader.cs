// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Scripts
{
    using Codefarts.ContentManager.Scripts.Code;

    using UnityEngine;

    public class CaptureScreenShotLoader : MonoBehaviour
    {
        /// <summary>
        ///  Holds a reference to the <see cref="ContentManager{TKey}"/> singleton.
        /// </summary>
        private ContentManager<string> manager;

        private GameObject meshObject;

        private Texture2D screenshotTexture;

        private bool isCapturingScreenShot;

        private string objectFilePath = string.Empty;

        public void OnPostRender()
        {
            if (this.isCapturingScreenShot)
            {
                this.isCapturingScreenShot = false;
                var mesh = this.manager.Load<MeshPreviewTexture>(this.objectFilePath);
                this.screenshotTexture = mesh.Texture;
            }
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        public void OnGUI()
        {
            this.objectFilePath = GUI.TextField(new Rect(10, 10, 256, 20), this.objectFilePath);

            if (GUI.Button(new Rect(10, 40, 128, 20), "Take Screen Shot"))
            {
                this.isCapturingScreenShot = true;
            }

            if (GUI.Button(new Rect(10, 70, 128, 20), "Load"))
            {
                var filter = this.meshObject.GetComponent<MeshFilter>();
                filter.sharedMesh = this.manager.Load<Mesh>(this.objectFilePath);
            }

            if (this.screenshotTexture != null)
            {
                GUI.DrawTexture(new Rect(16, 64, this.screenshotTexture.width, this.screenshotTexture.height), this.screenshotTexture);
            }
        }

        /// <summary>
        /// Start is called just before any of the Update methods is called the first time.
        /// </summary>
        public void Start()
        {
            // get singleton instance to the codefarts content manager using a Uri as the key type
            this.manager = ContentManager<string>.Instance;

            // register readers
            this.manager.Register(new ObjMeshPreviewTextureStringReader());
            this.manager.Register(new ObjMeshStringReader());

            // setup html game object
            var find = GameObject.Find("Mesh");
            this.meshObject = find == null ? new GameObject("Mesh") : find;
            this.meshObject.transform.position = Vector3.zero;
            var renderer = this.meshObject.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Diffuse"));
            this.meshObject.AddComponent<MeshFilter>();
        }
    }
}