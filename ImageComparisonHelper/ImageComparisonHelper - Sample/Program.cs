// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs">
//      Copyright 2017 Alexander Barthen
//      Licensed under the Apache License, Version 2.0;
//      you may not use this file except in compliance with the License.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ICH_Sample
{
    using System;
    using System.Drawing;

    using ImageComparisonHelper;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting to search all appearances of one image inside another.");

            Bitmap ImageToSearchIn = new Bitmap(ICH_Sample.Properties.Resources.Source);
            Bitmap ImageToFind = new Bitmap(ICH_Sample.Properties.Resources.SearchImage);
            var SearchResults = ImageComparisonHelper.Instance.FindImagesInImage(ImageToSearchIn, ImageToFind, false, true);

            Console.WriteLine("Finished searching. Displaying results:");

            foreach (Point point in SearchResults)
            {
                Console.WriteLine($"Image found at: {point.X}/{point.Y}");
            }
            Console.WriteLine("Press any key to close application.");

            Console.ReadKey();
        }
    }
}
