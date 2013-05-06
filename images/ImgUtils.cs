using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NBrightCore.common;

namespace NBrightCore.images
{
    public class ImgUtils
    {
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        // Bitmap bytes have to be created via a direct memory copy of the bitmap
        private static byte[] BmpToBytesMemStream(Bitmap bmp)
        {
            // convert to jpeg
            return BmpToBytesMemStream(bmp, ImageFormat.Jpeg);
        }

        private static byte[] BmpToBytesMemStream(Bitmap bmp, ImageFormat imgFormat)
        {
            var ms = new MemoryStream();

            // Save to memory using the Jpeg format
            //var info = ImageCodecInfo.GetImageEncoders();
            var jgpEncoder = GetEncoder(imgFormat);

            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 85L);

            bmp.Save(ms, jgpEncoder, encoderParameters);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }

        //Bitmap bytes have to be created using Image.Save()
        public static Image BytesToImg(byte[] bmpBytes)
        {
            var ms = new MemoryStream(bmpBytes);
            var img = Image.FromStream(ms);
            // Do NOT close the stream!

            return img;
        }

        public static bool IsImageFile(FileInfo sFileInfo)
        {
            return IsImageFile(sFileInfo.Extension);
        }

        public static bool IsImageFile(string strExtension)
        {
            return strExtension.ToLower() == ".jpg" | strExtension.ToLower() == ".jpeg" | strExtension.ToLower() == ".gif" | strExtension.ToLower() == ".png" | strExtension.ToLower() == ".tiff" | strExtension.ToLower() == ".bmp";
        }

        public static void AddWatermark(string imageFilePath, string waterMarkImagePath)
        {
            //add watermark if needed
            if (!string.IsNullOrEmpty(waterMarkImagePath))
            {
                var output = new ImgWaterMark(imageFilePath, waterMarkImagePath, true);
                output.AddWaterMark();
                Utils.SaveFile(imageFilePath, BmpToBytesMemStream(output.Image));
            }
        }

        public static Bitmap CreateCanvas(int width, int height)
        {
            return CreateCanvas(width, height, "White");
        }

        public static Bitmap CreateCanvas(int width, int height, string backGroundColor)
        {
            var b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(b);
            var colorBrush = new SolidBrush(Color.FromName(backGroundColor));
            g.FillRectangle(colorBrush, 0, 0, width, height);
            g.Dispose();
            colorBrush.Dispose();
            return b;
        }

        public static void AddToCanvas(string imageFilePath, string canvasImagePath)
        {
            //add watermark if needed
            if (!string.IsNullOrEmpty(canvasImagePath))
            {
                var output = new ImgWaterMark(canvasImagePath, imageFilePath, false);
                output.AddWaterMark();
                Utils.SaveFile(imageFilePath, BmpToBytesMemStream(output.Image));
            }
        }

        public static string ResizeImageToJpg(string fileNameIn, string fileNameOut, double imgSize)
        {
            try
            {
                return ResizeImage(fileNameIn, Utils.ReplaceFileExt(fileNameOut, ".jpg"), imgSize);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ResizeImageToPng(string fileNameIn, string fileNameOut, double imgSize)
        {
            try
            {
                return ResizeImage(fileNameIn, Utils.ReplaceFileExt(fileNameOut, ".png"), imgSize);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ResizeImage(string fileNamePath, string fileNamePathOut, double intMaxWidth)
        {

            try
            {
                // Get source Image
                using (var sourceImage = new Bitmap(fileNamePath))
                {
                    var thumbW = 0;
                    var thumbH = 0;
                    if (sourceImage.Height > sourceImage.Width)
                    {
                        thumbH = Convert.ToInt32(intMaxWidth);
                        thumbW = 0;
                    }
                    else
                    {
                        thumbW = Convert.ToInt32(intMaxWidth);
                        thumbH = 0;
                    }

                    var fName1 = Path.GetFileName(fileNamePathOut);
                    if (fName1 != null)
                    {
                        var fName2 = fName1.Replace(" ", "_");
                        fileNamePathOut = fileNamePathOut.Replace(fName1, fName2);

                        using (var newImage = CreateThumbnail(fileNamePath, Convert.ToInt32(thumbW), Convert.ToInt32(thumbH)))
                        {

                            if ((newImage != null))
                            {
                                ImageCodecInfo useEncoder;
                                var extension = Path.GetExtension(fileNamePathOut);
                                if (extension != null && extension.ToLower() == ".png")
                                {
                                    useEncoder = GetEncoder(ImageFormat.Png);
                                }
                                else
                                {
                                    useEncoder = GetEncoder(ImageFormat.Jpeg);
                                }

                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 85L);

                                try
                                {
                                    newImage.Save(fileNamePathOut, useEncoder, encoderParameters);
                                }
                                catch (Exception)
                                {
                                    GC.Collect();
                                    // attempt to clear all file locks and try again
                                    try
                                    {
                                        newImage.Save(fileNamePathOut, useEncoder, encoderParameters);
                                    }
                                    // ReSharper disable EmptyGeneralCatchClause
                                    catch
                                    // ReSharper restore EmptyGeneralCatchClause
                                    {
                                        //abandon save. 
                                        //Assumption is the thumb already is there, but locked. So no need for error.
                                    }
                                }

                                // Clean up
                                newImage.Dispose();
                            }
                        }
                    }

                    // Clean up
                    sourceImage.Dispose();
                }

                return fileNamePathOut;
            }
// ReSharper disable UnusedVariable
            catch (Exception e)
// ReSharper restore UnusedVariable
            {
                return "";
            }

        }

        public static int GetThumbWidth(string thumbSize)
        {
            if (!Utils.IsNumeric(thumbSize) & !string.IsNullOrEmpty(thumbSize))
            {
                var thumbSplit = thumbSize.Split('x');
                return Convert.ToInt32(thumbSplit[0]);
            }
            if (Utils.IsNumeric(thumbSize))
            {
                return Convert.ToInt32(thumbSize);                
            }
            return 0;
        }

        public static int GetThumbHeight(string thumbSize)
        {
            var thumbH = -1;
            if (!Utils.IsNumeric(thumbSize) & !string.IsNullOrEmpty(thumbSize))
            {
                string[] thumbSplit = thumbSize.Split('x');
                if (thumbSplit.GetUpperBound(0) >= 1)
                {
                    thumbH = Convert.ToInt32(thumbSplit[1]);
                }
            }
            return Convert.ToInt32(thumbH);
        }

        public static string GetThumbFileName(string imgPathName, int thumbW)
        {
            return GetThumbFileName(imgPathName,thumbW,0);
        }

        public static string GetThumbFileName(string imgPathName, int thumbW, int thumbH)
        {
            if (thumbH < 0)
            {
                return string.Format("Thumb_{0}{1}", thumbW, Path.GetFileName(imgPathName));                
            }
            return string.Format("Thumb_{0}x{1}{2}", thumbW, thumbH, Path.GetFileName(imgPathName));
        }

        public static string GetThumbFilePathName(string imgPathName, int thumbW)
        {
            return GetThumbFilePathName(imgPathName, thumbW, 0);
        }

        public static string GetThumbFilePathName(string imgPathName, int thumbW, int thumbH)
        {
            var fileName = Path.GetFileName(imgPathName);
            if (!string.IsNullOrEmpty(fileName))
            {
                return imgPathName.Replace(fileName, GetThumbFileName(imgPathName, thumbW, thumbH));
            }
            return imgPathName;
        }
        
        public static void CreateThumbOnDisk(string imgPathName, string thumbSizeCsv)
        {

            if (!string.IsNullOrEmpty(thumbSizeCsv))
            {

                if (!string.IsNullOrEmpty(imgPathName))
                {
                    var thumbSizeList = thumbSizeCsv.Split(',');

                    for (var lp = 0; lp <= thumbSizeList.GetUpperBound(0); lp++)
                    {
                        var thumbW = GetThumbWidth(thumbSizeList[lp]);
                        var thumbH = GetThumbHeight(thumbSizeList[lp]);
                        var filePathOut = GetThumbFilePathName(imgPathName, thumbW, thumbH);

                        using (var newImage = CreateThumbnail(imgPathName, Convert.ToInt32(thumbW), Convert.ToInt32(thumbH)))
                        {

                            if ((newImage != null))
                            {
                                ImageCodecInfo useEncoder;
                                var extension = Path.GetExtension(imgPathName);
                                if (extension != null && extension.ToLower() == ".png")
                                {
                                    useEncoder = GetEncoder(ImageFormat.Png);                                    
                                }
                                else
                                {
                                    useEncoder = GetEncoder(ImageFormat.Jpeg); 
                                }

                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 85L);

                                try
                                {
                                    newImage.Save(filePathOut, useEncoder, encoderParameters);
                                }
                                catch (Exception)
                                {
                                    GC.Collect();
                                    // attempt to clear all file locks and try again
                                    try
                                    {
                                        newImage.Save(filePathOut, useEncoder, encoderParameters);
                                    }
                                    // ReSharper disable EmptyGeneralCatchClause
                                    catch
                                    // ReSharper restore EmptyGeneralCatchClause
                                    {
                                        //abandon save. 
                                        //Assumption is the thumb already is there, but locked. So no need for error.
                                    }
                                }

                                // Clean up
                                newImage.Dispose();
                            }
                        }
                    }
                }
            }


        }

        public static Bitmap CreateThumbnail(string strFilepath, int intMaxWidth)
        {
            return CreateThumbnail(strFilepath, intMaxWidth, 0);
        }

        public static Bitmap CreateThumbnail(string strFilepath, int intMaxWidth, int intMaxHeight)
        {
            var cropType = "";
            if (intMaxHeight != 0)
            {
                if (intMaxHeight > intMaxWidth)
                {
                    cropType = "P";
                }
                else if (intMaxHeight == intMaxWidth)
                {
                    cropType = "S";
                }
                else
                {
                    cropType = "L";
                }
            }

            Bitmap newImage = null;

            if (System.IO.File.Exists(strFilepath))
            {

                using (var sourceImage = new Bitmap(strFilepath))
                {

                    var intSourceWidth = sourceImage.Width;
                    var intSourceHeight = sourceImage.Height;

                    //check if we dynamically choose Height or Width
                    if (intMaxHeight == -1)
                    {
                        intMaxHeight = 0;
                        if (intSourceWidth < intSourceHeight)
                        {
                            intMaxHeight = intMaxWidth;
                            intMaxWidth = 0;
                        }
                    }

                    if ((intMaxWidth > 0 & intMaxWidth < intSourceWidth) | (intMaxHeight > 0 & intMaxHeight < intSourceHeight))
                    {

                        // Resize image:
                        double aspect = sourceImage.PhysicalDimension.Width/sourceImage.PhysicalDimension.Height;

                        Bitmap cropImage;

                        int newWidth;
                        int newHeight;

                        if (intMaxWidth == 0)
                        {
                            intMaxWidth = Convert.ToInt32(intMaxHeight*aspect);
                        }
                        if (intMaxHeight == 0)
                        {
                            intMaxHeight = Convert.ToInt32(intMaxWidth/aspect);
                        }

                        newWidth = intMaxWidth;
                        newHeight = intMaxHeight;

                        if (string.IsNullOrEmpty(cropType))
                        {
                            newImage = new Bitmap(newWidth, newHeight);
                            var g = Graphics.FromImage(newImage);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                            g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                            g.DrawImage(sourceImage, 0, 0, newWidth, newHeight);
                        }
                        else if (cropType == "L")
                        {
                            newWidth = intMaxWidth;
                            newHeight = Convert.ToInt32(intMaxWidth/aspect);

                            cropImage = new Bitmap(newWidth, newHeight);
                            Graphics gc = Graphics.FromImage(cropImage);
                            gc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gc.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                            gc.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                            var destinationRec = new Rectangle(0, 0, newWidth, intMaxHeight);
                            newImage = new Bitmap(newWidth, intMaxHeight);
                            Graphics g = Graphics.FromImage(newImage);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.FillRectangle(Brushes.White, 0, 0, newWidth, intMaxHeight);
                            g.DrawImage(cropImage, destinationRec, 0, Convert.ToInt32((cropImage.Height - intMaxHeight)/2), newWidth, intMaxHeight, GraphicsUnit.Pixel);
                        }
                        else if (cropType == "P")
                        {
                            newWidth = Convert.ToInt32(intMaxHeight*aspect);
                            newHeight = intMaxHeight;

                            cropImage = new Bitmap(newWidth, newHeight);
                            var gc = Graphics.FromImage(cropImage);
                            gc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gc.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                            gc.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                            var destinationRec = new Rectangle(0, 0, intMaxWidth, newHeight);
                            newImage = new Bitmap(intMaxWidth, newHeight);
                            var g = Graphics.FromImage(newImage);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.FillRectangle(Brushes.White, 0, 0, intMaxWidth, newHeight);
                            g.DrawImage(cropImage, destinationRec, Convert.ToInt32((cropImage.Width - intMaxWidth)/2), 0, intMaxWidth, newHeight, GraphicsUnit.Pixel);

                        }
                        else if (cropType == "S")
                        {
                            if (sourceImage.PhysicalDimension.Height < sourceImage.PhysicalDimension.Width)
                            {
                                newWidth = Convert.ToInt32(intMaxHeight*aspect);
                                newHeight = intMaxHeight;
                            }
                            else
                            {
                                newWidth = intMaxWidth;
                                newHeight = Convert.ToInt32(intMaxWidth/aspect);
                            }

                            //-------------------------------------------
                            //Do NOT allow white space on S croptype.
                            if (newWidth < intMaxWidth)
                            {
                                newWidth = intMaxWidth;
                                newHeight = Convert.ToInt32(intMaxWidth/aspect);
                            }
                            if (newHeight < intMaxHeight)
                            {
                                newHeight = intMaxHeight;
                                newWidth = Convert.ToInt32(intMaxHeight*aspect);
                            }
                            //-------------------------------------------

                            cropImage = new Bitmap(newWidth, newHeight);
                            var gc = Graphics.FromImage(cropImage);
                            gc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gc.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                            gc.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                            var destinationRec = new Rectangle(0, 0, intMaxWidth, intMaxHeight);
                            newImage = new Bitmap(intMaxWidth, intMaxHeight);
                            var g = Graphics.FromImage(newImage);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.FillRectangle(Brushes.White, 0, 0, intMaxWidth, intMaxHeight);
                            g.DrawImage(cropImage, destinationRec, Convert.ToInt32((cropImage.Width - intMaxWidth)/2), Convert.ToInt32((cropImage.Height - intMaxHeight)/2), intMaxWidth, intMaxHeight, GraphicsUnit.Pixel);
                        }

                        //use original width (no maxwidth given or image is narrow enough: 
                    }
                    else
                    {
                        var cloneRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                        var format = sourceImage.PixelFormat;
                        newImage = sourceImage.Clone(cloneRect, format);
                        newImage.SetResolution(72, 72);
                    }

                    sourceImage.Dispose();
                }
            }
            return newImage;

        }

    }



}
