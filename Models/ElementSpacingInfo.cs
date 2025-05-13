using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocCreator01.Models
{
    /// <summary>
    /// Represents spacing information for UI elements with values for all four sides
    /// </summary>
    public class ElementSpacingInfo
    {
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
    }
}
