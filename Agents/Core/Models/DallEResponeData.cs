using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.Agents.Models
{
    public class DallEResponseData
    {
        public int created { get; set; }
        public List<ImageUrl> data { get; set; } = new List<ImageUrl>();
    }
    public class ImageUrl
    {
        public string url { get; set; } = null!;
    }

}
