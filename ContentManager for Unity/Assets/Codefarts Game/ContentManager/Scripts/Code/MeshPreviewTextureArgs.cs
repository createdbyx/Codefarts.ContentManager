// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Scripts
{
    using UnityEngine;

    public class MeshPreviewTextureArgs
    {
        public Material Material { get; set; }

        public string Key { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string CacheFolder { get; set; }

        public bool SaveToCache { get; set; }
        public bool LoadFromCache { get; set; }

        public Color BackgroundColor { get; set; }

        public Vector3 Location { get; set; }

        public MeshPreviewTextureArgs()
        {
            this.LoadFromCache = true;
        }
    }
}