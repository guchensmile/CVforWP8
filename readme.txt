Copyright:

This is a computer vision source library for Windows Phone 8 platform.
The main contributers are HuizeZhang , WeilongZhao and WentaoCao from Xidian University.
The library is designed for Computer Vision Application on Windows Phone 8 Platform .The library is free to use in either free software or commercial ones.

Version:

CVforWP81.0  updated on 9/1/2014

Useage:

The lib is provided as source code , developers should add this source code files to your own project to use the functions.
the mian structure and functions are listed below:

GeneralProcess:
    image erode (general)
    image dilate (general)
    image erode (binary)
    image dilate (binary)
    image left-right reverse
    special image erode (clean noise)
    color balance(in RGB color space)
    image add
ColorSpaceConvertor:
    rgb2YCbCr
    YCbCr2rgb
    rgb2gray
SkinDetectModel:
    skin detect in YUV
    skin detect in RGB
    skin detect with Guassian Model
BGModel:
    background subtraction with codebook("real-time foreground-background segmentation using codebook model")