using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vision_Project.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using static System.Collections.Specialized.BitVector32;
using System.Threading.Tasks;
using Vision_Project.Services;
using Vision_Project.Models;
using Nethereum.Web3;
using System.Threading;
using Nethereum.ABI.Model;
using System.Dynamic;
using System.Web.ApplicationServices;

namespace Vision_Project.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin

        private readonly EthereumService _ethereumService;
        VisionEntities db = new VisionEntities();

        public AdminController()
        {
            string privateKey = "0x12418685616d3358972baa583b7a123087576f8cca5ca4d042eda5dc728ef33c";
            _ethereumService = new EthereumService(privateKey);
        }
        private (Election Election, string Status) GetCurrentElection()
        {
            var ongoingElection = db.Elections
                .Where(el => el.U_Status == 1)
                .OrderByDescending(el => el.Start_date)
                .FirstOrDefault();

            if (ongoingElection != null)
            {
                return (ongoingElection, "Ongoing");
            }

            // If no ongoing election, get the most recent completed election
            var recentCompletedElection = db.Elections
                .Where(el => el.U_Status == 0)
                .OrderByDescending(el => el.End_date)
                .FirstOrDefault();

            return (recentCompletedElection, "Completed");
        }
        ////var ethereumService = new EthereumService(privateKey);
        ////var party = await ethereumService.GetPartyAsync(1);
        ////var totalVotes = await ethereumService.GetTotalVotesAsync();
        public ActionResult Index()
        {
            // Security: Ensure session values are valid
            if (Session["User_id"] == null || (int)Session["User_Status"] != 2)
            {
                return RedirectToAction("Index", "Account");
            }

            // Get counts
            var voter = db.Voter_TB.ToList();
            var candidates = db.Candidates.ToList();
            var partieslist = db.Parties.ToList();
            var cnic = db.UserInfoes.ToList();

            ViewBag.VoterCount = voter.Count;
            ViewBag.CandidateCount = candidates.Count;
            ViewBag.PartyCount = partieslist.Count;
            ViewBag.CnicCount = cnic.Count;

            // Check for current election
            var (election, status) = GetCurrentElection();
            if (election == null)
            {
                TempData["WarningMessage"] = "Please add an election to move further";
                return RedirectToAction("AddElection", "Admin");
            }

            ViewBag.Election = election;
            ViewBag.Status = status;

            // Call the private method to get candidate vote results
            var candidateVoteResults = GetCandidateVoteResults();

            // Create a dynamic model
            dynamic mymodel = new ExpandoObject();
            mymodel.candidatelist = candidateVoteResults.ToList();

            return View(mymodel);
        }

        // Private Method inside the controller to get candidate vote results
        private IEnumerable<PartyVoteResult> GetCandidateVoteResults()
        {
            return from candidate in db.Candidates
                   join p in db.Parties on candidate.Party_id equals p.Id
                   join vote in (
                       from v in db.Votes
                       group v by v.Candidate_id into g
                       select new { CandidateId = g.Key, VoteCount = g.Count() }
                   ) on candidate.Id equals vote.CandidateId into voteGroup
                   from vg in voteGroup.DefaultIfEmpty()
                   select new PartyVoteResult
                   {
                       CandidateName = candidate.Name,
                       PartyName = p.Name,
                       Area = candidate.Town.Name,
                       VoteCount = vg != null ? vg.VoteCount : 0
                   };
        }

        public ActionResult Roles()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var data = db.Role_TB.ToList();
                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }


        }
        public ActionResult AddRole()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        [HttpPost]
        public ActionResult AddRole(Role_TB r)
        {
            if (ModelState.IsValid == true)
            {
                //
                db.Role_TB.Add(r);
                int a = db.SaveChanges();
                //--
                if (a > 0)
                {

                    return RedirectToAction("Roles", "Admin");
                }
                else
                {

                    return View();
                }
                //--
            }
            else
            {

                return View();
            }
        }
        public ActionResult DeleteRole(int id)
        {
            var role = db.Role_TB
                .Include(r => r.Users)  // Load related users
                .SingleOrDefault(r => r.Id == id);

            if (role != null)
            {
                db.Role_TB.Remove(role);  // This should trigger cascading deletes
                int result = db.SaveChanges();

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Role and related data deleted successfully.";
                }
                else
                {
                    TempData["WarningMessage"] = "Failed to delete role.";
                }
            }
            else
            {
                TempData["WarningMessage"] = "Role not found.";
            }

            return RedirectToAction("Index", "Admin");
        }


        public ActionResult ElectionTypes()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var data = db.ElectionTypes.ToList();
                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }

        }
        public ActionResult AddE_Type()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        [HttpPost]
        public ActionResult AddE_Type(ElectionType et)
        {
            if (ModelState.IsValid == true)
            {
                //
                db.ElectionTypes.Add(et);
                int a = db.SaveChanges();
                //--
                if (a > 0)
                {

                    return RedirectToAction("ElectionTypes", "Admin");
                }
                else
                {

                    return View();
                }
                //--
            }
            else
            {

                return View();

            }
        }

        public ActionResult DeleteE_type(int id)
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var electionType = db.ElectionTypes
                                .Include(et => et.Elections)
                                .SingleOrDefault(et => et.Id == id);

                if (electionType != null)
                {
                    foreach (var election in electionType.Elections.ToList())
                    {
                        db.Elections.Remove(election);
                    }

                    db.ElectionTypes.Remove(electionType);

                    db.SaveChanges();
                    TempData["SucessMessage"] = "Election type and related elections deleted successfully.";





                    return RedirectToAction("ElectionTypes");
                }
                else
                {
                    TempData["WarningMessage"] = "Election type not found.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }


        public ActionResult Election()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var ongoing = db.Elections.FirstOrDefault(el => el.U_Status == 1);
                if (ongoing != null)
                {
                    Session["Election"] = ongoing.Id;
                }
                var data = db.Elections.ToList();
                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        public ActionResult AddElection()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                List<ElectionType> typeList = db.ElectionTypes.ToList();
                ViewBag.TypeList = new SelectList(typeList, "Id", "Type");

                List<SelectListItem> statusList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Ongoing", Value = "1" },
            new SelectListItem { Text = "Completed", Value = "0" }
        };
                ViewBag.StatusList = statusList;  // Ensure this matches the key used in the view

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        [HttpPost]
        public ActionResult AddElection(Election e)
        {
            if (ModelState.IsValid)
            {
                List<SelectListItem> statusList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Ongoing", Value = "1" },
            new SelectListItem { Text = "UpComing", Value = "0" }
        };
                ViewBag.StatusList = statusList;

                List<ElectionType> typeList = db.ElectionTypes.ToList();
                ViewBag.TypeList = new SelectList(typeList, "Id", "Type");

                db.Elections.Add(e);
                int a = db.SaveChanges();

                if (a > 0)
                {
                    return RedirectToAction("Election", "Admin");
                }
            }

            return View();
        }

        public ActionResult E_Status(int id)
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var election = db.Elections.FirstOrDefault(e => e.Id == id);
                if (election != null)
                {
                    election.U_Status = election.U_Status == 1 ? 0 : 1;
                    db.SaveChanges();

                    TempData["SucessMessage"] = "status updated successfully.";
                }
                else
                {
                    TempData["WarningMessage"] = "record not found.";
                }

                return RedirectToAction("Election");   
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        public ActionResult Voters()
        {
            var data = db.Voter_TB.ToList();
            return View(data);
        }



        //public async Task<ActionResult> Parties()
        //{
        //    if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
        //    {
        //        var blockchainParties = new List<Party>();


        //        uint totalParties = await _ethereumService.GetTotalPartiesAsync();

        //        for (uint i = 0; i < totalParties; i++)
        //        {
        //            var party = await _ethereumService.GetPartyAsync(i);
        //            blockchainParties.Add(party);
        //        }

        //        return View(blockchainParties);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Account");
        //    }
        //}
        public ActionResult Parties()
        {
            

            return View();
        }

        public ActionResult AddParties()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        [HttpPost]
        public async Task<ActionResult>  AddParties(Party party , User user)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (party.Image_file != null && party.Image_file.ContentLength > 0)
                    {
                        string filename = Path.GetFileNameWithoutExtension(party.Image_file.FileName);
                        string extension = Path.GetExtension(party.Image_file.FileName);
                        filename = filename + "_" + Guid.NewGuid().ToString() + extension;
                        party.Flagimg = "~/Content/images/" + filename;
                        string path = Path.Combine(Server.MapPath("~/Content/images/"), filename);
                        party.Image_file.SaveAs(path);
                    }

                    if (party.Leader_Image != null && party.Leader_Image.ContentLength > 0)
                    {
                        string filename = Path.GetFileNameWithoutExtension(party.Leader_Image.FileName);
                        string extension = Path.GetExtension(party.Leader_Image.FileName);
                        filename = filename + "_" + Guid.NewGuid().ToString() + extension;
                        party.Leader_img = "~/Content/images/" + filename;
                        string path = Path.Combine(Server.MapPath("~/Content/images/"), filename);
                        party.Leader_Image.SaveAs(path);
                    }

                    if (party.Symbol_img != null && party.Symbol_img.ContentLength > 0)
                    {
                        string filename = Path.GetFileNameWithoutExtension(party.Symbol_img.FileName);
                        string extension = Path.GetExtension(party.Symbol_img.FileName);
                        filename = filename + "_" + Guid.NewGuid().ToString() + extension;
                        party.Symbol = "~/Content/images/" + filename;
                        string path = Path.Combine(Server.MapPath("~/Content/images/"), filename);
                        party.Symbol_img.SaveAs(path);
                    }
                    user.Email = party.Email; // Set user email from party (if applicable)
                    user.Role_id = 3; // Assign role ID as required
                    user.Password = "123"; // Set a default password or handle appropriately

                    db.Users.Add(user);
                    db.SaveChanges();
                    // Save the party details to the SQL database
                    // Handle file uploads
                    party.User_id = user.Id;
                    party.Status = 0;
                    db.Parties.Add(party);
                    db.SaveChanges();

                    // Call the smart contract to save party data to the blockchain
                    var transactionHash = await _ethereumService.AddPartyAsync(party.Name, party.Email, party.Shortcode, party.Leader);

                    if (string.IsNullOrEmpty(transactionHash))
                    {
                        throw new Exception("Blockchain transaction failed" +
                            ".");
                    }

                    // Commit the database transaction
                    transaction.Commit();


                    // Redirect to a confirmation page or the list of parties
                    return RedirectToAction("Parties","Admin");
                }
                catch (Exception ex)
                {
                    // Rollback the database transaction in case of error
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                    if(ex.InnerException != null)
                    {
                        ModelState.AddModelError(string.Empty, "An error occurredyt: " + ex.InnerException);

                    }
                    return View(party);
                }
            }
        }

       

        public ActionResult DeleteParty(int id)
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var party = db.Parties.Find(id);

            if (party == null)
            {
                TempData["WarningMessage"] = "Party not found.";
                return RedirectToAction("Index");
            }

            try
            {
                db.Parties.Remove(party);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Party deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Failed to delete party. " + ex.Message;
            }

            return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        //public ActionResult Votes()
        //{
        //    var data = db.Votes.ToList();
        //    return View(data);
        //}
       
        public ActionResult UserInfo()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var data = db.UserInfoes.ToList();
            return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }

        }

        public ActionResult AddU_Info()
        {
            List<Town> AreaList = db.Towns.ToList();
            ViewBag.Area_List = new SelectList(AreaList, "Id", "Name");

            return View();

        }

        [HttpPost]
        public ActionResult AddU_Info(UserInfo ui)
        {
            if (ModelState.IsValid == true)
            {

                List<Town> AreaList = db.Towns.ToList();
                ViewBag.Area_List = new SelectList(AreaList, "Id", "Name");



                //
                db.UserInfoes.Add(ui);
                int a = db.SaveChanges();
                //--
                if (a > 0)
                {

                    return RedirectToAction("UserInfo", "Admin");
                }
                else
                {

                    return View();
                }

            }
            else
            {
                TempData["WarningMessage"] = "Something Went wrong Try Again";
                return View();
            }
        }


        public ActionResult Towns()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                var data = db.Towns.ToList();
            return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        public ActionResult AddTown()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        [HttpPost]
        public ActionResult AddTown(Town t)
        {
            if (ModelState.IsValid == true)
            {
                //
                db.Towns.Add(t);
                int a = db.SaveChanges();
                //--
                if (a > 0)
                {

                    return RedirectToAction("Towns", "Admin");
                }
                else
                {

                    return View();
                }
                //--
            }
            else
            {

                return View();
            }
        }
      
        public ActionResult Results()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                int u_id = (int)Session["User_id"];

                var allCandidates = from candidate in db.Candidates
                                    join p in db.Parties on candidate.Party_id equals p.Id
                                    select new
                                    {
                                        CandidateId = candidate.Id,
                                        CandidateName = candidate.Name,
                                        PartyName = p.Name,
                                        Area = candidate.Town.Name,
                                    };

                var voteCounts = from vote in db.Votes
                                 group vote by vote.Candidate_id into g
                                 select new
                                 {
                                     CandidateId = g.Key,
                                     VoteCount = g.Count()
                                 };

                var candidateVoteResults = from candidate in allCandidates
                                           join voteCount in voteCounts
                                           on candidate.CandidateId equals voteCount.CandidateId into voteGroup
                                           from vg in voteGroup.DefaultIfEmpty()
                                           select new PartyVoteResult
                                           {
                                               CandidateName = candidate.CandidateName,
                                               PartyName = candidate.PartyName,
                                               Area = candidate.Area, // Include the Area here
                                               VoteCount = vg != null ? vg.VoteCount : 0
                                           };

                // Assuming you are using this result somewhere in your view
                return View(candidateVoteResults.ToList());

            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        public ActionResult PartyResults()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 2)
            {
                // Get the current election and its status
                var (election, status) = GetCurrentElection();

            if (election == null)
            {
                TempData["WarningMessage"] = "No elections found.";
                return RedirectToAction("Index", "Home");
            }

            // Get all parties
            var parties = db.Parties.ToList();

            // Get votes and count them grouped by party
            var voteCounts = from vote in db.Votes
                             join candidate in db.Candidates on vote.Candidate_id equals candidate.Id
                             join party in db.Parties on candidate.Party_id equals party.Id
                             where candidate.Election_id == election.Id
                             group new { vote, party } by party.Id into g
                             select new
                             {
                                 PartyId = g.Key,
                                 PartyName = g.FirstOrDefault().party.Name,
                                 VoteCount = g.Count()
                             };

            // Get all parties with their vote counts
            var partyResults = from party in parties
                               join voteCount in voteCounts on party.Id equals voteCount.PartyId into voteGroup
                               from vg in voteGroup.DefaultIfEmpty()
                               select new PartyVoteResult
                               {
                                   PartyName = party.Name,
                                   VoteCount = vg != null ? vg.VoteCount : 0
                               };

            // Determine the winner and loser
            var sortedResults = partyResults.OrderByDescending(pr => pr.VoteCount).ToList();
            var highestVoteCount = sortedResults.FirstOrDefault()?.VoteCount ?? 0;
            var lowestVoteCount = sortedResults.LastOrDefault()?.VoteCount ?? 0;

            var finalResults = sortedResults.Select(pr => new PartyVoteResult
            {
                PartyName = pr.PartyName,
                VoteCount = pr.VoteCount,
                WinOrLoss = pr.VoteCount == highestVoteCount ? "Winner" : (pr.VoteCount == lowestVoteCount ? "Loser" : "Other")
            }).ToList();

            // Add the election information to the model
            ViewBag.Election = election;
            ViewBag.Status = status;

            return View(finalResults);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }


    }
}

