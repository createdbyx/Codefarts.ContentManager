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
    public class HtmlDataUriLoadingScript : MonoBehaviour
    {
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
        private string urlString = "http://www.codefarts.com/page/GridMappingDocumentation";

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlDataUriLoadingScript"/> class.
        /// </summary>
        public HtmlDataUriLoadingScript()
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
                // asynchronously load 
                try
                {
                    this.manager.Load<HtmlData>(uri, this.SetupTextureHtmlObject, false);
                }
                catch (Exception ex)
                {
                    this.SetupTextureHtmlObject(new ReadAsyncArgs<Uri, HtmlData>() { Key = key, Progress = 100f, State = ReadState.Completed, Error = ex });
                }
            }

            if (GUI.Button(new Rect(220, 40, 200, 25), "Load synchronously"))
            {
                // synchronously load  
                try
                {
                    var result = this.manager.Load<HtmlData>(uri, false);
                    this.SetupTextureHtmlObject(new ReadAsyncArgs<Uri, HtmlData>() { Key = key, Progress = 100f, State = ReadState.Completed, Result = result });
                }
                catch (Exception ex)
                {
                    this.SetupTextureHtmlObject(new ReadAsyncArgs<Uri, HtmlData>() { Key = key, Progress = 100f, State = ReadState.Completed, Error = ex });
                }
            }

            if (GUI.Button(new Rect(430, 40, 200, 25), "Reset"))
            {
                this.DoReset();
            }

            // restricted to 10k length because unity throws exceptions in console about exceeding UInt16 vertex bounds etc
            GUI.TextArea(new Rect(10, 100, Screen.width - 20, Screen.height - 110), this.html, 10000); 
        }

        private void DoReset()
        {
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
            this.manager.Register(new HtmlReader());                                                            

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
        private void SetupTextureHtmlObject(ReadAsyncArgs<Uri, HtmlData> data)
        {
            this.html = data.Error != null ? data.Error.Message : string.Format("Loading: {0}%  {1}", data.Progress, data.State);
            var text = this.htmlObject.guiText;
            text.text = this.html;
            text.fontSize = 24;
            text.font = this.guiText.font;
            this.htmlObject.transform.position = new Vector3(0.1f, 0, 0);
            text.pixelOffset = new Vector2(0, Screen.height - 70);
            if (data.State == ReadState.Completed)
            {
                this.html = data.Result.Markup;
            }
        }
    }
}