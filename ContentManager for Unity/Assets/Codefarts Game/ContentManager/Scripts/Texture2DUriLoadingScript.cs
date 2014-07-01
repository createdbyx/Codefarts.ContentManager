// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Scripts
{
    using System;

    using UnityEngine;

    /// <summary>
    /// Provides a loading script that 
    /// </summary>
    public class Texture2DUriLoadingScript : MonoBehaviour
    {
        /// <summary>
        /// Holds the google logo texture.
        /// </summary>
        private Texture2D texture;

        /// <summary>
        /// Holds the html markup to display.
        /// </summary>
        private string html = string.Empty;

        /// <summary>
        /// Holds a reference to a game object that displays the html markup.
        /// </summary>
        private GameObject htmlObject;

        /// <summary>
        ///  Holds a reference to the <see cref="ContentManager{TKey}"/> singleton.
        /// </summary>
        private ContentManager<Uri> manager;

        /// <summary>
        /// Holds the url string containing the url to the texture that will be loaded.
        /// </summary>
        private string urlString = "http://www.codefarts.com/Content/Images/Screenshots/GridMappingForUnity/Screenshot1.png";

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DUriLoadingScript"/> class.
        /// </summary>
        public Texture2DUriLoadingScript()
        {
            this.manager = ContentManager<Uri>.Instance;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        public void OnGUI()
        {
            var key = new Uri("http://www.google.com");
            this.urlString = GUI.TextField(new Rect(10, 10, Screen.width - 10, 20), this.urlString);

            var uri = new Uri(this.urlString);
            if (GUI.Button(new Rect(10, 40, 200, 25), "Load asynchronously"))
            {
                // asynchronously load image
                try
                {
                    this.manager.Load<Texture2D>(uri, this.SetupTextureHtmlObject, false);
                }
                catch (Exception ex)
                {
                    this.SetupTextureHtmlObject(new ReadAsyncArgs<Uri, Texture2D>() { Key = key, Progress = 100f, State = ReadState.Completed, Error = ex });
                }
            }

            if (GUI.Button(new Rect(220, 40, 200, 25), "Load synchronously"))
            {
                // synchronously load  
                try
                {
                    var result = this.manager.Load<Texture2D>(uri, false);
                    this.SetupTextureHtmlObject(new ReadAsyncArgs<Uri, Texture2D>() { Key = key, Progress = 100f, State = ReadState.Completed, Result = result });
                }
                catch (Exception ex)
                {
                    this.SetupTextureHtmlObject(new ReadAsyncArgs<Uri, Texture2D>() { Key = key, Progress = 100f, State = ReadState.Completed, Error = ex });
                }
            }

            if (GUI.Button(new Rect(430, 40, 200, 25), "Reset"))
            {
                this.DoReset();
            }

            if (this.texture != null)
            {
                GUI.DrawTexture(new Rect(10, 70, Screen.width - 20, Screen.height - 80), this.texture, ScaleMode.ScaleToFit);
            }
        }

        private void DoReset()
        {
            if (this.texture != null)
            {
                DestroyImmediate(this.texture);
            }

            this.html = string.Empty;
            this.htmlObject.guiText.text = string.Empty;
        }

        /// <summary>
        /// Start is called just before any of the Update methods is called the first time.
        /// </summary>
        public void Start()
        {
            // get singleton instance to the codefarts content manager using a Uri as the key type
            this.manager = ContentManager<Uri>.Instance;

            // register readers
            this.manager.Register(new Texture2DUriReader());

            // setup html game object
            this.htmlObject = new GameObject("HtmlText");
            this.htmlObject.AddComponent<GUIText>();
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public void Update()
        {
            this.guiText.enabled = this.manager.LoadingQueue > 0;
        }

        /// <summary>
        /// Sets up the html game object.
        /// </summary>
        /// <param name="data">
        /// The html data to setup.
        /// </param>
        private void SetupTextureHtmlObject(ReadAsyncArgs<Uri, Texture2D> data)
        {
            this.html = data.Error != null ? data.Error.Message : string.Format("Loading: {0}%  {1}", data.Progress, data.State);
            var text = this.htmlObject.guiText;
            text.text = this.html;
            text.fontSize = 24;
            text.font = this.guiText.font;
            this.htmlObject.transform.position = new Vector3(0.1f, 0.8f, 0);
            if (data.State == ReadState.Completed)
            {
                this.texture = data.Result;
            }
        }
    }
}