using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class Ability 
    {
        public string? Name { get; set; }
        public string? Description { get; set; }        
        public string? SchemaIn { get; set; }
        public string? SchemaOut { get; set; }
        public string? DoneWhen { get; set; }        
        public string? MemberId { get; set; }

        //public string Serialize() { }
        //public string Deserialize() { }
        
    }
}
