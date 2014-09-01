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

using Windows.Phone.Media.Capture;
using System.Windows.Media;
using Windows.Phone.Media;
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.ComponentModel; 


namespace CVforWP8
{
    public class ColorSpaceConverter
    {
        /// <summary>
        /// image color space from rgb to yuv
        /// </summary>
        /// <param name="imgSrc"></param>
        /// <param name="imgDst"></param>
        public static void RGB2YCbCr(WriteableBitmap imgSrc, WriteableBitmap imgDst)
        {
            if(imgSrc.PixelHeight != imgDst.PixelHeight || imgSrc.PixelWidth != imgDst.PixelWidth)
            {
                return;
            }
            int height = imgSrc.PixelHeight;
            int width = imgSrc.PixelWidth;
            

            int dataIndex = 0;
            byte R, G, B;
            byte Y, Cb, Cr;
            byte[] pixels;
            byte[] pixelsRet = new byte[4];
            for(int curY = 0; curY < height; curY++)
            {
                for(int curX = 0; curX < width ; curX++)
                {
                    pixels = BitConverter.GetBytes(imgSrc.Pixels[dataIndex]);
                    R = pixels[2];
                    G = pixels[1];
                    B = pixels[0];

                    //Y  = clip(((66 * R + 129 * G + 25 * B) >> 8) + 16);
                    //Cb = clip(((-38 * R - 74 * G + 112 * B) >> 8) + 128);
                    //Cr = clip(((112 * R - 94 * G - 18 * B) >> 8) + 128);

                    //this transmition is cooprated with skin threshold
                    Y = clip((int)(0.2990 * R + 0.5870 * G + 0.1140 * B));
                    Cr = clip((int)(0.5000 * R - 0.4187 * G - 0.0813 * B + 128));
                    Cb = clip((int)(-0.1687 * R - 0.3313 * G + 0.5000 * B + 128));

                    pixelsRet[3] = 255;
                    pixelsRet[2] = Y;
                    pixelsRet[1] = Cb;
                    pixelsRet[0] = Cr;

                    imgDst.Pixels[dataIndex] = BitConverter.ToInt32(pixelsRet, 0);

                    dataIndex++;  
                }
            }

        }

        // image color space from yuv to rgb
        public static void YCbCr2RGB(WriteableBitmap imgSrc,WriteableBitmap imgDst)
        {
            if (imgSrc.PixelHeight != imgDst.PixelHeight || imgSrc.PixelWidth != imgDst.PixelWidth)
            {
                return;
            }
            int height = imgSrc.PixelHeight;
            int width = imgSrc.PixelWidth;


            int dataIndex = 0;
            byte R, G, B;
            byte Y, Cb, Cr;
            int C, D, E;
            byte[] pixels;
            byte[] pixelsRet = new byte[4];
            for (int curY = 0; curY < height; curY++)
            {
                for (int curX = 0; curX < width; curX++)
                {
                    pixels = BitConverter.GetBytes(imgSrc.Pixels[dataIndex]);
                    Y = pixels[2];
                    Cb = pixels[1];
                    Cr = pixels[0];

                    C = Y - 16;
                    D = Cb - 128;
                    E = Cr - 128;

                    R = clip((298 * C + 409 * E + 128) >> 8);
                    G = clip((298 * C - 100 * D - 208 * E + 128) >> 8);
                    B = clip((298 * C + 516 * D + 128) >> 8);


                    pixelsRet[3] = 255;
                    pixelsRet[2] = R;
                    pixelsRet[1] = G;
                    pixelsRet[0] = B;

                    imgDst.Pixels[dataIndex] = BitConverter.ToInt32(pixelsRet, 0);

                    dataIndex++;
                }
            }
        }

        //pixel value beyong (0,255) is cliped
        static private byte clip(int n)
        {
            if(n > 255)
            {
                n = 255;
            }

            if(n < 0)
            {
                n = 0;
            }

            return (byte)n;
        }

        //image color space rgb to gray
        public static void RGB2Gray(WriteableBitmap imgSrc,WriteableBitmap imgDst)
        {
            if (imgSrc.PixelHeight != imgDst.PixelHeight || imgSrc.PixelWidth != imgDst.PixelWidth)
            {
                return;
            }
            int height = imgSrc.PixelHeight;
            int width = imgSrc.PixelWidth;


            int dataIndex = 0;
            byte R, G, B;
            byte[] pixels;
            byte[] pixelsRet = new byte[4];
            byte grayPixel;
            for (int curY = 0; curY < height; curY++)
            {
                for (int curX = 0; curX < width; curX++)
                {
                    pixels = BitConverter.GetBytes(imgSrc.Pixels[dataIndex]);
                    R = pixels[2];
                    G = pixels[1];
                    B = pixels[0];


                    grayPixel = (byte)(.299 * R + .587 * G + .114 * B);

                    pixelsRet[3] = 255;
                    pixelsRet[2] = grayPixel;
                    pixelsRet[1] = grayPixel;
                    pixelsRet[0] = grayPixel;

                    imgDst.Pixels[dataIndex] = BitConverter.ToInt32(pixelsRet, 0);

                    dataIndex++;
                }
            }
        }
    
    
    
    
    }// end of class
}
