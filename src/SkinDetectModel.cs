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
    //Tool Class for skin detect
    public class SkinDetectModel
    {
        /// <summary>
        /// skin detect via RGB color space
        /// </summary>
        /// <param name="bmpSrc">3-channel input image</param>
        /// <param name="bmpMask">output mask image in which skin area is drawn white</param>
        static public void skinDetectRGB(WriteableBitmap bmpSrc, WriteableBitmap bmpMask)
        {

            int dataIndex = 0;
            byte[] pixelBytes;
            byte[] pixelMask = new byte[4];
            int a, r, g, b;

            for (int y = 0; y < bmpSrc.PixelHeight; y++)
            {
                for (int x = 0; x < bmpSrc.PixelWidth; x++)
                {

                    pixelBytes = BitConverter.GetBytes(bmpSrc.Pixels[dataIndex]);
                
                    a = pixelBytes[3];
                    r = pixelBytes[2];
                    g = pixelBytes[1];
                    b = pixelBytes[0];


                    if ((r > 95 && g > 40 && b > 20 && (r - b) > 15 && (r - g) > 15 &&!(r > 170 && g > 170 && b > 170)) || 
                        (r > 200 && g > 210 && b > 170 && Math.Abs(r - b) < 15 && r > b && g > b))
                    {
                        pixelMask[3] = 255;
                        pixelMask[2] = 255;
                        pixelMask[1] = 255;
                        pixelMask[0] = 255;
                    }
                    else
                    {
                        pixelMask[3] = 255;
                        pixelMask[2] = 0;
                        pixelMask[1] = 0;
                        pixelMask[0] = 0;
                    }
                    bmpMask.Pixels[dataIndex] = BitConverter.ToInt32(pixelMask, 0);

                    dataIndex++;
                }
            }

        }

        /// <summary>
        /// skin detect via YUV
        /// </summary>
        /// <param name="bmpSrc"></param>
        /// <param name="bmpMask"></param>
        static public void skinDetectYUV(WriteableBitmap bmpSrc, WriteableBitmap bmpMask)
        {

            int Cb_low = 80;
            int Cb_up = 120;   // original thresh: 77≤Cb≤127 

            int Cr_low = 140;
            int Cr_up = 165;   // original thresh:133≤Cr≤173

            
            int Y_up = 255;
            int Y_low = 0;

            int dataIndex = 0;
            byte[] pixelBytes;
            byte[] pixelMask = new byte[4];
            int Y, Cb, Cr;
            int ret = ContoursFinder.blackpixel;

            for (int y = 0; y < bmpSrc.PixelHeight; y++)
            { 
                for (int x = 0; x < bmpSrc.PixelWidth; x++)
                {

                    pixelBytes = BitConverter.GetBytes(bmpSrc.Pixels[dataIndex]);

                    Y = pixelBytes[2];
                    Cb = pixelBytes[1];
                    Cr = pixelBytes[0];


                    if ((Y >= Y_low && Y <= Y_up) && (Cb >= Cb_low && Cb <= Cb_up) && (Cr >= Cr_low && Cr <= Cr_up))
                    {
                        ret = ContoursFinder.whitepixel;
                    }
                    else
                    {
                        ret = ContoursFinder.blackpixel;
                    }
                    bmpMask.Pixels[dataIndex] = ret;

                    dataIndex++;
                }
            }

        }

        /// <summary>
        /// skin detect via YUV
        /// </summary>
        /// <param name="bmpSrc"></param>
        /// <param name="bmpMask"></param>
        static public void skinDetectYUV2(WriteableBitmap bmpSrc, WriteableBitmap bmpMask)
        {
            int Cb_low = 80;
            int Cb_up = 120;   

            int Cr_low = 140;
            int Cr_up = 165; 

            int Y_up = 255;
            int Y_low = 0;

            int dataIndex = 0;
            byte[] pixelBytes;
            byte[] pixelMask = new byte[4];
            int Y, Cb, Cr;
            int ret = ContoursFinder.blackpixel;

            for (int y = 0; y < bmpSrc.PixelHeight; y++)
            {
                for (int x = 0; x < bmpSrc.PixelWidth; x++)
                {

                    pixelBytes = BitConverter.GetBytes(bmpSrc.Pixels[dataIndex]);

                    Y = pixelBytes[2];
                    Cb = pixelBytes[1];
                    Cr = pixelBytes[0];


                    if ((Y >= Y_low && Y <= Y_up) && (Cb >= Cb_low && Cb <= Cb_up) && (Cr >= Cr_low && Cr <= Cr_up))
                    {
                        ret = ContoursFinder.whitepixel;
                    }
                    else
                    {
                        ret = ContoursFinder.blackpixel;
                    }
                    bmpMask.Pixels[dataIndex] = ret;

                    dataIndex++;
                }
            }

        }

        /// <summary>
        /// skin detect via Gus Model
        /// </summary>
        /// <param name="bmpSrc"></param>
        /// <param name="bmpMask"></param>
        static public void skinDetectGus(WriteableBitmap bmpSrc, WriteableBitmap bmpMask)
        {
            int dataIndex = 0;
            byte[] pixelBytes;
            byte[] pixelMask = new byte[4];
            double Y, Cb, Cr;
            int ret = ContoursFinder.blackpixel;
            double pro = 0.0;
            double proThresh = 0.954;
            double result = 0.0;

            for (int y = 0; y < bmpSrc.PixelHeight; y++)
            {
                for (int x = 0; x < bmpSrc.PixelWidth; x++)
                {

                    pixelBytes = BitConverter.GetBytes(bmpSrc.Pixels[dataIndex]);

                    Cb = (double)pixelBytes[1];
                    Cr = (double)pixelBytes[0];

                    pro = 0.1049 * Cr * Cr + 0.0726 * Cb * Cb + 0.0924 * Cr * Cb - 16.762146 * Cr - 12.452652 * Cb + 791.9079;
                    result = 1 / (2 * (3.1415 * 13.4631)) * Math.Exp(-pro / 2);

                    if (result > proThresh)
                    {
                        ret = ContoursFinder.blackpixel;
                    }
                    else
                    {
                        ret = ContoursFinder.whitepixel;
                    }
                    bmpMask.Pixels[dataIndex] = ret;

                    dataIndex++;
                }
            }

        }   

        /// <summary>
        /// skin detect with center light effects
        /// </summary>
        /// <param name="bmpSrc"></param>
        /// <param name="bmpMask"></param>
         static public void skinDetectYUVCenterLight(WriteableBitmap bmpSrc, WriteableBitmap bmpMask)
        {
           
            int cols=bmpSrc.PixelWidth;
			int rows=bmpSrc.PixelHeight;
			double l=cols>rows?rows/2:cols/2;
			
            int Cb_low = 73;
            int Cb_up = 130;   // original thresh: 77≤Cb≤127 

            int Cr_low = 130;
            int Cr_up = 176;   // original thresh:133≤Cr≤173
            
            int Y_up = 255;
            int Y_low = 0;

            int dataIndex = 0;
            byte[] pixelBytes;
            byte[] pixelMask = new byte[4];
            int Y, Cb, Cr;
			byte[] pixelRet = new byte[4];
            for (int y = 0; y < bmpSrc.PixelHeight; y++)
            { 
                for (int x = 0; x < bmpSrc.PixelWidth; x++)
                {
					double d=Math.Sqrt((double)((x-cols/2)*(x-cols/2)+(y-rows/2)*(y-rows/2)));
					double ratio=d/l;
					if(ratio > 1)
                    {
                        ratio = 1;
                    }
                    pixelBytes = BitConverter.GetBytes(bmpSrc.Pixels[dataIndex]);

                    Y = pixelBytes[2];
                    Cb = pixelBytes[1];
                    Cr = pixelBytes[0];


                    if ((Y >= Y_low && Y <= Y_up) && (Cb >= Cb_low && Cb <= Cb_up) && (Cr >= Cr_low && Cr <= Cr_up))
                    {	
                        pixelRet[3] = 255;
                        pixelRet[2] = (byte)((int)(152 - ratio * 105));
                        pixelRet[1] = (byte)((int)(112 - ratio * 84));
                        pixelRet[0] = (byte)((int)(73 - ratio * 59));
                    }
                    else
                    {
                        pixelRet[3] = 0;
                        pixelRet[2] = 0;
                        pixelRet[1] = 0;
                        pixelRet[0] = 0;
                    }

                    bmpMask.Pixels[dataIndex] = BitConverter.ToInt32(pixelRet,0);

                    dataIndex++;
                }
            }

        }
    
    }//end of class
}
