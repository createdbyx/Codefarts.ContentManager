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
        ///  Holds a reference to the <see cref="ContentManager{TKey}"/> singleton.
        /// </summary>
        private ContentManager<Uri> manager;

        /// <summary>
        /// Holds the html markup to display.
        /// </summary>
        private string html = string.Empty;

        /// <summary>
        /// Holds the google logo texture.
        /// </summary>
        private Texture2D googleTexture;

        /// <summary>
        /// Holds a reference to a game object that displays the html markup.
        /// </summary>
        private GameObject htmlObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingScript"/> class.
        /// </summary>
        public LoadingScript()
        {
            this.manager = ContentManager<Uri>.Instance;
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
            this.manager.Register(new Texture2DReader());

            // setup html game object
            var find = GameObject.Find("HtmlText");
            this.htmlObject = find == null ? new GameObject("HtmlText") : find;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        public void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 200, 25), "Load html asynchronously"))
            {
                // asynchronously load google home page html markup
                this.manager.Load<HtmlData>(new Uri("http://www.google.com"), this.SetupHtmlObject);
            }

            if (GUI.Button(new Rect(220, 10, 200, 25), "Load html synchronously"))
            {
                // asynchronously load google home page html markup
                this.SetupHtmlObject(this.manager.Load<HtmlData>(new Uri("http://www.google.com")));
            }

            if (GUI.Button(new Rect(10, 45, 200, 25), "Load image asynchronously"))
            {
                // asynchronously load google home page html markup
                this.manager.Load<Texture2D>(
                    new Uri("http://www.google.ca/images/srpr/logo4w.png"),
                    data =>
                    {
                        this.googleTexture = data;
                    });
            }

            if (this.googleTexture != null)
            {
                GUI.DrawTexture(new Rect(10, 45, 512, 512), this.googleTexture, ScaleMode.ScaleToFit);
            }
        }

        /// <summary>
        /// Sets up the html game object.
        /// </summary>
        /// <param name="data">
        /// The html data to setup.
        /// </param>
        private void SetupHtmlObject(HtmlData data)
        {
            this.html = string.IsNullOrEmpty(data.Markup) ? string.Empty : data.Markup.Substring(0, 50);

            var text = this.htmlObject.guiText == null ? this.htmlObject.AddComponent<GUIText>() : this.htmlObject.guiText;
            text.text = this.html;
            text.fontSize = 24;
            text.font = this.guiText.font;
            this.htmlObject.transform.position = new Vector3(0.1f, 0.8f, 0);
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public void Update()
        {
            this.guiText.enabled = this.manager.LoadingQueue > 0;
        }
    }
}
