using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ClinicDent2
{
    public static class ImageCompressor
    {
        //public static byte[] CompressImage(byte[] originalBytes)
        //{
        //    using (System.Drawing.Image imageToCompress = System.Drawing.Image.FromStream(new MemoryStream(originalBytes)))
        //    {
        //        using (System.Drawing.Image memImage = new System.Drawing.Bitmap(imageToCompress,200,150))
        //        {
        //            int newQuality = 100;
        //            ImageCodecInfo myImageCodecInfo;
        //            System.Drawing.Imaging.Encoder myEncoder;
        //            EncoderParameter myEncoderParameter;
        //            EncoderParameters myEncoderParameters;
        //            myImageCodecInfo = GetEncoderInfo("image/jpeg");
        //            myEncoder = System.Drawing.Imaging.Encoder.Quality;
        //            myEncoderParameters = new EncoderParameters(1);
        //            myEncoderParameter = new EncoderParameter(myEncoder, newQuality);
        //            myEncoderParameters.Param[0] = myEncoderParameter;

        //            MemoryStream memStream = new MemoryStream();
        //            memImage.Save(memStream, myImageCodecInfo, myEncoderParameters);
        //            System.Drawing.Image newImage = System.Drawing.Image.FromStream(memStream);
        //            ImageAttributes imageAttributes = new ImageAttributes();
        //            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newImage))
        //            {
        //                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;  //**
        //                g.DrawImage(newImage, new System.Drawing.Rectangle(System.Drawing.Point.Empty, newImage.Size), 0, 0, newImage.Width, newImage.Height, System.Drawing.GraphicsUnit.Pixel, imageAttributes);
        //            }
        //            return ImageToByteArray(newImage);

        //        }
        //    }
        //}
        public static byte[] CompressImage(byte[] originalBytes, Size size)
        {
            using (Image imageToCompress = Image.FromStream(new MemoryStream(originalBytes)))
            {
                int sourceWidth = imageToCompress.Width; 
                int sourceHeight = imageToCompress.Height;
                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;
                nPercentW = ((float)size.Width / (float)sourceWidth);
                nPercentH = ((float)size.Height / (float)sourceHeight);
                if (nPercentH < nPercentW)
                    nPercent = nPercentH;
                else
                    nPercent = nPercentW;
                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);
                Bitmap b = new Bitmap(destWidth, destHeight);
                Graphics g = Graphics.FromImage((System.Drawing.Image)b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imageToCompress, 0, 0, destWidth, destHeight);
                g.Dispose();
                return ImageToByte2(b);
            }
        }
        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
        static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in encoders)
                if (ici.MimeType == mimeType) return ici;

            return null;
        }
        static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
        
    }
}
