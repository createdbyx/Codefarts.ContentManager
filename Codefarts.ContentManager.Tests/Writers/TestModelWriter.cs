// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Tests.Writers
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    using Codefarts.ContentManager.Tests.Models;

    public class TestModelWriter : IWriter<string>
    {
        public Type Type
        {
            get
            {
                return typeof(TestModel);
            }
        }

        public void Write(string key, object data, ContentManager<string> content)
        {
            if (this.Type != data.GetType())
            {
                throw new InvalidCastException("data is not of the right type.");
            }

            var model = data as TestModel;
            var serializer = new XmlSerializer(this.Type);
            var file = !Path.IsPathRooted(key) ? Path.Combine(content.RootDirectory, key) : key;
            using (var writer = XmlWriter.Create(file))
            {
                serializer.Serialize(writer, model);
            }
        }

        public bool CanWrite(string key, ContentManager<string> content)
        {
            var file = !Path.IsPathRooted(key) ? Path.Combine(content.RootDirectory, key) : key;
            var info = new FileInfo(file);
            return true;
        }

        public void WriteAsync(string key, object data, ContentManager<string> content, Action completedCallback)
        {
            throw new NotImplementedException();
        }
    }
}
