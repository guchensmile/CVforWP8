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
using System.Diagnostics;


namespace ShadowWithBack
{
	
    public class Point
    {
        public int x, y;

        public Point(int p1, int p2)
        {
            x = p1;
            y = p2;
        }

        public Point(Point other)
        {
            x = other.x;
            y = other.y;
        }

        public void copy(Point p)
        {
            x = p.x;
            y = p.y;
        }
    }

    public class Rectangle
    {
        public int x, y, height, width;

        public Rectangle(int p1, int p2, int p3, int p4)
        {
            x = p1;
            y = p2;
            height = p3;
            width = p4;
        }

        public void copy(Rectangle r)
        {
            x = r.x;
            y = r.y;
            height = r.height;
            width = r.width;
        }
    }
    
	
    public class ContoursFinder
    {
        //record the last valid point
        public Point point = new Point(0,0);

        public ContoursFinder()
        {
            point.x = 0;
            point.y = 0;
        }

        //将图像某一个矩形区域内设为全黑
        public static void SetImgBlackInARect(WriteableBitmap img, Rectangle rect)
        {
            int rowBound = rect.y + rect.height;
            int colBound = rect.x + rect.width;
            for (int row = rect.y; row <= rowBound; row++)
            {
                for (int col = rect.x; col <= colBound; col++)
                {
                    img.SetPixel(col, row, blackpixel);
                }
            }
        }

        //将图像黑白反转
        public static void negtiveImage(WriteableBitmap img)
        {
            long len = img.Pixels.Length;
            for (int i = 0; i < len; i++)
            {
                if (blackpixel == img.Pixels[i])
                    img.Pixels[i] = whitepixel;
                else
                    img.Pixels[i] = blackpixel;
            }
        }

        public static void getShadowImage(WriteableBitmap img )
        {
            int len = img.Pixels.Length;
            for(int i = 0;i < len;i++)
            {
                if(img.Pixels[i] == whitepixel)
                {
                    img.Pixels[i] = blackpixel;
                }
                else if (img.Pixels[i] == blackpixel)
                {
                    img.Pixels[i] = transBlackPixel;
                }
                else { }
            }
        }

        public static void getShadowImageOrange(WriteableBitmap img)
        {
            int len = img.Pixels.Length;
            for (int i = 0; i < len; i++)
            {
                if (img.Pixels[i] == blackpixel)
                {
                    img.Pixels[i] = OrangePixel;
                }
            }
        }

        public static void getShadowImageYellow(WriteableBitmap img)
        {
            int len = img.Pixels.Length;
            for (int i = 0; i < len; i++)
            {
                if (img.Pixels[i] == blackpixel)
                {
                    img.Pixels[i] = YellowPixel;
                }
            }
        }

        public static void getShadowImageBlue(WriteableBitmap img)
        {
            int len = img.Pixels.Length;
            for (int i = 0; i < len; i++)
            {
                if (img.Pixels[i] == whitepixel)
                {
                    img.Pixels[i] = BluePixel;
                }
                else if (img.Pixels[i] == blackpixel)
                {
                    img.Pixels[i] = transBlackPixel;
                }
                else { }
            }
        }

        //find the first white point in imgSrc, and paint the imgDst in the same position
        public static bool FindPoint(WriteableBitmap imgSrc, WriteableBitmap imgDst, Point point)
        {

            long len=imgSrc.Pixels.Length;
            for (int i = 0; i < len; i++)
            {
                if (whitepixel == imgSrc.Pixels[i])
                {
                    imgDst.Pixels[i] = whitepixel;
                    point.x = i % imgSrc.PixelWidth;
                    point.y = i / imgSrc.PixelWidth;
                    return true;
                }
            }

            return false;
        }

        //find a Minimum Enclosing Rectangle which cover all point
        public static bool ContourBoundingRect(List<Point> pointList, Rectangle rectangle)
        {
            if (pointList.Count == 0)
                return false;

            int max_x, min_x, max_y, min_y;
            min_x = max_x = pointList[0].x;
            min_y = max_y = pointList[0].y;

            foreach (Point i in pointList)
            {
                if (i.x > max_x)
                    max_x = i.x;
                else if (i.x < min_x)
                    min_x = i.x;
                if (i.y > max_y)
                    max_y = i.y;
                else if (i.y < min_y)
                    min_y = i.y;
            }
            rectangle.x = min_x;
            rectangle.y = min_y;
            rectangle.width = max_x - min_x;
            rectangle.height = max_y - min_y;
            return true;
        }

		/// <summary>
        /// utility fuction：Find Image Contours
        /// </summary>
        /// <param name="src">image src</param>
        /// <param name="dst">image dst</param>
        /// <param name="sequences">Image's Contours storing in a List(maybe there are many contours)</param>
        public static void FindContours(WriteableBitmap src, WriteableBitmap dst, List<List<Point>> sequences)
        {
            int height = src.PixelHeight;
            int width = src.PixelWidth;

            WriteableBitmap copy = new WriteableBitmap(src);

            bool bFindStartPoint;		// is this the first point of scanning process
            bool bFindPoint;			// is this a border point	
            Point StartPoint = new Point(0,0);
            Point CurrentPoint = new Point(0, 0);	// 起始边界点与当前边界点

            //Scan the continuous Contours point eight direction
            int[,] Direction = new int[8, 2] { { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 } };
            int BeginDirect;

            //先找到最左上方的边界点
            bFindStartPoint = false;
            while (FindPoint(copy, dst, StartPoint))
            {
                List<Point> sequence = new List<Point>();
                //由于起始点是在左下方，故起始扫描沿左上方向
                BeginDirect = 0;
                //跟踪边界
                bFindStartPoint = false;
                //从初始点开始扫描
                CurrentPoint.y = StartPoint.y;
                CurrentPoint.x = StartPoint.x;

                int nFailCount = 0;//一个点8领域搜索失败次数

                while (!bFindStartPoint)
                {
                    bFindPoint = false;
                    while (!bFindPoint && nFailCount < 8)
                    {
                        int nexty = CurrentPoint.y + Direction[BeginDirect, 1];
                        int nextx = CurrentPoint.x + Direction[BeginDirect, 0];

                        if (nexty < 0 || nextx < 0 || nextx >= width || nexty >= height)
                        {
                            goto Failed;
                        }
                        else
                        {
                            //沿扫描方向查看一个像素
                            int pixel = copy.GetPixeli(nextx, nexty);
                            if (pixel == whitepixel)
                            {
                                nFailCount = 0;
                                sequence.Add(new Point(CurrentPoint));//插入当前节点

                                bFindPoint = true;
                                CurrentPoint.y = nexty;
                                CurrentPoint.x = nextx;
                                if (CurrentPoint.y == StartPoint.y && CurrentPoint.x == StartPoint.x)
                                {
                                    bFindStartPoint = true;
                                }
                                dst.SetPixel(nextx, nexty, whitepixel);

                                //扫描的方向逆时针旋转两格
                                BeginDirect -= 2;
                                if (BeginDirect < 0)
                                {
                                    BeginDirect += 8;
                                }
                                continue;
                            }
                            else
                            {
                                goto Failed;
                            }
                        }


                    Failed:
                        {
                            //扫描方向顺时针旋转一格
                            BeginDirect++;
                            nFailCount++;
                            if (nFailCount == 8)
                            {
                                copy.SetPixel(CurrentPoint.x, CurrentPoint.y, blackpixel);
                                bFindStartPoint = true;
                                break;
                            }
                            if (BeginDirect == 8)
                            {
                                BeginDirect = 0;
                            }
                        }


                    }//end of while(!bFindPoint)
                }//end of while(!bFindStartPoint)
                if (sequence.Count > 1)
                {
                    sequences.Add(sequence);
                    Rectangle rectangle = new Rectangle(0, 0, 0, 0);
                    if (ContourBoundingRect(sequence, rectangle))
                        SetImgBlackInARect(copy, rectangle);
                }
            }//end of main while

        }

        //找轮廓序列2
        public static void FindContours2(WriteableBitmap src, WriteableBitmap dst, List<List<Point>> sequences)
        {
            int height = src.PixelHeight;
            int width = src.PixelWidth;

            WriteableBitmap copy = new WriteableBitmap(src);

            bool bFindStartPoint;		// 是否找到起始点及回到起始点
            bool bFindPoint;			// 是否扫描到一个边界点	
            Point StartPoint = new Point(0, 0);
            Point CurrentPoint = new Point(0, 0);	// 起始边界点与当前边界点

            //八个方向和起始扫描方向
            int[,] Direction = new int[8, 2] { { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 } };
            int BeginDirect;

            //先找到最左上方的边界点
            bFindStartPoint = false;
            while (FindPoint(copy, dst, StartPoint))
            {
                List<Point> sequence = new List<Point>();
                //由于起始点是在左下方，故起始扫描沿左上方向
                BeginDirect = 0;
                //跟踪边界
                bFindStartPoint = false;
                //从初始点开始扫描
                CurrentPoint.y = StartPoint.y;
                CurrentPoint.x = StartPoint.x;

                int nFailCount = 0;//一个点8领域搜索失败次数

                while (!bFindStartPoint)
                {
                    bFindPoint = false;
                    while (!bFindPoint && nFailCount < 8)
                    {

                            //沿扫描方向查看一个像素
                            int pixel = copy.GetPixeli(CurrentPoint.x+Direction[BeginDirect,0], CurrentPoint.y+Direction[BeginDirect,1]);
                            if (pixel == whitepixel)
                            {
                                nFailCount = 0;
                                sequence.Add(new Point(CurrentPoint));//CurrentPoint);//插入当前节点

                                bFindPoint = true;
                                CurrentPoint.y = CurrentPoint.y+Direction[BeginDirect,1];
                                CurrentPoint.x = CurrentPoint.x+Direction[BeginDirect,0];
                                if (CurrentPoint.y == StartPoint.y && CurrentPoint.x == StartPoint.x)
                                {
                                    bFindStartPoint = true;
                                }
                                dst.SetPixel(CurrentPoint.x+Direction[BeginDirect,0], CurrentPoint.y+Direction[BeginDirect,1], whitepixel);

                                //扫描的方向逆时针旋转两格
                                BeginDirect -= 2;
                                if (BeginDirect < 0)
                                {
                                    BeginDirect += 8;
                                }
                            }
                            else
                            {
                                //扫描方向顺时针旋转一格
                                BeginDirect++;
                                nFailCount++;
                                if (nFailCount == 8)
                                {
                                    copy.SetPixel(CurrentPoint.x, CurrentPoint.y, blackpixel);
                                    bFindStartPoint = true;
                                    break;
                                }
                                if (BeginDirect == 8)
                                {
                                    BeginDirect = 0;
                                }
                            }
                      }//end of while(!bFindPoint)
                }//end of while(!bFindStartPoint)
                if (sequence.Count > 1)
                {
                    sequences.Add(sequence);
                    Rectangle rectangle = new Rectangle(0, 0, 0, 0);
                    if (ContourBoundingRect(sequence, rectangle))
                        SetImgBlackInARect(copy, rectangle);
                }
            }//end of main while

        }
	
        /// <summary>
        /// A Test Function 
        /// </summary>
        /// <param name="src">image src</param>
        /// <param name="dst">image dst</param>
        public static List<Point> TestMyContours(WriteableBitmap src,WriteableBitmap dst)
        {
            List<Point> sequence = findHandContour(src);
            Rectangle rectangle = new Rectangle(0, 0, 0, 0);
            
			//paint the contour points white
            foreach(Point p in sequence)
            {
                dst.SetPixel(p.x, p.y, whitepixel);
            }

			//
            if(ContourBoundingRect(sequence,rectangle))
            {
                dst.DrawRectangle(rectangle.x, rectangle.y, rectangle.x + rectangle.width, rectangle.y + rectangle.height, whitepixel);
            }

            return sequence;
        }

        //use skin detect binary image(src) and frame-diff(diff) image to find hand center
        public static void findHandCenter(WriteableBitmap src , WriteableBitmap diff , Point p , Rectangle rect)
        {
            List<List<Point>> sequences = new List<List<Point>>();

            FindContours3(src, sequences);

            int areaMax = 0;
            int area = 0;

            //p.copy(point);

            foreach (List<Point> it in sequences)
            {
                Rectangle rectangle = new Rectangle(0, 0, 0, 0);

                if (it.Count < 20)
                    continue;


                if (ContourBoundingRect(it, rectangle))
                {
                    area = rectangle.width * rectangle.height;
                    
                    if( (area > areaMax)  && isRatioOK(diff,rectangle))
                    {
                       
                        areaMax = area;
                        p.x = rectangle.x + rectangle.width / 2;
                        p.y = rectangle.y + rectangle.height / 2;
                        rect.copy(rectangle);
                    }                     
                }
            }// end of foreach
        }

		//
        public static List<Point> findHandContour(WriteableBitmap src)
        {
            List<List<Point>> sequences = new List<List<Point>>();
            List<Point> result = new List<Point>();
            FindContours3(src, sequences);

            double maxRatio = 0.0;


            foreach (List<Point> it in sequences)
            {
                Rectangle rectangle = new Rectangle(0, 0, 0, 0);

                if (it.Count < 20)
                    continue;

                double perimeter = contourPerimeter(it);
                double area = contourArea(it);
                if(area>0&&perimeter>0)
                {
                    double ratio = area;
                    if(ratio>maxRatio)
                    {
                        maxRatio = ratio;
                        result = it;
                    }
                }
            }// end of foreach
            return result;
        }

		//
        public static Rectangle findMaxRect(WriteableBitmap src)
        {
            Rectangle rect = new Rectangle(0, 0, 0, 0);
            List<List<Point>> sequences = new List<List<Point>>();

            FindContours3(src, sequences);

            int areaMax = 0;
            int area = 0;

            foreach (List<Point> it in sequences)
            {
                Rectangle rectangle = new Rectangle(0, 0, 0, 0);

                if (it.Count < 20)
                    continue;

                if (ContourBoundingRect(it, rectangle))
                {
                    area = rectangle.width * rectangle.height;

                    if (area > areaMax)
                    {
                        areaMax = area;
                        rect.copy(rectangle);
                    }
                }
            }// end of foreach

            return rect;
        }

        private static bool isRatioOK(WriteableBitmap diff, Rectangle rectangle)
        {
            const double thresh = 0.3;
            int cnt = 0;
            int index = 0;
            int value = 0;
            for (int y = rectangle.y; y < rectangle.y + rectangle.height; y++)
            {
                for (int x = rectangle.x; x < rectangle.x + rectangle.width; x++) 
                {
                    index = y * diff.PixelWidth + x;
                    value = diff.Pixels[index];
                    if (value == whitepixel)
                        cnt++;
                }
            } 

            double ratio = (double)cnt / (rectangle.width * rectangle.height);
            if (ratio > thresh)
                return true;
            else
                return false;
        }

        public static WriteableBitmap clearOutRect(WriteableBitmap bmp,Rectangle rect)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            WriteableBitmap bmpRet = new WriteableBitmap(width, height);
            getBlackImge(bmpRet);

            int dataIndexOrg = rect.y * width + rect.x;
            int dataIndex = 0;
            for(int j = 0 ; j < rect.height ; j++)
            {
                dataIndex = dataIndexOrg + width * j;
                for(int i = 0 ; i < rect.width; i++ )
                {
                    bmpRet.Pixels[dataIndex] = bmp.Pixels[dataIndex];
                    dataIndex++;
                }
            }

            return bmpRet;

        }

        public static List<Point> findMaxSequence(WriteableBitmap src)
        {
            List<List<Point>> sequences = new List<List<Point>>();

            FindContours3(src, sequences);

            int areaMax = 0;
            int area = 0;
            List<Point> listRet = null;

            foreach (List<Point> it in sequences)
            {
                Rectangle rectangle = new Rectangle(0, 0, 0, 0);

                if (it.Count < 20)
                    continue;

                if (ContourBoundingRect(it, rectangle))
                {
                    area = rectangle.width * rectangle.height;
                    if (area > areaMax)
                    {
                        areaMax = area;
                        listRet = it;
            
                    }
                }
            }// end of foreach

            return listRet;
        }

        //get Black Image (the A Channel is 0,means transparent)
        public static void getTransBlackImge(WriteableBitmap img)
        {

            for (int i = 0; i < img.Pixels.Length; i++)
            {
                img.Pixels[i]=transBlackPixel;
            }
        }

		//get Black Image (the A Channel is 255,means black)
        public static void getBlackImge(WriteableBitmap img)
        {

            for (int i = 0; i < img.Pixels.Length; i++)
            {
                img.Pixels[i] = blackpixel;
            }
        }

		//thresholding the Image, some points are white in which is betweens low and high, otherwise black
        public static void setThreshold(WriteableBitmap src, WriteableBitmap dst, byte low, byte high)
        {
            byte[] pixelsRet = new byte[4];
            pixelsRet[3] = 255;
            pixelsRet[2] = high;
            pixelsRet[1] = high;
            pixelsRet[0] = high;
            int highpixel = BitConverter.ToInt32(pixelsRet, 0);

            pixelsRet[3] = 255;
            pixelsRet[2] = low;
            pixelsRet[1] = low;
            pixelsRet[0] = low;
            int lowpixel = BitConverter.ToInt32(pixelsRet, 0);

            long len = src.Pixels.Length;
            for (int i = 0; i < len; i++)
            {
                int value = src.Pixels[i];
                if (value < lowpixel || value > highpixel)
                    dst.Pixels[i] = blackpixel;
                else
                    dst.Pixels[i] = whitepixel;
            }
        }

		//
        public static bool FindPoint3(WriteableBitmap imgSrc, Point point)
        {

            int height = imgSrc.PixelHeight;
            int width = imgSrc.PixelWidth;

            int dataIndex = 0;

            for (int curY = 0; curY < height; curY++)
            {
                for (int curX = 0; curX < width; curX++)
                {
                    if (whitepixel == imgSrc.Pixels[dataIndex])
                    {
                        point.x = curX;
                        point.y = curY;
                        return true;
                    }
                    dataIndex++;
                }
            }

            return false;
        }

        public static double contourPerimeter(List<Point> sequence)
        {
            double perimeter = 0.0;
            int length = sequence.Count;
            if(length>0)
            {
                int preX = sequence[0].x;
                int preY = sequence[0].y;
                for(int i=1;i<length;++i)
                {
                    int dx = sequence[i].x - preX;
                    int dy = sequence[i].y - preY;
                    perimeter+=Math.Sqrt(dx*dx+dy*dy);

                    preX=sequence[i].x;
                    preY=sequence[i].y;
                }
            }
            return perimeter;
        }

        public static double contourArea(List<Point> sequence)
        {
            double area = 0.0;
            double a00 = 0.0;
            int xi_1,yi_1;
            int length=sequence.Count;
            if(length>0)
            {
                xi_1=sequence[0].x;
                yi_1=sequence[0].y;
                for(int i=1;i<length;++i)
                {
                    int dxy,xi,yi;
                    xi=sequence[i].x;
                    yi=sequence[i].y;
                    dxy=xi_1*yi-xi*yi_1;
                    a00+=dxy;

                    xi_1=xi;
                    yi_1=yi;
                }
                area=a00*0.5;
            }
            return Math.Abs(area);
        }

        public static void FindContours3(WriteableBitmap src, List<List<Point>> sequences)
        {
            int height = src.PixelHeight;
            int width = src.PixelWidth;

            WriteableBitmap copy = new WriteableBitmap(src);

            bool bFindStartPoint;		// 是否找到起始点及回到起始点
            bool bFindPoint;			// 是否扫描到一个边界点	
            Point StartPoint = new Point(0, 0);
            Point CurrentPoint = new Point(0, 0);	// 起始边界点与当前边界点

            //八个方向和起始扫描方向
            int[,] Direction = new int[8, 2] { { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 } };
            int BeginDirect;

            //先找到最左上方的边界点
            bFindStartPoint = false;
            while (FindPoint3(copy, StartPoint))
            {
                List<Point> sequence = new List<Point>();
                //由于起始点是在左下方，故起始扫描沿左上方向
                BeginDirect = 0;
                //跟踪边界
                bFindStartPoint = false;
                //从初始点开始扫描
                CurrentPoint.y = StartPoint.y;
                CurrentPoint.x = StartPoint.x;

                int nFailCount = 0;//一个点8领域搜索失败次数

                while (!bFindStartPoint)
                {
                    bFindPoint = false;
                    while (!bFindPoint && nFailCount < 8)
                    {
                        int nexty = CurrentPoint.y + Direction[BeginDirect, 1];
                        int nextx = CurrentPoint.x + Direction[BeginDirect, 0];

                        if (nexty < 0 || nextx < 0 || nextx >= width || nexty >= height)
                        {
                            goto Failed;
                        }
                        else
                        {
                            //沿扫描方向查看一个像素
                            int pixel = copy.GetPixeli(nextx, nexty);
                            if (pixel == whitepixel)
                            {
                                nFailCount = 0;
                                sequence.Add(new Point(CurrentPoint));//插入当前节点

                                bFindPoint = true;
                                CurrentPoint.y = nexty;
                                CurrentPoint.x = nextx;
                                if (CurrentPoint.y == StartPoint.y && CurrentPoint.x == StartPoint.x)
                                {
                                    bFindStartPoint = true;
                                }

                                //扫描的方向逆时针旋转两格
                                BeginDirect -= 2;
                                if (BeginDirect < 0)
                                {
                                    BeginDirect += 8;
                                }
                                continue;
                            }
                            else
                            {
                                goto Failed;
                            }
                        }


                    Failed:
                        {
                            //扫描方向顺时针旋转一格
                            BeginDirect++;
                            nFailCount++;
                            if (nFailCount == 8)
                            {
                                copy.SetPixel(CurrentPoint.x, CurrentPoint.y, blackpixel);
                                bFindStartPoint = true;
                                break;
                            }
                            if (BeginDirect == 8)
                            {
                                BeginDirect = 0;
                            }
                        }


                    }//end of while(!bFindPoint)
                }//end of while(!bFindStartPoint)
                if (sequence.Count > 1)
                {
                    sequences.Add(sequence);
                    Rectangle rectangle = new Rectangle(0, 0, 0, 0);
                    if (ContourBoundingRect(sequence, rectangle))
                        SetImgBlackInARect(copy, rectangle);
                }
            }//end of main while

        }

        public static int whitepixel = BitConverter.ToInt32(new byte[] { 255,255,255,255 }, 0);
        public static int blackpixel = BitConverter.ToInt32(new byte[] { 0, 0, 0, 255 }, 0);
        public static int greenpixel = BitConverter.ToInt32(new byte[] { 0, 255, 0, 255 }, 0);
        public static int redpixel = BitConverter.ToInt32(new byte[] { 0, 0, 255, 255 }, 0);
        public static int transBlackPixel = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0 }, 0);
        public static int GreenPixel = BitConverter.ToInt32(new byte[] { 21, 232, 40, 255 }, 0);
        public static int BluePixel = BitConverter.ToInt32(new byte[]{239 , 80 , 0 ,255}, 0);
        public static int OrangePixel = BitConverter.ToInt32(new byte[] { 6, 64, 224, 255 }, 0);
        public static int YellowPixel = BitConverter.ToInt32(new byte[] { 0, 165, 255, 255 }, 0);
    }// end of class

    public class ContourTemplate
    {
        public ContourTemplate()
        {
            contours = new List<Point>();
            sum = 0.0;
        }
        public string name;
        public string url;
        public double sum;
        public List<Point> contours;
    }

    // class and shape for match contours
    public class Moments
    {
        public Moments()
        {
            m00 = m10 = m01 = m20 = m11 = m02 = m30 = m21 = m12 = m03 =
            mu20 = mu11 = mu02 = mu30 = mu21 = mu12 = mu03 =
            nu20 = nu11 = nu02 = nu30 = nu21 = nu12 = nu03 = 0.0;
        }

        public Moments(double _m00, double _m10, double _m01, double _m20, double _m11, double _m02, double _m30, double _m21, double _m12, double _m03)
        {
            const double dbl_epsilon = 2.2204460492503131e-016;

            m00 = _m00; m10 = _m10; m01 = _m01;
            m20 = _m20; m11 = _m11; m02 = _m02;
            m30 = _m30; m21 = _m21; m12 = _m12; m03 = _m03;

            double cx = 0, cy = 0, inv_m00 = 0;
            if (Math.Abs(m00) > dbl_epsilon)
            {
                inv_m00 = 1.0 / m00;
                cx = m10 * inv_m00; cy = m01 * inv_m00;
            }

            mu20 = m20 - m10 * cx;
            mu11 = m11 - m10 * cy;
            mu02 = m02 - m01 * cy;

            mu30 = m30 - cx * (3 * mu20 + cx * m10);
            mu21 = m21 - cx * (2 * mu11 + cx * m01) - cy * mu20;
            mu12 = m12 - cy * (2 * mu11 + cy * m10) - cx * mu02;
            mu03 = m03 - cy * (3 * mu02 + cy * m01);

            double inv_sqrt_m00 = Math.Sqrt(Math.Abs(inv_m00));
            double s2 = inv_m00 * inv_m00, s3 = s2 * inv_sqrt_m00;

            nu20 = mu20 * s2; nu11 = mu11 * s2; nu02 = mu02 * s2;
            nu30 = mu30 * s3; nu21 = mu21 * s3; nu12 = mu12 * s3; nu03 = mu03 * s3;
        }

        //! spatial moments
        public double m00, m10, m01, m20, m11, m02, m30, m21, m12, m03;
        //! central moments
        public double mu20, mu11, mu02, mu30, mu21, mu12, mu03;
        //! central normalized moments
        public double nu20, nu11, nu02, nu30, nu21, nu12, nu03;
        public double inv_sqrt_m00; /* m00 != 0 ? 1/sqrt(m00) : 0 */
    }

    public class HuMoments
    {
        public double hu1, hu2, hu3, hu4, hu5, hu6, hu7; /* Hu invariants */
    };

    public class ShapesMathcer
    {
        public static void completeMomentState(Moments moments)
        {
            const double dbl_epsilon = 2.2204460492503131e-016;
            double cx = 0, cy = 0;
            double mu20, mu11, mu02;

            moments.inv_sqrt_m00 = 0;

            if (Math.Abs(moments.m00) > dbl_epsilon)
            {
                double inv_m00 = 1.0 / moments.m00;
                cx = moments.m10 * inv_m00;
                cy = moments.m01 * inv_m00;
                moments.inv_sqrt_m00 = Math.Sqrt(Math.Abs(inv_m00));
            }

            // mu20 = m20 - m10*cx
            mu20 = moments.m20 - moments.m10 * cx;
            // mu11 = m11 - m10*cy
            mu11 = moments.m11 - moments.m10 * cy;
            // mu02 = m02 - m01*cy
            mu02 = moments.m02 - moments.m01 * cy;

            moments.mu20 = mu20;
            moments.mu11 = mu11;
            moments.mu02 = mu02;

            // mu30 = m30 - cx*(3*mu20 + cx*m10)
            moments.mu30 = moments.m30 - cx * (3 * mu20 + cx * moments.m10);
            mu11 += mu11;
            // mu21 = m21 - cx*(2*mu11 + cx*m01) - cy*mu20
            moments.mu21 = moments.m21 - cx * (mu11 + cx * moments.m01) - cy * mu20;
            // mu12 = m12 - cy*(2*mu11 + cy*m10) - cx*mu02
            moments.mu12 = moments.m12 - cy * (mu11 + cy * moments.m10) - cx * mu02;
            // mu03 = m03 - cy*(3*mu02 + cy*m01)
            moments.mu03 = moments.m03 - cy * (3 * mu02 + cy * moments.m01);
        }

		//get center distance, zero order, first order, second order and third order moments in x or y driection
        public static void contoursMoments(List<Point> contour, Moments moments)
        {
            const double flt_epsilon = 1.192092896e-07F;

            if (contour.Count != 0)
            {
                double a00, a10, a01, a20, a11, a02, a30, a21, a12, a03;
                double xi, yi, xi2, yi2, xi_1, yi_1, xi_12, yi_12, dxy, xii_1, yii_1;
                int lpt = contour.Count();

                a00 = a10 = a01 = a20 = a11 = a02 = a30 = a21 = a12 = a03 = 0;

                xi_1 = contour[0].x;
                yi_1 = contour[0].y;

                xi_12 = xi_1 * xi_1;
                yi_12 = yi_1 * yi_1;

                for (int i = 0; i < lpt; ++i)
                {
                    int index = i + 1;
                    if (index == lpt) index = 0;
                    xi = contour[index].x;
                    yi = contour[index].y;

                    xi2 = xi * xi;
                    yi2 = yi * yi;
                    dxy = xi_1 * yi - xi * yi_1;
                    xii_1 = xi_1 + xi;
                    yii_1 = yi_1 + yi;

                    a00 += dxy;
                    a10 += dxy * xii_1;
                    a01 += dxy * yii_1;
                    a20 += dxy * (xi_1 * xii_1 + xi2);
                    a11 += dxy * (xi_1 * (yii_1 + yi_1) + xi * (yii_1 + yi));
                    a02 += dxy * (yi_1 * yii_1 + yi2);
                    a30 += dxy * xii_1 * (xi_12 + xi2);
                    a03 += dxy * yii_1 * (yi_12 + yi2);
                    a21 +=
                        dxy * (xi_12 * (3 * yi_1 + yi) + 2 * xi * xi_1 * yii_1 +
                        xi2 * (yi_1 + 3 * yi));
                    a12 +=
                        dxy * (yi_12 * (3 * xi_1 + xi) + 2 * yi * yi_1 * xii_1 +
                        yi2 * (xi_1 + 3 * xi));

                    xi_1 = xi;
                    yi_1 = yi;
                    xi_12 = xi2;
                    yi_12 = yi2;
                }

                double db1_2, db1_6, db1_12, db1_24, db1_20, db1_60;

                if (Math.Abs(a00) > flt_epsilon)
                {
                    if (a00 > 0)
                    {
                        db1_2 = 0.5;
                        db1_6 = 0.16666666666666666666666666666667;
                        db1_12 = 0.083333333333333333333333333333333;
                        db1_24 = 0.041666666666666666666666666666667;
                        db1_20 = 0.05;
                        db1_60 = 0.016666666666666666666666666666667;
                    }
                    else
                    {
                        db1_2 = -0.5;
                        db1_6 = -0.16666666666666666666666666666667;
                        db1_12 = -0.083333333333333333333333333333333;
                        db1_24 = -0.041666666666666666666666666666667;
                        db1_20 = -0.05;
                        db1_60 = -0.016666666666666666666666666666667;
                    }

                    // spatial moments
                    moments.m00 = a00 * db1_2;
                    moments.m10 = a10 * db1_6;
                    moments.m01 = a01 * db1_6;
                    moments.m20 = a20 * db1_12;
                    moments.m11 = a11 * db1_24;
                    moments.m02 = a02 * db1_12;
                    moments.m30 = a30 * db1_20;
                    moments.m21 = a21 * db1_60;
                    moments.m12 = a12 * db1_60;
                    moments.m03 = a03 * db1_20;

                    completeMomentState(moments);
                }
            }
        }

		//get Moment
        public static void getMoments(List<Point> contour, Moments moments)
        {
            contoursMoments(contour, moments);
        }

		//get Hu Moment
        public static void getHuMoments(Moments mState, HuMoments HuState)
        {
            double m00s = mState.inv_sqrt_m00, m00 = m00s * m00s, s2 = m00 * m00, s3 = s2 * m00s;

            double nu20 = mState.mu20 * s2,
                nu11 = mState.mu11 * s2,
                nu02 = mState.mu02 * s2,
                nu30 = mState.mu30 * s3,
                nu21 = mState.mu21 * s3, nu12 = mState.mu12 * s3, nu03 = mState.mu03 * s3;

            double t0 = nu30 + nu12;
            double t1 = nu21 + nu03;

            double q0 = t0 * t0, q1 = t1 * t1;

            double n4 = 4 * nu11;
            double s = nu20 + nu02;
            double d = nu20 - nu02;

            HuState.hu1 = s;
            HuState.hu2 = d * d + n4 * nu11;
            HuState.hu4 = q0 + q1;
            HuState.hu6 = d * (q0 - q1) + n4 * t0 * t1;

            t0 *= q0 - 3 * q1;
            t1 *= 3 * q0 - q1;

            q0 = nu30 - 3 * nu12;
            q1 = 3 * nu21 - nu03;

            HuState.hu3 = q0 * q0 + q1 * q1;
            HuState.hu5 = q0 * t0 + q1 * t1;
            HuState.hu7 = q1 * t0 - q0 * t1;
        }

		//match Shape, there are three methods
        public static double matchShapes(List<Point> contour1, List<Point> contour2, int method = 1)
        {
            Moments moments = new Moments();
            HuMoments huMoments = new HuMoments();
            double[] ma = new double[7], mb = new double[7];
            int i, sma, smb;
            double eps = 1.0E-5;
            double mmm;
            double result = 0;


            if (contour1.Count == 0 || contour2.Count == 0)
            //	throw;//std::cerr<<"contours null ptr"<<std::endl;
            {
                ;
            }
            // calculate moments of the first shape
            getMoments(contour1, moments);
            getHuMoments(moments, huMoments);

            ma[0] = huMoments.hu1;
            ma[1] = huMoments.hu2;
            ma[2] = huMoments.hu3;
            ma[3] = huMoments.hu4;
            ma[4] = huMoments.hu5;
            ma[5] = huMoments.hu6;
            ma[6] = huMoments.hu7;


            // calculate moments of the second shape
            getMoments(contour2, moments);
            getHuMoments(moments, huMoments);

            mb[0] = huMoments.hu1;
            mb[1] = huMoments.hu2;
            mb[2] = huMoments.hu3;
            mb[3] = huMoments.hu4;
            mb[4] = huMoments.hu5;
            mb[5] = huMoments.hu6;
            mb[6] = huMoments.hu7;

            switch (method)
            {
                case 1:
                    {
                        for (i = 0; i < 7; i++)
                        {
                            double ama = Math.Abs(ma[i]);
                            double amb = Math.Abs(mb[i]);

                            if (ma[i] > 0)
                                sma = 1;
                            else if (ma[i] < 0)
                                sma = -1;
                            else
                                sma = 0;
                            if (mb[i] > 0)
                                smb = 1;
                            else if (mb[i] < 0)
                                smb = -1;
                            else
                                smb = 0;

                            if (ama > eps && amb > eps)
                            {
                                ama = 1.0 / (sma * Math.Log10(ama));
                                amb = 1.0 / (smb * Math.Log10(amb));
                                result += Math.Abs(-ama + amb);
                            }
                        }
                        break;
                    }

                case 2:
                    {
                        for (i = 0; i < 7; i++)
                        {
                            double ama = Math.Abs(ma[i]);
                            double amb = Math.Abs(mb[i]);

                            if (ma[i] > 0)
                                sma = 1;
                            else if (ma[i] < 0)
                                sma = -1;
                            else
                                sma = 0;
                            if (mb[i] > 0)
                                smb = 1;
                            else if (mb[i] < 0)
                                smb = -1;
                            else
                                smb = 0;

                            if (ama > eps && amb > eps)
                            {
                                ama = sma * Math.Log10(ama);
                                amb = smb * Math.Log10(amb);
                                result += Math.Abs(-ama + amb);
                            }
                        }
                        break;
                    }

                case 3:
                    {
                        for (i = 0; i < 7; i++)
                        {
                            double ama = Math.Abs(ma[i]);
                            double amb = Math.Abs(mb[i]);

                            if (ma[i] > 0)
                                sma = 1;
                            else if (ma[i] < 0)
                                sma = -1;
                            else
                                sma = 0;
                            if (mb[i] > 0)
                                smb = 1;
                            else if (mb[i] < 0)
                                smb = -1;
                            else
                                smb = 0;

                            if (ama > eps && amb > eps)
                            {
                                ama = sma * Math.Log10(ama);
                                amb = smb * Math.Log10(amb);
                                mmm = Math.Abs((ama - amb) / ama);
                                if (result < mmm)
                                    result = mmm;
                            }
                        }
                        break;
                    }
                default:
                    break;//throw;//std::cerr<<"Unknown comparison method"<<std::endl;
            }

            return result;
        }

    }// end of class

}
