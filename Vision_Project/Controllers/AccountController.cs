using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using Vision_Project.Models;

namespace Vision_Project.Controllers
{
    public class AccountController : Controller
    {
        VisionEntities db = new VisionEntities();
        public ActionResult Index()
        {

            if (Session["User_Name"] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult Index(User u)
        {
            var row = db.Users.Where(model => model.Email == u.Email && model.Password == u.Password).FirstOrDefault();

            if (row != null)
            {
                Session["User_id"] = (int)row.Id;
                Session["User_Email"] = row.Email;
                Session["User_Status"] = row.Role_id;
                string email = row.Email;
                string username = email.Substring(0, email.IndexOf('@'));
                Session["User_Name"] = username;

                if (row.Role_id == 1)
                {
                    TempData["SucessMessage"] = "Login Successful";
                    return RedirectToAction("Index", "Home");
                }
                else if (row.Role_id == 2)
                {
                    TempData["SucessMessage"] = "Admin Login Successful";
                    ModelState.Clear();
                    return RedirectToAction("Index", "Admin");
                }
                else if (row.Role_id == 3)
                {
                    var d=db.Parties.Where(p => p.User_id ==  row.Id).FirstOrDefault();
                    Session["User_Name"] = d.Name;
                    Session["Symbol_Img"] = d.Symbol;
                    TempData["SucessMessage"] = "Party Login Successful";
                    ModelState.Clear();
                    return RedirectToAction("Index", "Party");
                }
                else
                {
                    TempData["WarningMessage"] = "Unknown user status";
                    ModelState.Clear();
                    return View();
                }
            }
            else
            {
                TempData["WarningMessage"] = "Invalid email or password";
                return View();
            }
        }

        public ActionResult Register()
        {
            if (Session["User_Name"] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Register(User u)
        {

            if (ModelState.IsValid == true)
            {
                if (u.Password == u.c_password)
                {

                    u.Role_id = 1;
                    db.Users.Add(u);
                    int a = db.SaveChanges();
                    //--
                    if (a > 0)
                    {
                        Session["User_id"] = (int)u.Id;
                        Session["User_Name"] = u.Name;
                        Session["User_Email"] = u.Email;
                        Session["User_Status"] = u.Role_id;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    TempData["WarningMessage"] = "Password does match confirm password field ! Try Again";
                    ModelState.Clear();
                    return View();

                }
                }
                else
                {
                    TempData["WarningMessage"] = "Something Went Wrong! Please Try Again!";
                    ModelState.Clear();
                    return View();


                }
                //

                //--
            
          
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Account");
        }
    }
}
