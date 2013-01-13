// <copyright file="ImageResizer.cs" company="Sinbadsoft">
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
using System.IO;
using Microsoft.Test.Tools.WicCop.InteropServices.ComTypes;
using Sinbadsoft.Lib.Imaging.InteropServices;

namespace Sinbadsoft.Lib.Imaging
{
    public class ImageResizer
    {
        private const string ImageQualityParamName = "ImageQuality";
        private readonly object[] jpegQuality = new object[] { 0.9f };
        private readonly int thumbDpi;

        public ImageResizer(int targetDpi, float jpgQuality)
        {
            this.thumbDpi = targetDpi;
            this.jpegQuality[0] = jpgQuality;
        }

        public void Resize(string sourceFile, string targetFile, uint targetMaxSize)
        {
            var imageBytes = File.ReadAllBytes(sourceFile);
            this.Resize(imageBytes, targetFile, targetMaxSize);
        }

        public void Resize(byte[] sourceBytes, string targetFile, uint targetMaxSize)
        {
            using (var thumbnailStream = File.OpenWrite(targetFile))
            {
                this.Resize(sourceBytes, thumbnailStream, targetMaxSize);
            }            
        }

        public void Resize(byte[] sourceBytes, Stream targetStream, uint targetMaxSize)
        {
            var factory = (IWICComponentFactory)new WICImagingFactory();
            var inputStream = factory.CreateStream();
            inputStream.InitializeFromMemory(sourceBytes, (uint)sourceBytes.Length);
            var decoder = factory.CreateDecoderFromStream(inputStream, null, WICDecodeOptions.WICDecodeMetadataCacheOnLoad);
            var frame = decoder.GetFrame(0);

            // Compute target size
            uint width, height, targetWidth, targetHeight;
            frame.GetSize(out width, out height);
            if (width <= targetMaxSize && height <= targetMaxSize)
            {
                targetHeight = height;
                targetWidth = width;
            }
            else if (width > height)
            {
                targetWidth = targetMaxSize;
                targetHeight = height * targetMaxSize / width;
            }
            else
            {
                targetWidth = width * targetMaxSize / height;
                targetHeight = targetMaxSize;
            }

            // Prepare output stream to cache file
            var outputStreamAdapter = new StreamAdapter(targetStream);
            
            // Prepare JPG encoder
            var encoder = factory.CreateEncoder(Consts.GUID_ContainerFormatJpeg, null);
            encoder.Initialize(outputStreamAdapter, WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache);

            // Prepare output frame
            IWICBitmapFrameEncode outputFrame;
            var argument = new IPropertyBag2[1];
            encoder.CreateNewFrame(out outputFrame, argument);
            var propBag = argument[0];
            var propertyBagOption = new PROPBAG2[1];
            propertyBagOption[0].pstrName = ImageQualityParamName;
            propBag.Write(1, propertyBagOption, this.jpegQuality);
            outputFrame.Initialize(propBag);
            outputFrame.SetResolution(this.thumbDpi, this.thumbDpi);
            outputFrame.SetSize(targetWidth, targetHeight);

            // Prepare scaler
            var scaler = factory.CreateBitmapScaler();
            scaler.Initialize(frame, targetWidth, targetHeight, WICBitmapInterpolationMode.WICBitmapInterpolationModeLinear);

            // Write the scaled source to the output frame
            outputFrame.WriteSource(scaler, new WICRect { X = 0, Y = 0, Width = (int)targetWidth, Height = (int)targetHeight });
            outputFrame.Commit();
            encoder.Commit();
        }
    }
}
