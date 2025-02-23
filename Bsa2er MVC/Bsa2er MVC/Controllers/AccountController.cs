﻿using Bsa2er_MVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Bsa2er_MVC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationDbContext db= new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager/*,ApplicationDbContext _db*/)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var user1 = UserManager.Users.FirstOrDefault(a => a.UserName == model.username);
            if(user1.EmailConfirmed==false)
            {
                ModelState.AddModelError("", "من فضلك قم بتفعيل بريدك الالكتروني");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
           

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = UserManager.Users.SingleOrDefault(u => u.Email == model.Email);
                    if (user.Roles.Any(r=>r.RoleId=="1")||user.Roles.Any(r=>r.RoleId=="2"))
                    {
                        return RedirectToAction("DashBoardPage","DashBoard");
                    }
                    else if (user.Roles.Any(r => r.RoleId == "4"))
                    {
                        return RedirectToAction("StudentDashboard");
                    }
                    else if (user.Roles.Any(r => r.RoleId == "3"))
                    {
                        return RedirectToAction("Index", "InstructorDashboard", new { id = user.Id });
                    }

                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "خطا في اسم المستخدم او كلمة السر");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register

        //[AllowAnonymous]
        //[ActionName("StudentRegister")]
        //public ActionResult Register()
        //{
        //    ViewBag.id ="4";
        //    return View();
        //}
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register(string id)
        {
           
            if(id!=null && (User.IsInRole("Admin")||User.IsInRole("Owner")))
            {
                ViewBag.id = id;
            }
            else
            {
                ViewBag.id = "4";
            }
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(string id,RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string date = model.year+"-"+ model.month +"-"+ model.day;
                var user = new ApplicationUser { UserName = model.Username,pathofimage="/images/DashBoard/user.png",birthcountry=model.countryofbirth ,fullname = model.fullname, Email = model.Email, Country = model.Countries, Qualification = model.Qualifications, PhoneNumber = model.Phonenumber, dateofbirth = DateTime.Parse(date), gender = model.gender.ToString() ,dataOfRegister=DateTime.Now};
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if(id==db.Roles.SingleOrDefault(r=>r.Name=="Admin").Id)
                    {
                        await UserManager.AddToRolesAsync(user.Id,"Admin");

                    }
                    else if(id == db.Roles.SingleOrDefault(r => r.Name == "Instructor").Id)
                    {
                        await UserManager.AddToRolesAsync(user.Id,"Instructor");
                        db.Instructors.Add(new Instructor() { InsId = user.Id, Degree = user.Qualification });
                        db.SaveChanges();


                    }
                    else if(id == db.Roles.SingleOrDefault(r => r.Name == "Student").Id)
                    {
                       await UserManager.AddToRolesAsync(user.Id,"Student");
                        db.Students.Add(new Student() { StdId = user.Id });
                        db.SaveChanges();
                    }
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                  //  string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                   // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                   // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a><br> Your userName:" + model.Username + "<br>Your Password:" + model.Password);
                   // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                 //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a><br> Your userName:" + model.Username + "<br>Your Password:" + model.Password);
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link             " + callbackUrl + " Your userName: " + model.Username + ",  Your Password: " + model.Password);
                    

                    if (id == "4")
                        return RedirectToAction("GoConfirmYourEmail");

                    return RedirectToAction("DashBoardPage", "DashBoard");
                }
                AddErrors(result);
            }
            // If we got this far, something failed, redisplay form
            ViewBag.id = id;
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
        [AllowAnonymous]
        public ActionResult GoConfirmYourEmail()
        {
            return View();
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.username);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [Authorize(Roles = "Student")]
        public ActionResult StudentDashboard()
        {
            var userID = User.Identity.GetUserId();
            var user = db.Students.Find(userID);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult ChangeInfo(Student newUser)
        {
            if (ModelState.IsValid)
            {
                var userID = User.Identity.GetUserId();
                var user = db.Students.Find(userID);
                user.ApplicationUser.fullname = newUser.ApplicationUser.fullname;
                user.ApplicationUser.PhoneNumber = newUser.ApplicationUser.PhoneNumber;
                user.ApplicationUser.birthcountry = newUser.ApplicationUser.birthcountry;
                user.ApplicationUser.Country = newUser.ApplicationUser.Country;
                user.ApplicationUser.Qualification = newUser.ApplicationUser.Qualification;
                db.SaveChanges();
                return RedirectToAction("StudentDashboard");
            }
            return RedirectToAction("StudentDashboard");
        }

        [ValidateAntiForgeryToken]
        public ActionResult ChangePic(HttpPostedFileBase image)
        {
            if (image != null)
            {
                var userID = User.Identity.GetUserId();
                var user = db.Users.Find(userID);
                var userImage = user.pathofimage;

                if (userImage != "/images/DashBoard/user.png")
                {
                    System.IO.File.Delete(Server.MapPath(userImage));
                }
                var arr = image.FileName.Split('.');
                string filename = Guid.NewGuid() + "." + arr[arr.Length - 1];
                user.pathofimage = $"/images/{filename}";
                image.SaveAs(Server.MapPath("~/images/") + filename);
                db.SaveChanges();
                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("StudentDashboard");

                }
                else if (User.IsInRole("Instructor"))
                {
                    return RedirectToAction("Index", "InstructorDashboard");
                }
                else if (User.IsInRole("Owner") || User.IsInRole("Admin"))
                {
                    return RedirectToAction("DashBoardPage", "DashBoard");
                }
                return RedirectToAction("Index", "Error");
            }
            else
            {
                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("StudentDashboard");

                }
                else if (User.IsInRole("Instructor"))
                {
                    return RedirectToAction("Index", "InstructorDashboard");
                }
                else if (User.IsInRole("Owner") || User.IsInRole("Admin"))
                {
                    return RedirectToAction("DashBoardPage", "DashBoard");
                }
                return RedirectToAction("Index", "Error");
            }
        }

        [Authorize(Roles = "Student")]
        public ActionResult AddProgram(int id)
        {
            var userID = User.Identity.GetUserId();
            
                try
                {
                    db.StudentsPrograms.Add(new StudentsPrograms() { Std_Id = userID, Program_Id = id, Program_Status = ProgramStatus.Continuous, StartDateTime = DateTime.Now });
                    db.SaveChanges();
                }
                catch
                {
                    return RedirectToAction("StudentDashboard");
                }

            return RedirectToAction("StudentDashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                if(error.StartsWith("Name"))
                {
                    ModelState.AddModelError("", "اسم المستخدم مسجل من قبل");
                }
                else if(error.StartsWith("Email"))
                {
                    ModelState.AddModelError("", "البريد الالكتروني مسجل من قبل");
                }
                else
                {
                    ModelState.AddModelError("", error);

                }
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}