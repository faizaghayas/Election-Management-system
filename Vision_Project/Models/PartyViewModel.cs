using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vision_Project.Models
{
    using Nethereum.ABI.FunctionEncoding.Attributes;

    public class PartyViewModel
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Shortcode { get; set; }
        public string Leader { get; set; }
    }


}