using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vision_Project.Models;
using System.Data.Entity;
using Vision_Project.Services;
using System.Threading.Tasks;
using System.Dynamic;
namespace Vision_Project.Controllers
{
    public class PartyController : Controller
    {
        // GET: Party
        VisionEntities db = new VisionEntities();
        private readonly EthereumService _ethereumService;

        public PartyController()
        {
            string privateKey = "0x12418685616d3358972baa583b7a123087576f8cca5ca4d042eda5dc728ef33c";

            _ethereumService = new EthereumService(privateKey);
        }
        public ActionResult Index()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 3)
            {
                int u_id = (int)Session["User_id"];

                // Get the party for the logged-in user
                var party = db.Parties.FirstOrDefault(p => p.User_id == u_id);
                if (party == null)
                {
                    TempData["WarningMessage"] = "Party not found.";
                    return RedirectToAction("Index", "Home");
                }

                
                var Voteslist = db.Votes.Where(v => v.Candidate.Party_id == party.Id).ToList();
                ViewBag.VotesCount = Voteslist.Count;

               
                var candidates = db.Candidates.Where(c => c.Party.User_id == u_id).ToList();
                ViewBag.CandidateCount = candidates.Count;

                
                var candidateVoteResults = GetCandidateVoteResults();

                
                dynamic mymodel = new ExpandoObject();
                mymodel.resultslist = candidateVoteResults.ToList();
                mymodel.candidatesList = candidates;

                return View(mymodel);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        private IEnumerable<CandidateVoteResult> GetCandidateVoteResults()
        {
            int u_id = (int)Session["User_id"];

            // Get candidates for the user's party
            var candidates = db.Candidates.Where(c => c.Party.User_id == u_id).ToList();

            // Get grouped vote counts for the candidates
            var voteCounts = db.Votes
                .GroupBy(v => v.Candidate_id)
                .Select(g => new
                {
                    CandidateId = g.Key,
                    VoteCount = g.Count()
                }).ToList();

            // Join candidates with their vote counts
            return from candidate in candidates
                   join voteCount in voteCounts
                   on candidate.Id equals voteCount.CandidateId into voteGroup
                   from vg in voteGroup.DefaultIfEmpty()
                   select new CandidateVoteResult
                   {
                       CandidateName = candidate.Name,
                       VoteCount = vg != null ? vg.VoteCount : 0
                   };
        }


        public ActionResult Candidates()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 3)
            {
                int userId = (int)Session["User_id"];

                var p_ids= db.Parties
                    .Where(p => p.User_id == userId)
                    .FirstOrDefault();

                int p_id = p_ids.Id;
                Session["User_PartyId"]= p_id;


                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        public ActionResult AddCandidate()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 3)
            {
                int userId = (int)Session["User_id"];

            var data = db.Candidates
                .Where(c => c.Party.User_id == userId)
                .ToList();
            List<SelectListItem> statusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "0" },
                new SelectListItem { Text = "UnActive", Value = "1" },
            };
            ViewBag.Status = statusList;


            int u_id = (int)Session["User_id"];
            ViewBag.User_id = u_id;

            List<Election> ElectionList = db.Elections.ToList();
            ViewBag.Election_list = new SelectList(ElectionList, "Id", "Name");
            List<Town> AreaList = db.Towns.ToList();
            ViewBag.Area_List = new SelectList(AreaList, "Id", "Name");
            return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }

        }
        [HttpPost]
        public async Task<ActionResult>  AddCandidate(Candidate c)
        {
            if (ModelState.IsValid == true)
            {
                //

                int userId = (int)Session["User_id"];

                var data = db.Parties
                    .Where(p => p.User_id == userId)
                    .FirstOrDefault();

                List<SelectListItem> statusList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Active", Value = "0" },
                    new SelectListItem { Text = "UnActive", Value = "1" },
                };
                ViewBag.Status = statusList;


                int u_id = (int)Session["User_id"];
                ViewBag.User_id = u_id;

                List<Election> ElectionList = db.Elections.ToList();
                ViewBag.Election_list = new SelectList(ElectionList, "Id", "Name");
                List<Town> AreaList = db.Towns.ToList();
                ViewBag.Area_List = new SelectList(AreaList, "Id", "Name");
                //
                var p_id = data.Id;
                  // Interact with the Ethereum contract
                      if (p_id > 0) {
                    // await _ethereumService.AddCandidate(c.Name, (uint)p_id, (uint)c.Election_id, (uint)c.Area_id, (int)c.Status);
                    var transactionHash = await _ethereumService.AddCandidateAsync(c.Name, p_id,c.Election_id);

                    if (string.IsNullOrEmpty(transactionHash))
                    {
                        throw new Exception("Blockchain transaction failed.");
                    }
                    c.Party_id = data.Id;
                    db.Candidates.Add(c);
                int a = db.SaveChanges();
                    if (a > 0)
                    {

                        return RedirectToAction("Candidates", "Party");
                    }
                    else
                    {

                        return View();
                    }
                }
                else{
                    TempData["WarningMessage"] = "not found";
                    return RedirectToAction("Candidates", "Party");

                }

                //--
            }
            else
            {

                return View();
            }
        }
      

        public ActionResult DeleteCandidate(int id)
        {
            var candidate = db.Candidates
                .Include(c => c.Votes) // Include related Votes
                .Include(c => c.Election) // Include related Election
                .Include(c => c.Party) // Include related Party
                .SingleOrDefault(c => c.Id == id);

            if (candidate == null)
            {
                TempData["WarningMessage"] = "Candidate not found.";
                return RedirectToAction("Index"); // Redirect to the list of candidates or another view
            }

            try
            {
                // Remove the candidate
                db.Candidates.Remove(candidate);

                // Save changes to apply deletion
                db.SaveChanges();

                TempData["SuccessMessage"] = "Candidate deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Failed to delete candidate. " + ex.Message;
            }

            return RedirectToAction("Index"); // Redirect to the list of candidates or another view
        }
        public ActionResult ConfirmDelete(int id)
        {
            var candidate = db.Candidates
                .Include(c => c.Party) // Include related data if necessary
                .Include(c => c.Election) // Include related data if necessary
                .SingleOrDefault(c => c.Id == id);

            if (candidate == null)
            {
                TempData["WarningMessage"] = "Candidate not found.";
                return RedirectToAction("Index");
            }

            return View(candidate);
        }


        public ActionResult Results()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 3)
            {
                int u_id = (int)Session["User_id"];

            var party = db.Parties.FirstOrDefault(p => p.User_id == u_id);
            if (party == null)
            {
                TempData["WarningMessage"] = "Party not found.";
                return RedirectToAction("Index", "Home");
            }

            var allCandidates = from candidate in db.Candidates
                                where candidate.Party_id == party.Id
                                join p in db.Parties on candidate.Party_id equals p.Id
                                select new
                                {
                                    CandidateId = candidate.Id,
                                    CandidateName = candidate.Name
                                };

            var voteCounts = from vote in db.Votes
                             group vote by vote.Candidate_id into g
                             select new
                             {
                                 CandidateId = g.Key,
                                 VoteCount = g.Count()
                             };

            var candidateVoteResults = from candidate in allCandidates.ToList()
                                       join voteCount in voteCounts.ToList()
                                       on candidate.CandidateId equals voteCount.CandidateId into voteGroup
                                       from vg in voteGroup.DefaultIfEmpty()
                                       select new CandidateVoteResult
                                       {
                                           CandidateName = candidate.CandidateName,
                                           VoteCount = vg != null ? vg.VoteCount : 0
                                       };

            var resultsList = candidateVoteResults.ToList();

            if (resultsList == null)
            {
                resultsList = new List<CandidateVoteResult>();
                }

                return View(resultsList);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }


        }
}