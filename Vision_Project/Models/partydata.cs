using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vision_Project.Models
{
    public class partydata
    {
        public List<CandidateVoteResult> CandidateResults { get; set; }
        public List<Candidate> candidatesList { get; set; }
    }
}