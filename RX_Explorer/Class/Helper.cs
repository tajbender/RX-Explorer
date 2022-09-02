﻿using RX_Explorer.View;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace RX_Explorer.Class
{
    public static class Helper
    {
        public static bool GetSuitableInnerViewerPageType(FileSystemStorageFile File, out Type PageType)
        {
            switch (File.Type.ToLower())
            {
                case ".jpg" or ".jpeg" or ".png" or ".bmp":
                    {
                        PageType = typeof(PhotoViewer);
                        return true;
                    }
                case ".mkv" or ".mp4" or ".mp3" or ".flac" or ".wma" or ".wmv" or ".m4a" or ".mov" or ".alac":
                    {
                        PageType = typeof(MediaPlayer);
                        return true;
                    }
                case ".txt":
                    {
                        PageType = typeof(TextViewer);
                        return true;
                    }
                case ".pdf":
                    {
                        PageType = typeof(PdfReader);
                        return true;
                    }
                case ".zip":
                    {
                        PageType = typeof(CompressionViewer);
                        return true;
                    }
                default:
                    {
                        PageType = null;
                        return false;
                    }
            }
        }

        public static async Task<byte[]> GetByteArrayFromRandomAccessStreamAsync(IRandomAccessStream Stream)
        {
            using (MemoryStream TempStream = new MemoryStream())
            {
                await Stream.AsStreamForRead().CopyToAsync(TempStream);
                return TempStream.ToArray();
            }
        }

        public static async Task<InMemoryRandomAccessStream> CreateRandomAccessStreamAsync(byte[] Data)
        {
            InMemoryRandomAccessStream Stream = new InMemoryRandomAccessStream();
            await Stream.WriteAsync(Data.AsBuffer());
            Stream.Seek(0);
            return Stream;
        }

        public static async Task<BitmapImage> CreateBitmapImageAsync(byte[] Data)
        {
            using (InMemoryRandomAccessStream Stream = await CreateRandomAccessStreamAsync(Data))
            {
                return await CreateBitmapImageAsync(Stream);
            }
        }

        public static async Task<BitmapImage> CreateBitmapImageAsync(IRandomAccessStream Stream)
        {
            Stream.Seek(0);
            BitmapImage Bitmap = new BitmapImage();
            await Bitmap.SetSourceAsync(Stream);
            return Bitmap;
        }

        public static async Task<IRandomAccessStream> GetThumbnailFromStreamAsync(IRandomAccessStream InputStream, uint RequestedSize)
        {
            if (InputStream == null)
            {
                return null;
            }

            BitmapDecoder Decoder = await BitmapDecoder.CreateAsync(InputStream);

            if (Decoder.PixelWidth < RequestedSize || Decoder.PixelHeight < RequestedSize)
            {
                return InputStream;
            }

            InMemoryRandomAccessStream OutputStream = new InMemoryRandomAccessStream();

            try
            {
                BitmapEncoder Encoder = await BitmapEncoder.CreateForTranscodingAsync(OutputStream, Decoder);

                Encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

                if (Decoder.PixelWidth > Decoder.PixelHeight)
                {
                    Encoder.BitmapTransform.ScaledHeight = RequestedSize;
                    Encoder.BitmapTransform.ScaledWidth = Convert.ToUInt32((double)RequestedSize / Decoder.PixelHeight * Decoder.PixelWidth);
                }
                else
                {
                    Encoder.BitmapTransform.ScaledWidth = RequestedSize;
                    Encoder.BitmapTransform.ScaledHeight = Convert.ToUInt32((double)RequestedSize / Decoder.PixelWidth * Decoder.PixelHeight);
                }

                await Encoder.FlushAsync();
            }
            catch (Exception)
            {
                OutputStream.Dispose();
                throw new NotSupportedException("Could not generate the thumbnail for the image");
            }

            return OutputStream;
        }
    }
}
