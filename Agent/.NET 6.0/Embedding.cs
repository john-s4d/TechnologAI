using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    internal class Embedding
    {
        public string ModelId { get; set; }
        public int Dimensions { get; set; }
        public float[] Vector { get; set; }
    }
}
