﻿using System.Globalization;

namespace GameMasterPresentation
{
    public static class Constants
    {
        public static readonly string MessageBoxName = "Configuration";
        public static readonly string GameMasterMessageBoxName = "Game Master";
        public static readonly string ConfigurationDirectoryPath = @"Configuration";
        public static readonly string ConfigurationFilePath = @"Configuration\gameMasterConfiguration.json";

        public static readonly int HorizontalLineZIndex = 50;
        public static readonly int VerticalLineZIndex = 50;
        public static readonly int BackgroundZIndex = 10;

        public static CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");
    }
}
