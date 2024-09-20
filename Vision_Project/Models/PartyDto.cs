using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Numerics;


namespace Vision_Project.Models
{
    using Nethereum.ABI.FunctionEncoding.Attributes;
    [FunctionOutput]
    public class PartyDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "id", 1)]
        public BigInteger Id { get; set; }

        [Parameter("string", "name", 2)]
        public string Name { get; set; }

        [Parameter("string", "email", 3)]
        public string Email { get; set; }

        [Parameter("string", "shortcode", 4)]
        public string Shortcode { get; set; }

        [Parameter("string", "leader", 5)]
        public string Leader { get; set; }
    }
}