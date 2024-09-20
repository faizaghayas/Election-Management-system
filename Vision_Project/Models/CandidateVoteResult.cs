using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vision_Project.Models
{
    public class CandidateVoteResult
    {
        public string CandidateName { get; set; }
        public int VoteCount { get; set; }
        public string Area { get; set; }
    }

}