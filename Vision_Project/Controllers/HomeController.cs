using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vision_Project.Models;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web.UI.WebControls;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using Vision_Project.Services;

namespace Vision_Project.Controllers
{
    public class HomeController : Controller
    {
        VisionEntities db = new VisionEntities();
        // GET: Home 
        private readonly EthereumService _ethereumService;

        private readonly string privateKey = "0x12418685616d3358972baa583b7a123087576f8cca5ca4d042eda5dc728ef33c";

        public HomeController()
        {
            _ethereumService = new EthereumService(privateKey);
        }

        private Election GetCurrentElection()
        {

            var ongoingElection = db.Elections
                .Where(el => el.U_Status == 1)
                .OrderByDescending(el => el.Start_date)
                .FirstOrDefault();

            if (ongoingElection != null)
            {
                return ongoingElection;
            }

            // If no ongoing election, get the most recent completed election
            var recentCompletedElection = db.Elections
                .Where(el => el.U_Status == 0)
                .OrderByDescending(el => el.End_date)
                .FirstOrDefault();

            return recentCompletedElection;

        }

        public ActionResult Index()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 1)
            {

                var ongoing = db.Elections.FirstOrDefault(el => el.U_Status == 1);
            var completed = db.Elections.FirstOrDefault(el => el.U_Status == 0);


            var voter = db.Voter_TB.ToList();
            var candidates = db.Candidates.ToList();
            var Locations = db.Towns.ToList();

            ViewBag.VoterCount = voter.Count;
            ViewBag.CandidateCount = candidates.Count;
            ViewBag.LocationCount = Locations.Count;
            int u_id = (int)Session["User_id"];
            var cnic = db.Voter_TB.FirstOrDefault(vt => vt.User_id == u_id);

            if (cnic != null) {
                Session["Cnic_id"] = cnic.Cnic_id;

            }

            if (ongoing != null) {
                Session["Election"] = ongoing.Id;
                return View();
            }
            else if (completed != null)
            {

                Session["Election"] = completed.Id;
                return View();
            }
            else
            {
                return View();
            }
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }


        }
        public ActionResult VoterRegister()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 1)
            {
                var currentElection = GetCurrentElection();
            if (currentElection == null)
            {
                TempData["WarningMessage"] = "No active or recent elections found.";
                return View();
            }

            ViewBag.ElectionId = currentElection.Id;
            return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        [HttpPost]
        public ActionResult VoterRegister(Voter_TB v, string Cnic)
        {
            if (ModelState.IsValid)
            {
                int u_id = (int)Session["User_id"];
                var currentElection = GetCurrentElection();
                if (currentElection == null)
                {
                    TempData["WarningMessage"] = "No active or recent elections found.";
                    return View();
                }

                // Check if CNIC exists in UserInfo
                var userInfo = db.UserInfoes.FirstOrDefault(ui => ui.Cnic_No == Cnic);

                if (userInfo != null)
                {
                    var cnic_id = userInfo.Id;

                    var AlreadyRegistered = db.Voter_TB
                      .FirstOrDefault(vt => vt.User_id == u_id);
                    if(AlreadyRegistered != null)
                    {

                        TempData["WarningMessage"] = "This Account is alredy registered , make a new one to cast another vote";
                        return View();
                    }

                    // Check if CNIC is already registered in Voter_TB for the same election
                    var existingRegistration = db.Voter_TB
                        .FirstOrDefault(vt => vt.Cnic_id == cnic_id && vt.Election_id == currentElection.Id);

                    if (existingRegistration != null)
                    {
                        TempData["WarningMessage"] = "This CNIC is already registered for this election.";
                        return View();
                    }

                    // Check if CNIC is associated with another account (excluding the current user)
                    var userWithCnic = db.Voter_TB
                        .FirstOrDefault(vt => vt.Cnic_id == cnic_id && vt.User_id != u_id);

                    if (userWithCnic != null)
                    {
                        TempData["WarningMessage"] = "This CNIC is already associated with another account.";
                        return View();
                    }


                    // Proceed with registration
                    TempData["Cnic_id"] = userInfo.Id;
                    TempData["Area_id"] = userInfo.Area_id;
                    TempData["SuccessMessage"] = "We emailed you an OTP. Please enter that next. Thank you!";

                    return RedirectToAction("OTPChecker");
                }
                else
                {
                    TempData["WarningMessage"] = "This CNIC is not registered.";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        public ActionResult OTPChecker()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 1)
            {
                int u_id = (int)Session["User_id"];
            var user = db.Users.FirstOrDefault(u => u.Id == u_id);
            int num = new Random().Next(10, 20);
            var randomcode = $"{u_id}{DateTime.Today:MMdd}{num}";
            TempData["Random_code"] = randomcode;


            string fromMail = "faizaghayas16@gmail.com";
            string fromPassword = "glvn mgxx plio rugi";
            MailMessage message = new MailMessage
            {
                From = new MailAddress(fromMail, "Election Management System"),
                Subject = "One Time Password for Voter Registration",
                Body = $"<html><body>Please enter this OTP on our website to register yourself as a voter. Thank you!<br/><br/><table style='border-collapse: collapse; width: 38%;'><tr><td><b>OTP</b></td><td>{randomcode}</td></tr></table><br/><br/></body></html>",
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(user.Email));

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true
            };

            try
            {
                smtpClient.Send(message);
                return View();
            }
            catch (Exception ex)
            {

                return View();
            }
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        [HttpPost]
        public async Task<ActionResult> OTPChecker(string otp, Voter_TB v)
        {
            var currentElection = GetCurrentElection();
            if (currentElection == null)
            {
                TempData["WarningMessage"] = "No active or recent elections found.";
                return View();
            }

            var randomCode = TempData["Random_code"] as string;
            if (randomCode == null)
            {
                return RedirectToAction("Error");
            }

            if (otp == randomCode)
            {
                var cnic_id = (int)TempData["Cnic_id"];
                var u_id = (int)Session["User_id"];
                int e_id = (int)Session["Election"];

                try
                {
                    // Call the blockchain service to add the voter
                   //var transactionReceipt = await _ethereumService.AddVoter((uint)u_id, (uint)cnic_id, (uint)currentElection.Id,0);
                    var transactionReceipt = await _ethereumService.AddVoterAsync(u_id, currentElection.Id);

                    //if (transactionReceipt.Status.Value == 1) // Success
                    //{
                    // Proceed with saving the voter information in the database
                    v.User_id = u_id;
                        v.Status = 0;  // Adjust based on your requirements
                        v.Election_id = currentElection.Id;
                        v.Cnic_id = cnic_id;
                        db.Voter_TB.Add(v);

                        int a = db.SaveChanges();
                        if (a > 0)
                        {
                            TempData["SuccessMessage"] = "Congratulations, you're registered as a voter. Please vote!";
                            Session["Voter_id"] = v.Id;
                            Session["Cnic_id"] = v.Cnic_id;

                            return RedirectToAction("Voting", "Home");
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Something went wrong during the registration process!";
                            return View();
                        }
                    //}
                    //else
                    //{
                    //    TempData["WarningMessage"] = "Blockchain transaction failed!";
                    //    return View();
                    //}
                }
                catch (Exception ex)
                {
                    TempData["WarningMessage"] = "Error while processing blockchain transaction: " + ex.Message;
                    return View();
                }
            }
            else
            {
                TempData["WarningMessage"] = "Incorrect OTP";
                return View();
            }
        }

        public ActionResult DisplayCnic(int id)
        {
            var row = db.Voter_TB.FirstOrDefault(v => v.Id == id);
            if (row != null)
            {
                return View(row);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        public ActionResult Voting()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 1)
            {
                var currentElection = GetCurrentElection();

            if (currentElection == null || currentElection.U_Status != 1)
            {
                TempData["WarningMessage"] = "No ongoing election found.";
                return RedirectToAction("Index", "Home");
            }

                // Determine the voter ID
                object cnicIdSession = Session["Cnic_id"];
                if (cnicIdSession == null)
                {
                    TempData["WarningMessage"] = "No voter registered. Please register yourself first.";
                    return RedirectToAction("VoterRegister", "Home"); 
                }

                int c_id = (int)cnicIdSession;

                var voter = db.Voter_TB.FirstOrDefault(vt => vt.Cnic_id == c_id);

            if (voter == null)
            {
                TempData["WarningMessage"] = "No voter registered. Please register yourself first.";
                return RedirectToAction("VoterRegister", "Home");
            }

            var data = db.Candidates
                          .Where(c => c.Election_id == currentElection.Id && c.Area_id == voter.UserInfo.Area_id)
                          .ToList();

            TempData["SuccessMessage"] = "Vote! Remember, one can vote only once. Choose wisely!";
            return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }
        [HttpPost]
        public async Task<ActionResult> SubmitVote(int CandidateId)
        {
            int u_id = (int)Session["User_id"];

            // Retrieve voter IDs associated with the user
            var voters = db.Voter_TB
                           .Where(vt => vt.User_id == u_id)
                           .Select(vt => vt.Id)
                           .ToList();

            if (!voters.Any())
            {
                TempData["WarningMessage"] = "No associated voters found.";
                return RedirectToAction("Index", "Home");
            }

            // Retrieve voter IDs that have already voted
            var votedVoterIds = db.Votes
                                  .Where(v => voters.Contains(v.Voter_id))
                                  .Select(v => v.Voter_id)
                                  .Distinct()
                                  .ToList();

            // Find the first voter ID that has not yet voted
            var voterToVoteId = voters.FirstOrDefault(vtId => !votedVoterIds.Contains(vtId));

            if (voterToVoteId != 0)
            {
                int e_id = (int)Session["Election"];
                var vote = new Vote
                {
                    Voter_id = voterToVoteId,
                    Election_id = e_id,
                    Candidate_id = CandidateId
                };

                db.Votes.Add(vote);

                var havevoted = db.Voter_TB.FirstOrDefault(v => v.Id == vote.Voter_id);
                var candidate = db.Candidates.FirstOrDefault(c => c.Id == CandidateId);

                if (candidate != null && havevoted != null)
                {
                    candidate.VoteCount += 1;
                    havevoted.Status = 1;
                }

                try
                {

                    // Call the blockchain service
                    await _ethereumService.VoteAsync(voterToVoteId,CandidateId);

                   // await _ethereumService.AddVote( (uint)voterToVoteId, (uint)CandidateId);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Congratulations! You have voted. Please wait for the results.";
                    return RedirectToAction("Index", "Home");
                }
                catch (DbEntityValidationException ex)
                {
                    TempData["WarningMessage"] = "Validation failed. Please check the input and try again.";
                    return RedirectToAction("Voting", "Home");
                }
                catch (Exception ex)
                {
                    TempData["WarningMessage"] = "An error occurred while processing your vote. Please try again later. "+ex.Message + ".";
                    return RedirectToAction("Voting", "Home");
                }
            }
            else
            {
                TempData["WarningMessage"] = "All associated voters have already voted. Please wait for the results.";
                return RedirectToAction("Index", "Home");
            }
        }


        public ActionResult Results()
        {
            if (Session["User_id"] != null && (int)Session["User_Status"] == 1)
            {
                int u_id = (int)Session["User_id"];
            var completedElection = GetCurrentElection();

            if (completedElection == null || completedElection.U_Status != 0)
            {
                TempData["WarningMessage"] = "No completed elections found.";
                return RedirectToAction("Index", "Home");
            }

            var user = db.Voter_TB.FirstOrDefault(vt => vt.User_id == u_id);
            if (user == null)
            {
                TempData["WarningMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            int? sessionCnicId = Session["Cnic_id"] as int?;
            int c_id = sessionCnicId ?? user.Cnic_id;

            var voter = db.Voter_TB.FirstOrDefault(vt => vt.Cnic_id == c_id);
            if (voter == null)
            {
                TempData["WarningMessage"] = "Voter not found.";
                return RedirectToAction("Index", "Home");
            }

            var voterAreaId = voter.UserInfo?.Area_id;
            if (voterAreaId == null)
            {
                TempData["WarningMessage"] = "Voter area not found.";
                return RedirectToAction("Index", "Home");
            }

            // Get all candidates for the completed election and filter by voter's area
            var candidates = db.Candidates
                               .Where(c => c.Election_id == completedElection.Id && c.Area_id == voterAreaId)
                               .ToList(); 

            // Project the candidates into the PartyVoteResult
            var partyVoteResults = candidates
                                   .Select(candidate => new PartyVoteResult
                                   {
                                       PartyName = candidate.Party.Name,
                                       Leader = candidate.Party.Leader,
                                       Shortcode = candidate.Party.Shortcode,
                                       Flagimg = candidate.Party.Flagimg,
                                       CandidateName = candidate.Name,
                                       VoteCount = candidate.VoteCount
                                   })
                                   .OrderByDescending(r => r.VoteCount)
                                   .ToList();

            return View(partyVoteResults);
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }


    }
}
