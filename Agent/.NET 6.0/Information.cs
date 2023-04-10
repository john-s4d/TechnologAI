using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Technologai
{
    public class Information
    {   
        public string? ContextId {  get; set; }
        public string? OwnerId { get; set; }
        public string? Payload { get; set; }

        public  static Information? FromJson(string json)
        {            
            return JsonSerializer.Deserialize<Information>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
