// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs">
//      Copyright 2017 Alexander Barthen
//      Licensed under the Apache License, Version 2.0;
//      you may not use this file except in compliance with the License.
// </copyright>
// <summary>
//   Attempts to provide a reliable tool with some nice additional features when dealing with visual comparison of images
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageComparisonHelper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;

    /// <summary>
    /// Attempts to provide a reliable tool with some nice additional features when dealing with visual comparison of images
    /// </summary>
    public class ImageComparisonHelper
    {
        private static volatile ImageComparisonHelper instance;
        private static readonly object syncRoot = new object();

        /// <summary> Prevents a default instance of the <see cref="ImageComparisonHelper"/> class from being created. </summary>
        private ImageComparisonHelper()
        {
        }

        /// <summary> Gets the instance and ensures that only one is available. </summary>
        public static ImageComparisonHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ImageComparisonHelper();
                    }
                }

                return instance;
            }
        }

        /// <summary> Finds all occurrences of one image inside another an returns their positions as a list. </summary>
        /// <param name="imageToSearchIn"> The image to search in. </param> 
        /// <param name="imageToFind"> The image to find. </param>
        /// <param name="stopAfterFirstFoundImage"> If set the method will exit as soon as one image has been found and the method will return a list with one item.  </param>
        /// <param name="dumpDebugScreenshotToApplicationFolder"> If set the method will dump an image consisting of the captured region and found images will be highlighted. </param>
        /// <returns> The <see cref="List"/>.  </returns>
        public List<Point> FindImagesInImage(Bitmap imageToSearchIn, Bitmap imageToFind, bool stopAfterFirstFoundImage = false, bool dumpDebugScreenshotToApplicationFolder = false)
        {
            return this._findImagesInRegion(imageToSearchIn, imageToFind, stopAfterFirstFoundImage, dumpDebugScreenshotToApplicationFolder);
        }

        /// <summary> Provides the main functionality of this helper. </summary>
        /// <param name="regionImage"> The image to search in. </param>
        /// <param name="searchImage"> The image to find. </param>
        /// <param name="findOnlyOneImage"> If set the method will exit as soon as one image has been found and the method will return a list with one item </param>
        /// <param name="dumpDebugScreenshot"> If set the method will dump an image consisting of the captured region and found images will be highlighted. </param>
        /// <returns> The <see cref="List"/>. </returns>
        private List<Point> _findImagesInRegion(Bitmap regionImage, Bitmap searchImage, bool findOnlyOneImage = false, bool dumpDebugScreenshot = true)
        {
            var sw = Stopwatch.StartNew();
            // Return value
            var retVal = new List<Point>();

            Console.WriteLine($"Image to search in: {regionImage.Width}*{regionImage.Height} pixel.");
            Console.WriteLine($"Image to find: {searchImage.Width}*{searchImage.Height} pixel.");

            // Transfoming both images into arrays to allow easier comparison
            var regionPixelArray = this.transformImageIntoPixelArray(regionImage);
            var searchImagePixelArray = this.transformImageIntoPixelArray(searchImage);

            // Starting from the top most line of pixel traversing until the last line
            for (int y = 0; y < regionImage.Height; y++)
            {
                // Traverse each line of pixels from left to right
                for (int x = 0; x < regionImage.Width; x++)
                {
                    // Trying to match the first pixel of the search image with the current pixel
                    if (this.compareColorOfPixel(regionPixelArray[x, y], searchImagePixelArray[0, 0])
                        && x + searchImage.Width <= regionImage.Width && y + searchImage.Height <= regionImage.Height)
                    {
                        // If both pixel match continue with comparing the following pixels on this line with the first line of the search image
                        bool imageMatches = true;
                        for (int i = 0; i < searchImage.Width; i++)
                        {
                            if (this.compareColorOfPixel(regionPixelArray[x + i, y], searchImagePixelArray[i, 0]))
                            {
                            }
                            else
                            {
                                imageMatches = false;
                            }
                        }

                        if (imageMatches)
                        {
                            // If the first line matches continue with the reamining lines of the search image until one pixel does not match or the end of the search image has been reached
                            for (int y2 = 1; y2 < searchImage.Height && imageMatches; y2++)
                            {
                                for (int x2 = 0; x2 < searchImage.Width; x2++)
                                {
                                    if (this.compareColorOfPixel(
                                        regionPixelArray[x + x2, y + y2],
                                        searchImagePixelArray[x2, y2]))
                                    {
                                    }
                                    else
                                    {
                                        imageMatches = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (imageMatches)
                        {
                            // If the search image has been found we will add the top left pixel to our list of found points
                            retVal.Add(new Point(x, y));

                            // If this function shall only return the first point found we will leave our search algorithm here
                            if (findOnlyOneImage)
                            {
                                if (dumpDebugScreenshot)
                                {
                                    this.drawRectanglesToBitmapAndSave(regionImage, retVal, searchImage.Width, searchImage.Height);
                                }
                                Console.WriteLine($"Elapsed time until first result was found: {sw.ElapsedMilliseconds} ms");
                                return retVal;
                            }
                        }
                    }
                }
            }

            if (dumpDebugScreenshot)
            {
                // If we
                this.drawRectanglesToBitmapAndSave(regionImage, retVal, searchImage.Width, searchImage.Height);
            }
            Console.WriteLine($"Elapsed time until the whole image has been processed: {sw.ElapsedMilliseconds} ms");
            return retVal;
        }

        /// <summary> This method will break down any given image into a 2-dimensional array with the size of the image as axis [x,y] and each item will consist of the RGB values of the pixel corresponding to its position on the image.</summary>
        /// <param name="image"> The image. </param>
        /// <returns> The <see cref="Color[,]"/>. </returns>
        private Color[,] transformImageIntoPixelArray(Bitmap image)
        {
            var horizontalLinesCount = image.Width;
            var verticalLinesCount = image.Height;

            // Put all pixels into an array
            Color[,] retVal = new Color[horizontalLinesCount, verticalLinesCount];

            for (int x = 0; x < horizontalLinesCount; x++)
            {
                for (int y = 0; y < verticalLinesCount; y++)
                {
                    retVal[x, y] = image.GetPixel(x, y);
                }
            }

            return retVal;
        }

        /// <summary> Compares the color of two pixels (Not really necessary but make the code a lot easier to read.) </summary>
        /// <param name="c1"> The first pixel. </param>
        /// <param name="c2"> The second pixel. </param>
        /// <returns> The <see cref="bool"/>. </returns>
        private bool compareColorOfPixel(Color c1, Color c2)
        {
            var retVal = c1.Equals(c2);
            return retVal;
        }

        /// <summary> Draw given rectangles to a bitmap and saves it with a timestamp to the application folder..
        /// </summary> <param name="bmp"> The bmp. </param>
        /// <param name="points"> The points. </param>
        /// <param name="rectangleWidth"> The rectangle width. </param>
        /// <param name="rectangleHeight"> The rectangle height. </param>
        private void drawRectanglesToBitmapAndSave(Bitmap bmp, List<Point> points, int rectangleWidth, int rectangleHeight)
        {
            Rectangle[] rects = new Rectangle[points.Count];
            Pen turqPen = new Pen(Color.LawnGreen, 3);

            for (int i = 0; i < points.Count; i++)
            {
                rects[i] = new Rectangle(points[i].X, points[i].Y, rectangleWidth, rectangleHeight);
            }

            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.DrawRectangles(turqPen, rects);
            }

            var filename =
                DateTime.Now.ToString().Replace(":", string.Empty) + ".bmp";
            Console.WriteLine($"Your debug image has been saved as '{filename}'.");
            bmp.Save(filename);
        }
    }
}
