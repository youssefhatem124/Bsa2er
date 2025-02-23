﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Bsa2er_MVC.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage ="يجب عليك ادخال البريد الالكتروني")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string  Email { get; set; }

        public string username { set; get; }

        [Required(ErrorMessage ="يجب عليك ادخال كلمة السر")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }


    public class RegisterViewModel
    {
        public Gender gender { set; get; }
        public string Username { set; get; }
        public string fullname { set; get; }
        private const string V = "ادخل بريدك الالكتروني";
        [Required(ErrorMessage = V)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string imagepath { set; get; }

        [Required(ErrorMessage = "يجب ادخال كلمة المرور")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب ان تكون 10 أرقام علي الاقل", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "لا تطابق كلمة المرور")]
        public String ConfirmPassword { get; set; }
        public String Countries { set; get; }
        public String Qualifications { set; get; }
        public String Phonenumber { set; get; }
       
        public string month { set; get; }
        
        public string year { set; get; }
       
        public string day { set; get; }

        public string countryofbirth { set; get; }

        
    }
    public enum Gender
    {
        male,
        female
    }


    public class ResetPasswordViewModel
    {

        public string username { set; get; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب ان تكون 6 حروف علي الاقل", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "لا تطابق كلمة المرور")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
