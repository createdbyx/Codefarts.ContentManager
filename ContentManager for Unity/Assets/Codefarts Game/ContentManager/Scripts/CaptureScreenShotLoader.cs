// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Scripts
{
    using System.IO;

    using Codefarts.ContentManager.Scripts.Code;

    using UnityEngine;

    public class CaptureScreenShotLoader : MonoBehaviour
    {
        private GameObject meshObject;

        private Texture2D screenshotTexture;

        private bool isCapturingScreenShot;

        private string modelPath = @"C:\Users\Dean\Documents\AeroFS\Unity\Codefarts.GridMapping\Assets\Codefarts Game\Grid Mapping\Resources\Codefarts.Unity\Source Meshes\InsideCornerBox2D.obj";

        private bool isCapturingScreenShotAsync;

        private float percentage;

        public Material material;

        public Material previewMaterial;

        public void OnPostRender()
        {
            if (this.isCapturingScreenShot)
            {
                this.isCapturingScreenShot = false;
                var args = new MeshPreviewTextureArgs()
                               {
                                   Key = this.modelPath,
                                   Width = 256,
                                   Height = 256,
                                   CacheFolder = Application.temporaryCachePath,
                                   SaveToCache = false,
                                   LoadFromCache = false,
                                   BackgroundColor = Color.clear,
                                   Material = this.previewMaterial
                               };

                if (this.isCapturingScreenShotAsync)
                {
                    ContentManager<MeshPreviewTextureArgs>.Instance.Load<MeshPreviewTexture>(args, this.ScreenShotProgress);
                }
                else
                {
                    var mesh = ContentManager<MeshPreviewTextureArgs>.Instance.Load<MeshPreviewTexture>(args);
                    this.screenshotTexture = mesh.Texture;
                    this.isCapturingScreenShot = false;
                }
            }
        }

        private void ScreenShotProgress(ReadAsyncArgs<MeshPreviewTextureArgs, MeshPreviewTexture> args)
        {
            if (args.State == ReadState.Completed)
            {
                this.screenshotTexture = args.Result.Texture;
                this.isCapturingScreenShot = false;
                return;
            }

            this.percentage = args.Progress;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        public void OnGUI()
        {
            this.modelPath = GUI.TextField(new Rect(10, 10, Screen.width - 20, 20), this.modelPath);

            if (GUI.Button(new Rect(10, 40, 128, 20), "Take Screen Shot"))
            {
                this.isCapturingScreenShot = true;
                this.isCapturingScreenShotAsync = false;
            }

            if (GUI.Button(new Rect(140, 40, 200, 20), "Take Screen Shot Async"))
            {
                this.isCapturingScreenShot = true;
                this.isCapturingScreenShotAsync = true;
            }

            if (GUI.Button(new Rect(10, 70, 128, 20), "Load"))
            {
                var filter = this.meshObject.GetComponent<MeshFilter>();
                filter.sharedMesh = ContentManager<string>.Instance.Load<Mesh>(this.modelPath);
            }

            if (this.isCapturingScreenShot)
            {
                GUI.Label(new Rect(10, 50, 256, 30), string.Format("Loading: {0}%", this.percentage));
            }

            if (this.screenshotTexture != null)
            {
                GUI.DrawTexture(new Rect(16, 100, this.screenshotTexture.width, this.screenshotTexture.height), this.screenshotTexture);
            }
        }

        /// <summary>
        /// Start is called just before any of the Update methods is called the first time.
        /// </summary>
        public void Start()
        {
            // register readers
            ContentManager<MeshPreviewTextureArgs>.Instance.Register(new ObjMeshPreviewTextureStringReader());
            ContentManager<string>.Instance.Register(new ObjMeshStringReader());

            // setup html game object
            var find = GameObject.Find("Mesh");
            this.meshObject = find == null ? new GameObject("Mesh") : find;
            this.meshObject.transform.position = Vector3.zero;
            var renderer = this.meshObject.AddComponent<MeshRenderer>();
            renderer.material = this.material;
            this.meshObject.AddComponent<MeshFilter>();
        }
    }
}