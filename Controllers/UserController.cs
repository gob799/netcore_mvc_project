using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using v29.Models;
using v29.Helpers;
using Newtonsoft.Json;
using System;

namespace v29.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public UserController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult UserList()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("101")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("101")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        public IActionResult UserInsert()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("108")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("108")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpGet]
        public IActionResult UserEdit(string id)
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("102")){
                ViewBag.Gid = id;
                var outRes =  UserHelper.GetUserD(int.Parse(id));
                if(outRes.GetType().ToString() != "System.String"){
                    ViewBag.udata = outRes;
                    return View();
                } else {
                    return Content("user not found");
                }
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("102")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpPost]
        [HttpGet]
        public IActionResult UserListAc(string id,string ac)
        {
           if(ac == "insert"){
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("108")){

                    if(String.IsNullOrWhiteSpace(Request.Form["txt_username"].ToString())) // This will prevent exception when textbox is empty
                    {
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "ชื่อผู้ใช้ไม่ถูกต้อง";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if (!System.Text.RegularExpressions.Regex.IsMatch(Request.Form["txt_username"].ToString(), "^[a-zA-Z0-9]+$"))
                    {
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "ชื่อผู้ใช้อนุญาติให้ใช้ A-Z,0-9";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(Request.Form["select_groupid"].ToString().Length < 1){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "กรุณาเลือกสิทธิ";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(Request.Form["txt_password"].ToString().Length < 3){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "รหัสผ่านสั้นเกินไป";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(Request.Form["txt_password"].ToString() != Request.Form["txt_repassword"].ToString()){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "รหัสผ่านไม่ตรงกัน";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    RespondModel resOut = UserHelper.Insert(Request.Form["txt_username"].ToString(),Request.Form["txt_password"].ToString(),Request.Form["select_groupid"].ToString(),
                        Request.Form["txt_fname"].ToString(),Request.Form["txt_lname"].ToString(),Request.Form["select_dob"].ToString(),Request.Form["select_gender"].ToString());
                    if(resOut.res_status == 200){
                        ViewData["Type"] = "success";
                        ViewData["Backpage"] = "/User/UserList/";
                        return View("~/Views/Shared/Components/Bark.cshtml");

                    } else {
                        ViewData["Type"] = "error";
                        ViewData["Text"] = resOut.res_errortxt;
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }

                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("108")){
                    ViewData["Type"] = "error";
                    ViewData["Text"] = "No Permission";
                    return View("~/Views/Shared/Components/Bark.cshtml");
                } else {
                    return RedirectToAction("SignoutAc", "Account");
                }
            } else  if(ac == "update"){
                var vpassword = "";
                var vgroup = "";
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("102")){
                    if(Request.Form["txt_fname"].ToString().Length < 3){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "ชื่อสั้นเกินไป";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(Request.Form["txt_lname"].ToString().Length < 3){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "นามสกุลสั้นเกินไป";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(Request.Form["select_gender"].ToString().Length < 1){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "กรุณาเลือกเพศ";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(Request.Form["select_dob"].ToString().Length < 10){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "กรุณาระบุวันเกิด";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    if(UserHelper.CheckPerm("103")){
                        if(Request.Form["txt_repassword"].ToString().Length > 0 && Request.Form["txt_repassword"].ToString().Length < 3){
                            ViewData["Type"] = "error";
                            ViewData["Text"] = "รหัสผ่านต้องมากกว่า 3 ตัวอักษร";
                            return View("~/Views/Shared/Components/Bark.cshtml");
                        }
                        if(Request.Form["txt_password"].ToString() != Request.Form["txt_repassword"].ToString() ){
                            ViewData["Type"] = "error";
                            ViewData["Text"] = "รหัสผ่านไม่ตรงกัน";
                            return View("~/Views/Shared/Components/Bark.cshtml");
                        }
                        vpassword = Request.Form["txt_repassword"].ToString();
                    }
                    if(UserHelper.CheckPerm("104")){
                        vgroup = Request.Form["select_groupid"].ToString();
                    }
                    RespondModel resOut = UserHelper.UpdateDetail(id, Request.Form["txt_fname"].ToString(),Request.Form["txt_lname"].ToString(),Request.Form["select_gender"].ToString(),Request.Form["select_dob"].ToString(),vpassword,vgroup);
                    if(resOut.res_status == 200){
                        ViewData["Type"] = "success";
                        ViewData["Backpage"] = "/User/UserList/";
                        return View("~/Views/Shared/Components/Bark.cshtml");

                    } else {
                        ViewData["Type"] = "error";
                        ViewData["Text"] = resOut.res_errortxt;
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("102")){
                    ViewData["Type"] = "error";
                    ViewData["Text"] = "No Permission";
                    return View("~/Views/Shared/Components/Bark.cshtml");
                } else {
                    return RedirectToAction("SignoutAc", "Account");
                }

            } else  if(ac == "changestate"){
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("105")){
                    RespondModel resOut = UserHelper.UpdateState(Request.Form["rid"].ToString());
                    if(resOut.res_status == 200){
                        return Content("ok");
                    } else {
                        return Content(resOut.res_errortxt);
                    }
                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("105")){
                    return Content("No Permission.");
                } else {
                    return Content("No Permission.");
                }

            } else {
                return Content("No Action");
            }
        }
        [HttpPost]
        public IActionResult UserListJson()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("101")){
                return Content(UserHelper.GetList(Request.Form["draw"],Request.Form["start"],Request.Form["length"],Request.Form["search[value]"],Request.Form["order[0][column]"],Request.Form["order[0][dir]"]));
            } else {
                return Content("No Permissions");
            }
        }
        public IActionResult PermissionList()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("10")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("10")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpPost]
        public IActionResult PermissionListJson()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("10")){
                return Content(PermissionsIHelp.GetList(Request.Form["draw"],Request.Form["start"],Request.Form["length"],Request.Form["search[value]"],Request.Form["order[0][column]"],Request.Form["order[0][dir]"]));
            } else {
                return Content("No Permissions");
            }
        }
         public IActionResult PermissionInsert()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("12")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("12")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpGet]
        public IActionResult PermissionEdit(string id)
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("13")){
                ViewBag.Gid = id;
                var outRes = PermissionsIHelp.GetOne(id);
                if(outRes.res_status == 200){
                    ViewBag.Permdata = PermissionsIHelp.GetOne(id).res_val;
                    return View();
                } else {
                    return Content(outRes.res_errortxt);
                }
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("13")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpPost]
        [HttpGet]
        public IActionResult PermissionAc(string id,string ac)
        {
           if(ac == "insert"){
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("12")){
                    if(Request.Form["txt_name"].ToString().Length < 3){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "ชื่อสั้นเกินไป";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    RespondModel resOut = PermissionsIHelp.InsertOne(Request.Form["txt_name"].ToString(),Request.Form["perm[]"].ToString().Replace(',','|'));
                    if(resOut.res_status == 200){
                        ViewData["Type"] = "success";
                        ViewData["Backpage"] = "/User/PermissionList/";
                        return View("~/Views/Shared/Components/Bark.cshtml");

                    } else {
                        ViewData["Type"] = "error";
                        ViewData["Text"] = resOut.res_errortxt;
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("12")){
                    ViewData["Type"] = "error";
                    ViewData["Text"] = "No Permission";
                    return View("~/Views/Shared/Components/Bark.cshtml");
                } else {
                    return RedirectToAction("SignoutAc", "Account");
                }
            } else  if(ac == "update"){
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("13")){
                    if(Request.Form["txt_name"].ToString().Length < 3){
                        ViewData["Type"] = "error";
                        ViewData["Text"] = "ชื่อสั้นเกินไป";
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                    RespondModel resOut = PermissionsIHelp.UpdateOne(id,Request.Form["txt_name"].ToString(),Request.Form["perm[]"].ToString().Replace(',','|'));
                    if(resOut.res_status == 200){
                        ViewData["Type"] = "success";
                        ViewData["Backpage"] = "/User/PermissionList/";
                        return View("~/Views/Shared/Components/Bark.cshtml");

                    } else {
                        ViewData["Type"] = "error";
                        ViewData["Text"] = resOut.res_errortxt;
                        return View("~/Views/Shared/Components/Bark.cshtml");
                    }
                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("13")){
                    ViewData["Type"] = "error";
                    ViewData["Text"] = "No Permission";
                    return View("~/Views/Shared/Components/Bark.cshtml");
                } else {
                    return RedirectToAction("SignoutAc", "Account");
                }
            } else  if(ac == "changestate"){
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("14")){
                    RespondModel resOut = PermissionsIHelp.UpdateState(Request.Form["rid"].ToString());
                    if(resOut.res_status == 200){
                        return Content("ok");
                    } else {
                        return Content(resOut.res_errortxt);
                    }
                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("14")){
                    return Content("No Permission.");
                } else {
                    return Content("No Permission.");
                }
            } else  if(ac == "delete"){
                if(UserHelper.CheckSignin() && UserHelper.CheckPerm("15")){
                    RespondModel resOut = PermissionsIHelp.DeleteOne(Request.Form["rid"].ToString());
                    if(resOut.res_status == 200){
                        return Content("ok");
                    } else {
                        return Content(resOut.res_errortxt);
                    }
                } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("15")){
                    return Content("No Permission.");
                } else {
                    return Content("No Permission.");
                }

            } else {
                return Content("No Action");
            }
        }
        public IActionResult BanList(){
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("2")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("2")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpPost]
        public IActionResult BanListJson()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("2")){
                return Content(BanHelper.GetBanList(Request.Form["draw"],Request.Form["start"],Request.Form["length"],Request.Form["search[value]"],Request.Form["order[0][column]"],Request.Form["order[0][dir]"]));
            } else {
                return Content("No Permissions");
            }
        }
        [HttpPost]
        public IActionResult BanListAc()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("3")){
                    RespondModel resOut = BanHelper.UpdateBanState(Request.Form["rid"].ToString());
                    if(resOut.res_status == 200){
                        return Content("ok");
                    } else {
                        return Content(resOut.res_errortxt);
                    }
            } else {
                return Content("No Permissions");
            }
        }
        public IActionResult LogsList(){
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("4")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("4")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpPost]
        public IActionResult LogsListJson()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("4")){
                return Content(BanHelper.GetBanLogList(Request.Form["draw"],Request.Form["start"],Request.Form["length"],Request.Form["search[value]"],Request.Form["order[0][column]"],Request.Form["order[0][dir]"]));
            } else {
                return Content("No Permissions");
            }
        }
        [HttpPost]
        public IActionResult LogsListAc()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("5")){
                    RespondModel resOut = BanHelper.ClearBanLogs(Request.Form["cleanup"].ToString());
                    if(resOut.res_status == 200){
                        return Content("ok");
                    } else {
                        return Content(resOut.res_errortxt);
                    }
            } else {
                return Content("No Permissions");
            }
        }
        public IActionResult UserLogsList(){
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("6")){
                return View();
            } else if (UserHelper.CheckSignin() && !UserHelper.CheckPerm("6")){
                return Content("No Permissions");
            } else {
                return RedirectToAction("SignoutAc", "Account");
            }
        }
        [HttpPost]
        public IActionResult UserLogsListJson()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("4")){
                return Content(UserHelper.GetUserLogList(Request.Form["draw"],Request.Form["start"],Request.Form["length"],Request.Form["search[value]"],Request.Form["order[0][column]"],Request.Form["order[0][dir]"]));
            } else {
                return Content("No Permissions");
            }
        }
        [HttpPost]
        public IActionResult UserLogsListAc()
        {
            if (UserHelper.CheckSignin() && UserHelper.CheckPerm("7")){
                    RespondModel resOut = UserHelper.ClearUserLogs(Request.Form["cleanup"].ToString());
                    if(resOut.res_status == 200){
                        return Content("ok");
                    } else {
                        return Content(resOut.res_errortxt);
                    }
            } else {
                return Content("No Permissions");
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

