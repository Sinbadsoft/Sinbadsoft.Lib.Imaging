// <copyright file="ImageResizerTest.cs" company="Sinbadsoft">
// Copyright (c) Chaker Nakhli 2010
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by 
// applicable law or agreed to in writing, software distributed under the License
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// </copyright>
// <author>Chaker Nakhli</author>
// <email>chaker.nakhli@sinbadsoft.com</email>
// <date>2010/11/04</date>
using System;
using System.Drawing;
using System.IO;

using NUnit.Framework;

namespace Sinbadsoft.Lib.Imaging.Tests
{
    [TestFixture]
    public class ImageResizerTest
    {
        private const string TestdataDirectory = "TestData";

        [Test]
        public void Thumbails()
        {
            const uint TargetMaxSize = 250;
            var resizer = new ImageResizer(50, 0.50f);
            var sourceFiles = Directory.GetFiles(TestdataDirectory);
            Assert.Greater(sourceFiles.Length, 0);
            foreach (var sourceFile in sourceFiles)
            {
                Console.WriteLine(sourceFile);
                var sourceFileInfo = new FileInfo(sourceFile);
                var targetFileInfo = new FileInfo(Path.GetFileNameWithoutExtension(sourceFile) + "_s.jpg");
                Assert.DoesNotThrow(
                    () => resizer.Resize(sourceFileInfo.FullName, targetFileInfo.FullName, TargetMaxSize));
                Assert.IsTrue(targetFileInfo.Exists);
                Console.WriteLine("Compression Ratio : {0}", targetFileInfo.Length * 100.0 / sourceFileInfo.Length);
                Assert.Greater(targetFileInfo.Length, 0);
                Assert.Less(targetFileInfo.Length, sourceFileInfo.Length);

                using (var sourceImage = Image.FromFile(sourceFile))
                using (var targetImage = Image.FromFile(targetFileInfo.FullName))
                {
                    if (targetImage.Size.Height > targetImage.Size.Width)
                    {
                        Assert.AreEqual(targetImage.Size.Height, TargetMaxSize);
                    }
                    else
                    {
                        Assert.AreEqual(targetImage.Size.Width, TargetMaxSize);
                    }

                    // Aspect ratio
                    Assert.That(
                        targetImage.Size.Height / (double)targetImage.Size.Width, 
                        Is.EqualTo(sourceImage.Size.Height / (double)sourceImage.Size.Width).Within(0.009));
                }
            }
        }

        [Test]
        public void SmallImagesAreNotScaledDown()
        {
            foreach (uint offset in new[] { 0, 10, 1000 })
            {
                foreach (var sourceFile in Directory.GetFiles(TestdataDirectory, "*.jpg"))
                {
                    var targetFile = Path.GetFileNameWithoutExtension(sourceFile) + "_s.jpg";
                    using (var sourceImage = Image.FromFile(sourceFile))
                    {
                        var targetMaxSize = (uint)Math.Max(sourceImage.Size.Height, sourceImage.Size.Width) + offset;
                        var resizer = new ImageResizer(75, 0.80f);
                        resizer.Resize(sourceFile, targetFile, targetMaxSize);
                        using (var targetImage = Image.FromFile(targetFile))
                        {
                            Assert.AreEqual(targetImage.Size, sourceImage.Size);
                        }
                    }
                }
            }
        }
    }
}
