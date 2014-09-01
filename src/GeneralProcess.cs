/*
 * CV for Windows Phone 8 Library 
 * Version: 1.0
 * Update Date: 2014\9\1
 * Contributer: Michael Zhang , Weilong Zhao , Wentao Cao
 * */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CVforWP8
{
    // several functions for general process
    public class GeneralProcess
    {
        /// <summary>
        /// image left-right reverse
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>

        static public WriteableBitmap LeftRightReverse(WriteableBitmap src)
        {
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            WriteableBitmap dst = new WriteableBitmap(width, height);

            int dataIndex = 0;
            for(int y = 0 ; y<height ; y++)
            {
                for(int x = dataIndex , x1 = dataIndex + width - 1; x < dataIndex + width && x1 >= dataIndex; x++,x1--)
                {
                    dst.Pixels[x1] = src.Pixels[x];
                }
                dataIndex += width;
            }

            return dst;
        }

        /// <summary>
        /// image erode
        /// </summary>
        /// <param name="bmpSrc">image source</param>
        /// <param name="bmpDst">image output</param>
        /// <param name="kernelSize">erode kernel size</param>
        static public void Erode(WriteableBitmap bmpSrc, WriteableBitmap bmpDst, int kernelSize)
        {

            int filterOffset = (kernelSize - 1) / 2;
            int height = bmpSrc.PixelHeight;
            int width = bmpSrc.PixelWidth;
            int stepWidth = bmpSrc.PixelWidth;
            byte OriginalValue = 255;

            int dataIndex;
            byte[] pixel;
            byte[] pixelRet = new byte[4];
            byte resetValue;
            for (int curY = filterOffset; curY < height - filterOffset; curY++)
            {
                for (int curX = filterOffset; curX < width - filterOffset; curX++)
                {
                    dataIndex = curY * stepWidth + curX;
                    resetValue = OriginalValue;

                    for (int offsetY = -filterOffset; offsetY <= filterOffset; offsetY++)
                    {
                        for (int offsetX = -filterOffset; offsetX <= filterOffset; offsetX++)
                        {
                            int offsetIndex = dataIndex + offsetY * stepWidth + offsetX;
                            pixel = BitConverter.GetBytes(bmpSrc.Pixels[offsetIndex]);
                            if (pixel[0] < resetValue)
                            {
                                resetValue = pixel[0];
                            }

                        }
                    }

                    pixelRet[3] = 255;
                    pixelRet[2] = resetValue;
                    pixelRet[1] = resetValue;
                    pixelRet[0] = resetValue;
                    bmpDst.Pixels[dataIndex] = BitConverter.ToInt32(pixelRet,0);

                }
            }

        }

        /// <summary>
        /// image dilate
        /// </summary>
        /// <param name="bmpSrc">image input</param>
        /// <param name="bmpDst">image output</param>
        /// <param name="kernelSize">dilate kernel size</param>
        static public void Dilate(WriteableBitmap bmpSrc, WriteableBitmap bmpDst, int kernelSize)
        {

            int filterOffset = (kernelSize - 1) / 2;
            int height = bmpSrc.PixelHeight;
            int width = bmpSrc.PixelWidth;
            int stepWidth = bmpSrc.PixelWidth;
            byte OriginalValue = 0;

            int dataIndex;
            byte[] pixel;
            byte[] pixelRet = new byte[4];
            byte resetValue;
            for (int curY = filterOffset; curY < height - filterOffset; curY++)
            {
                for (int curX = filterOffset; curX < width - filterOffset; curX++)
                {
                    dataIndex = curY * stepWidth + curX;
                    resetValue = OriginalValue;

                    for (int offsetY = -filterOffset; offsetY <= filterOffset; offsetY++)
                    {
                        for (int offsetX = -filterOffset; offsetX <= filterOffset; offsetX++)
                        {
                            int offsetIndex = dataIndex + offsetY * stepWidth + offsetX;
                            pixel = BitConverter.GetBytes(bmpSrc.Pixels[offsetIndex]);
                            if (pixel[0] > resetValue)
                            {
                                resetValue = pixel[0];
                            }

                        }
                    }

                    pixelRet[3] = 255;
                    pixelRet[2] = resetValue;
                    pixelRet[1] = resetValue;
                    pixelRet[0] = resetValue;
                    bmpDst.Pixels[dataIndex] = BitConverter.ToInt32(pixelRet, 0);

                }
            }

        }

        /// <summary>
        /// image pixel add (if pixels in bmp1 and bmp2 are both black , the output pixel is black )
        /// </summary>
        /// <param name="bmp1">add source 1</param>
        /// <param name="bmp2">add source 2</param>
        /// <param name="bmpDst">output image</param>
        static public void imageAdd(WriteableBitmap bmp1, WriteableBitmap bmp2 , WriteableBitmap bmpDst)
        {

            int height = bmp1.PixelHeight;
            int width = bmp1.PixelWidth;
            int dataIndex = 0;
            int pixel1, pixel2;
            for(int curY = 0 ; curY < height ; curY++)
            {
                for(int curX = 0; curX < width ; curX++)
                {
                    pixel1 = bmp1.Pixels[dataIndex];
                    pixel2 = bmp2.Pixels[dataIndex];

                    if((pixel1 == ContoursFinder.blackpixel) && ( pixel2 == ContoursFinder.blackpixel))
                    {
                        bmpDst.Pixels[dataIndex] = ContoursFinder.blackpixel;
                    }
                    else
                    {
                        bmpDst.Pixels[dataIndex] = ContoursFinder.transBlackPixel;
                    }

                    dataIndex++;
                }
            }
        
        }

        /// <summary>
        /// image erode speed up exclusively for binary image
        /// </summary>
        /// <param name="input"></param>
        /// <param name="kernelSize"></param>
        /// <returns></returns>
        public static WriteableBitmap Erode2(WriteableBitmap input , int kernelSize)
        {

            int filterOffset = (kernelSize - 1) / 2;

            var p = input.Pixels;
            int width = input.PixelWidth;
            int height = input.PixelHeight;
            WriteableBitmap result = new WriteableBitmap(width, height);
            ContoursFinder.getBlackImge(result);
            var rp = result.Pixels;


            byte[] bytes = new byte[4];
            bytes[0] = 0;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 255;
            int empty = BitConverter.ToInt32(bytes,0);

            bytes[0] = 255;
            bytes[1] = 255;
            bytes[2] = 255;
            bytes[3] = 255;
            int ResultColor = BitConverter.ToInt32(bytes,0);

            int dataIndex;
            int offsetIndex;
            int cm,c;
            bool sig = false;

            for (int curY = filterOffset; curY < height - filterOffset; curY++)
            {
                for (int curX = filterOffset; curX < width - filterOffset; curX++)
                {
                    dataIndex = curY * width + curX;
                    cm = p[dataIndex];
                    if (cm == empty) { continue; }

                    sig = false;
                    for (int offsetY = -filterOffset; offsetY <= filterOffset; offsetY++)
                    {
                        if (sig == true)
                            break;

                        for (int offsetX = -filterOffset; offsetX <= filterOffset; offsetX++)
                        {
                            offsetIndex = dataIndex + offsetY * width + offsetX;
                            c = p[offsetIndex];

                            if (c == empty)
                            {
                                sig = true;
                                break;
                            }

                        }
                    }

                    if(sig == false)
                        rp[dataIndex] = cm;

                }
            }
                    
            return result;
        }

        /// <summary>
        /// image dilate speed up exclusively for binary image
        /// </summary>
        /// <param name="input"></param>
        /// <param name="kernelSize"></param>
        /// <returns></returns>
        public static WriteableBitmap Dilate2(WriteableBitmap input, int kernelSize)
        {

            int filterOffset = (kernelSize - 1) / 2;

            var p = input.Pixels;
            int width = input.PixelWidth;
            int height = input.PixelHeight;
            WriteableBitmap result = new WriteableBitmap(width, height);
            ContoursFinder.getBlackImge(result);
            var rp = result.Pixels;

            int empty = ContoursFinder.blackpixel;
            int white = ContoursFinder.whitepixel;

            int dataIndex;
            int offsetIndex;
            int cm, c;
            bool sig = false;

            for (int curY = filterOffset; curY < height - filterOffset; curY++)
            {
                for (int curX = filterOffset; curX < width - filterOffset; curX++)
                {
                    dataIndex = curY * width + curX;
                    cm = p[dataIndex];
                    if (cm == white) { rp[dataIndex] = white; continue; }

                    sig = false;
                    for (int offsetY = -filterOffset; offsetY <= filterOffset; offsetY++)
                    {
                        if (sig == true)
                            break;

                        for (int offsetX = -filterOffset; offsetX <= filterOffset; offsetX++)
                        {
                            offsetIndex = dataIndex + offsetY * width + offsetX;
                            c = p[offsetIndex];

                            if (c == white)
                            {
                                sig = true;
                                break;
                            }

                        }
                    }

                    if (sig == true)
                        rp[dataIndex] = white;

                }
            }

            return result;
        }

        /// <summary>
        /// special dilate (neighbor pixel more than thresh is dilated)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="kernelSize"></param>
        /// <param name="thresh"></param>
        /// <returns></returns>
        public static WriteableBitmap Fill(WriteableBitmap input, int kernelSize ,  int thresh)
        {

            int filterOffset = (kernelSize - 1) / 2;

            var p = input.Pixels;
            int width = input.PixelWidth;
            int height = input.PixelHeight;
            WriteableBitmap result = new WriteableBitmap(width, height);
            ContoursFinder.getBlackImge(result);
            var rp = result.Pixels;



            int black = ContoursFinder.blackpixel;
            int white = ContoursFinder.whitepixel;

            int dataIndex;
            int offsetIndex;
            int cm, c;
            bool sig = false;
            int acc = 0;
            int Thresh = thresh;

            for (int curY = filterOffset; curY < height - filterOffset; curY++)
            {
                for (int curX = filterOffset; curX < width - filterOffset; curX++)
                {
                    dataIndex = curY * width + curX;
                    cm = p[dataIndex];
                    if (cm == white) { rp[dataIndex] = white; continue; }

                    sig = false;
                    acc = 0;
                    for (int offsetY = -filterOffset; offsetY <= filterOffset; offsetY++)
                    {
                        if (sig == true)
                            break;

                        for (int offsetX = -filterOffset; offsetX <= filterOffset; offsetX++)
                        {
                            offsetIndex = dataIndex + offsetY * width + offsetX;
                            c = p[offsetIndex];

                            if (c == white)
                            {
                                acc++;
                                if (acc >= Thresh)
                                {
                                    sig = true;
                                    break;
                                }
                                
                            }

                        }
                    }

                    if (sig == true)
                        rp[dataIndex] = white;

                }
            }

            return result;
        }

        /// <summary>
        /// special erode
        /// </summary>
        /// <param name="input"></param>
        /// <param name="kernelSize"></param>
        /// <param name="thresh"></param>
        /// <returns></returns>
        public static WriteableBitmap cleanNoise(WriteableBitmap input, int kernelSize, int thresh)
        {

            int filterOffset = (kernelSize - 1) / 2;

            var p = input.Pixels;
            int width = input.PixelWidth;
            int height = input.PixelHeight;
            WriteableBitmap result = new WriteableBitmap(width, height);
            ContoursFinder.getBlackImge(result);
            var rp = result.Pixels;



            int black = ContoursFinder.blackpixel;
            int white = ContoursFinder.whitepixel;

            int dataIndex;
            int offsetIndex;
            int cm, c;
            bool sig = false;
            int acc = 0;
            int Thresh = thresh;

            for (int curY = filterOffset; curY < height - filterOffset; curY++)
            {
                for (int curX = filterOffset; curX < width - filterOffset; curX++)
                {
                    dataIndex = curY * width + curX;
                    cm = p[dataIndex];
                    if (cm == black) { rp[dataIndex] = black; continue; }

                    sig = false;
                    acc = 0;
                    for (int offsetY = -filterOffset; offsetY <= filterOffset; offsetY++)
                    {
                        if (sig == true)
                            break;

                        for (int offsetX = -filterOffset; offsetX <= filterOffset; offsetX++)
                        {
                            offsetIndex = dataIndex + offsetY * width + offsetX;
                            c = p[offsetIndex];

                            if (c == white)
                            {
                                acc++;
                                if (acc >= Thresh)
                                {
                                    sig = true;
                                    break;
                                }
                                
                            }
                        }
                    }

                    if (sig == true)
                        rp[dataIndex] = white;

                }
            }

            return result;
        }

        //color balance
        static public WriteableBitmap ColorBalance(WriteableBitmap src)
        {
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            int dataIndex = 0;
            WriteableBitmap dst = new WriteableBitmap(width, height);
            byte[] pixel;
            byte[] pixelRet = new byte[4];
            double BSum = 0.0, GSum = 0.0, RSum = 0.0;
            double BRatio = 0.0, GRatio = 0.0, RRatio = 0.0;
            for (int curY = 0; curY < height; curY++)
            {
                for (int curX = 0; curX < width; curX++)
                {
                    pixel = BitConverter.GetBytes(src.Pixels[dataIndex]);
                    BSum += pixel[0];
                    GSum += pixel[1];
                    RSum += pixel[2];
                    dataIndex++;
                }
            }

            double total = BSum + GSum + RSum;
            //BRatio = BSum / total;
            //GRatio = BSum / total;
            //RRatio = BSum / total;
            BRatio = GSum / RSum;
            GRatio = 1;
            RRatio = GSum / RSum;
            

            dataIndex = 0;
            for (int curY = 0; curY < height; curY++)
            {
                for (int curX = 0; curX < width; curX++)
                {
                    pixel = BitConverter.GetBytes(src.Pixels[dataIndex]);
                    pixelRet[3] = 255;
                    pixelRet[2] = ((byte)(int)(pixel[2] * RRatio));
                    pixelRet[1] = ((byte)(int)(pixel[1] * GRatio));
                    pixelRet[0] = ((byte)(int)(pixel[0] * BRatio));
                    dst.Pixels[dataIndex] = BitConverter.ToInt32(pixelRet, 0);
                    dataIndex++;
                }
            }

            return dst;
        }


   
       
    }// end of class
}


