/*
<copyright>
  Copyright (c) 2012 Codefarts
  All rights reserved.
  contact@codefarts.com
  http://www.codefarts.com
</copyright>
*/
namespace Codefarts.ContentManager.Scripts
{
    using System;

    using UnityEngine;

    /// <summary>
    /// Provides a loading script that 
    /// </summary>
    public class LoadingScript : MonoBehaviour
    {
        /// <summary>
        /// Holds the google logo texture.
        /// </summary>
        private Texture2D googleTexture;

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
        /// Initializes a new instance of the <see cref="LoadingScript"/> class.
        /// </summary>
        public LoadingScript()
        {
            this.manager = ContentManager<Uri>.Instance;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        public void OnGUI()
        {
            var key = new Uri("http://www.google.com");
            if (GUI.Button(new Rect(10, 10, 200, 25), "Load html asynchronously"))
            {
                // asynchronously load google home page html markup
                this.manager.Load<HtmlData>(key, this.SetupHtmlObject, false);
            }

            if (GUI.Button(new Rect(220, 10, 200, 25), "Load html synchronously"))
            {
                // asynchronously load google home page html markup
                this.SetupHtmlObject(new ReadAsyncArgs<Uri, HtmlData>() { Key = key, Progress = 100f, State = ReadState.Completed, Result = this.manager.Load<HtmlData>(key, false) });
            }

            if (GUI.Button(new Rect(220, 45, 200, 25), "Reset"))
            {
                if (this.googleTexture != null)
                {
                    DestroyImmediate(this.googleTexture);
                }

                this.html = string.Empty;
                this.htmlObject.guiText.text = string.Empty;
            }

            if (GUI.Button(new Rect(10, 45, 200, 25), "Load image asynchronously"))
            {
                // asynchronously load google home page html markup
                this.manager.Load<Texture2D>(
                    new Uri("http://www.codefarts.com/Content/Images/Screenshots/GridMappingForUnity/Screenshot1.png"),
                     data => this.SetupTextureHtmlObject(data),
                     false);
            }

            if (this.googleTexture != null)
            {
                GUI.DrawTexture(new Rect(10, 45, 512, 512), this.googleTexture, ScaleMode.ScaleToFit);
            }
        }

        /// <summary>
        /// Start is called just before any of the Update methods is called the first time.
        /// </summary>
        public void Start()
        {
            // get singleton instance to the codefarts content manager using a Uri as the key type
            this.manager = ContentManager<Uri>.Instance;

            // register readers
            this.manager.Register(new HtmlReader());
            this.manager.Register(new Texture2DUriReader());

            // setup html game object
            var find = GameObject.Find("HtmlText");
            this.htmlObject = find == null ? new GameObject("HtmlText") : find;
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
        private void SetupHtmlObject(ReadAsyncArgs<Uri, HtmlData> data)
        {
            if (data.State != ReadState.Completed)
            {
                this.html = string.Format("Loading: {0}%", data.Progress);
            }
            else
            {
                this.html = string.IsNullOrEmpty(data.Result.Markup) ? string.Empty : data.Result.Markup.Substring(0, 50);
            }

            var text = this.htmlObject.guiText == null ? this.htmlObject.AddComponent<GUIText>() : this.htmlObject.guiText;
            text.text = this.html;
            text.fontSize = 24;
            text.font = this.guiText.font;
            this.htmlObject.transform.position = new Vector3(0.1f, 0.8f, 0);
        }

        /// <summary>
        /// Sets up the html game object.
        /// </summary>
        /// <param name="data">
        /// The html data to setup.
        /// </param>
        private void SetupTextureHtmlObject(ReadAsyncArgs<Uri, Texture2D> data)
        {
            this.html = string.Format("Loading: {0}%  {1}", data.Progress, data.State);
            var text = this.htmlObject.guiText == null ? this.htmlObject.AddComponent<GUIText>() : this.htmlObject.guiText;
            text.text = this.html;
            text.fontSize = 24;
            text.font = this.guiText.font;
            this.htmlObject.transform.position = new Vector3(0.1f, 0.8f, 0);
            if (data.State == ReadState.Completed)
            {
                this.googleTexture = data.Result;
            }
        }
    }
}
