// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Tests
{
    using System.IO;
    using System.Runtime.InteropServices;

    using Codefarts.ContentManager.Tests.Models;
    using Codefarts.ContentManager.Tests.Writers;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WriterTests
    {
        private ContentManager<string> contentManager;

        private string tempPath;

        [TestInitialize]
        public void StartUp()
        {
            this.tempPath = Path.GetTempPath();
            this.contentManager = new ContentManager<string>(this.tempPath);
            this.contentManager.Register(new TestModelWriter());
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.contentManager.Unload();
            this.contentManager = null;
        }

        [TestMethod]
        public void TestMethod1()
        {
            var data = new TestModel() { FileName = "TestName.txt", FloatValue = 5.25f, IntegerValue = 123 };
            try
            {
                this.contentManager.Save("testData.xml", data);
                var file = Path.Combine(this.contentManager.RootDirectory, "testData.xml");
                Assert.AreEqual(true, File.Exists(file));
            }
            catch (System.Exception ex)
            {
                Assert.Fail("Should not have thrown excepton!", ex);
            }
        }
    }
}
