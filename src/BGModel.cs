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

namespace CVforWP8.ImageProcess
{
    //Code Book Background Model for Bootstrap Scenarios
    class BGModel
    {
        const uint NChannels = 3;
        const uint ElementMaxLength = 3;

        int nChannels;
        int nBounds;
        float rate;
        int thresh;

        int width;
        int height;
        int dataLength;

        public WriteableBitmap wbBack;
        public WriteableBitmap wbFore;

        CodeBook[] cb;

        public long frameCount = 0;
        public int buildTime = 300;
        
        struct CodeElement
        {
            public float[] mean;
            public long count;
        }

        struct CodeBook
        {
            public CodeElement[] ce;
            public int numEntries;
        }

        public struct CodeBookPara
        {
            public int nChannels;
            public int nBounds;
            public float rate;
            public int thresh;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="w">image width</param>
        /// <param name="h">image height</param>
        /// <param name="para">background model parameter</param>
        public BGModel(int w ,int h, CodeBookPara para)
        {
            nChannels = para.nChannels;
            nBounds = para.nBounds;
            rate = para.rate;
            thresh = para.thresh;

            width = w;
            height = h;
            dataLength = width * height;

            wbBack = new WriteableBitmap(width, height);
            wbFore = new WriteableBitmap(width, height);

            cb = new CodeBook[dataLength];
            for(int i=0 ;i < dataLength;i++)
            {
                cb[i].ce = new CodeElement[ElementMaxLength];
                for(int n = 0; n < ElementMaxLength; n++)
                {
                    cb[i].ce[n].mean = new float[NChannels];
                }

                cb[i].numEntries = 0;
            }

        }

        /// <summary>
        /// update model with a new frame
        /// </summary>
        /// <param name="currImg">current frame</param>
        public void updateBGModel(WriteableBitmap currImg)
        {
            if (frameCount < buildTime)
            {
                for (int c = 0; c < dataLength; c++)
                {
                    updateCodeBook(ref cb[c], currImg.Pixels[c]);
                }
                
            }
            else if(frameCount == buildTime)
            {
                updateBackImg();
            }
            else
            {
                updateForeImg(currImg);
            }

            frameCount++;
        }

        /// <summary>
        /// get current building state
        /// </summary>
        /// <returns>true for current state is building background model; false for finish building</returns>
        public bool getBuildingState()
        {
            if(frameCount < buildTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //update code book in a certain pixel position
        private int updateCodeBook(ref CodeBook codeBook, int pixel)
        {
            int matchChannels;
            int i;

            byte[] bytes = BitConverter.GetBytes(pixel);

            for(i = 0; i < codeBook.numEntries ; i++)
            {
                matchChannels = 0;
                for(int n = 0; n <nChannels; n++)
                {
                    if(codeBook.ce[i].mean[n] - nBounds <= bytes[n] && codeBook.ce[i].mean[n] + nBounds >= bytes[n])
                    {
                        matchChannels++;
                    }
                }

                if (matchChannels == nChannels)
                {
                    codeBook.ce[i].count++;
                    for (int n = 0; n < nChannels; n++)
                    {
                        codeBook.ce[i].mean[n] = codeBook.ce[i].mean[n] * (1 - rate) + bytes[n] * rate;
                    }
                    break;
                }
            }// end of for all the code element

            if(i == codeBook.numEntries)
            {
                int k;
                if(codeBook.numEntries < ElementMaxLength)
                {
                    k = codeBook.numEntries;
                    codeBook.numEntries++;
                }
                else 
                {
                    k = codeBook.numEntries - 1;
                }

                codeBook.ce[k].count = 1;
                for(int n = 0; n < nChannels; n++)
                {
                    codeBook.ce[k].mean[n] = bytes[n];
                }
            }

            sortCodeBook(ref codeBook);
            return i;
            
        }

        //sort codebook desc
        private void sortCodeBook(ref CodeBook cb)
        {
            for(int i = 0; i < cb.numEntries; i++)
            {
                for(int j = cb.numEntries - 1; j >= i+1; j--)// j > i+1 is the bug enable sort function and bother me lots time
                {
                    if(cb.ce[j].count > cb.ce[j-1].count)
                    {
                        CodeElement temp;
                        temp = cb.ce[j];
                        cb.ce[j] = cb.ce[j - 1];
                        cb.ce[j - 1] = temp;

                    }
                }
            }
        }

        //update background image with codebooks' first tuple
        private void updateBackImg()
        {
            for(int c = 0; c < dataLength; c++)
            {
                byte[] pixel = new byte[4];
                pixel[3] = 255;
                for(int n = 0; n < nChannels; n++)
                {
                    pixel[n] = (byte)cb[c].ce[0].mean[n];
                }

                wbBack.Pixels[c] = BitConverter.ToInt32(pixel , 0);
            }
        }

        //update forground image with current frame
        public void updateForeImg(WriteableBitmap currImg)
        {
            for(int c = 0; c < dataLength; c++)
            {
                byte[] backPixel = BitConverter.GetBytes(wbBack.Pixels[c]);
                byte[] currPixel = BitConverter.GetBytes(currImg.Pixels[c]);
                int retValue;
                
                if(Math.Abs(backPixel[0] - currPixel[0]) < thresh
                    && Math.Abs(backPixel[1] - currPixel[1]) < thresh
                    && Math.Abs(backPixel[2] - currPixel[2]) < thresh)
                {
                    // background 
                    //retValue = ContoursFinder.blackpixel;
                    retValue = ContoursFinder.transBlackPixel;
                }
                else
                {
                    //retValue = ContoursFinder.whitepixel;
                    retValue = ContoursFinder.blackpixel;
                }

                wbFore.Pixels[c] = retValue;
            }
        }

        //reset Backgournd model
        public void resetBGModel()
        {
            frameCount = 0;
            for(int i = 0;i < dataLength; i++)
            {
                cb[i].numEntries = 0;
            }
        }




    }// end of class 
}
