﻿using System;

namespace Sdl.Web.Mvc.Models
{
    public class YouTubeVideo : MediaItem
    {
        public string Headline { get; set; }
        public string YouTubeId { get; set; }
        public int FileSize { get; set; }
        // TODO determine correct width and height or allow to be set
        public int Width { get { return 640; } }
        public int Height { get { return 390; } }
    }
}