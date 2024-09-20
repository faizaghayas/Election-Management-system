using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vision_Project.Models
{
    public class PartyVoteResult
    {
        public string PartyName { get; set; }
        public string CandidateName { get; set; }
        public string Leader { get; set; }
        public string Shortcode { get; set; }
        public string Flagimg { get; set; }
        public string Area { get; set; }
        public int VoteCount { get; set; }
        public string WinOrLoss { get; set; }
    }

}