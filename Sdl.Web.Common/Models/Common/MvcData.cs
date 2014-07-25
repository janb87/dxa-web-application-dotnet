﻿using System.Collections.Generic;

namespace Sdl.Web.Common.Models.Common
{
    public class MvcData
    {
        public string ControllerName { get; set; }
        public string ControllerAreaName { get; set; }
        public string ActionName { get; set; }
        public string ViewName { get; set; }
        public string AreaName { get; set; }
        public Dictionary<string, string> RouteValues { get; set; }
    }
}
