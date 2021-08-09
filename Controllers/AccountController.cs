using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using v29.Models;
using v29.Helpers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace v29.Controllers
{
    public class AccountController : Controller
    {

        [Route("signin_ac")]
        [HttpPost]
        public IActionResult SigninAc(string vusername, string vpassword)
        {
            if (vusername != null && vpassword != null)
            {
                var takeSign = UserHelper.TakeSignin(vusername,vpassword);
                return Content(takeSign);
            }
            else
            {
                return Content("Invalid Account");
            }
        }

        [Route("signout_ac")]
        [HttpGet]
        public IActionResult SignoutAc()
        {
            HttpContext.Session.Remove("admin_username");
            HttpContext.Session.Remove("admin_password");
            HttpContext.Session.Remove("tokens");
            return RedirectToAction("Signin");
        }

        public IActionResult Signin()
        {
            if(Startup.Configuration["DevState"] == "true"){
                UserHelper.CheckSignin();
                return RedirectToAction("Index", "Home");
            }
            if (HttpContext.Session.GetString("admin_username") != null && HttpContext.Session.GetString("admin_password") != null)
            {
                if(UserHelper.CheckSignin()){
                    return RedirectToAction("Index", "Home");
                } else {
                    HttpContext.Session.Remove("admin_username");
                    HttpContext.Session.Remove("admin_password");
                    HttpContext.Session.Remove("tokens");
                    return View();
                }
            } else {
                return View();
            }

        }

        public IActionResult Profile()
        {
            if(UserHelper.CheckSignin()){
                return View();
            } else {
                return RedirectToAction("SignoutAc","Account");
            }
        }

        [HttpPost]
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> ProfileAcAsync(string value,string ac="",string password="",string repassword="",List<IFormFile> avatar = null)
        {
            var sendout = new Dictionary<string,dynamic>();
            if(UserHelper.CheckSignin()){
                if(ac == "gender"){
                    var res = UserHelper.UpdateOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_gender",value);
                    if(res == "ok"){
                         UserHelper.setUserLogs($"เปลี่ยนเพศเป็น {value}");
                         sendout.Add("status","ok");
                    } else {
                        sendout.Add("status","error");
                        sendout.Add("text",res);
                    }
                }else if(ac == "fname"){
                    var res = UserHelper.UpdateOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_fname",value);
                    if(res == "ok"){
                         UserHelper.setUserLogs($"เปลี่ยนชื่อเป็น {value}");
                         sendout.Add("status","ok");
                    } else {
                        sendout.Add("status","error");
                        sendout.Add("text",res);
                    }
                }else if(ac == "lname"){
                    var res = UserHelper.UpdateOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_lname",value);
                    if(res == "ok"){
                         UserHelper.setUserLogs($"เปลี่ยนนามสกุลเป็น {value}");
                         sendout.Add("status","ok");
                    } else {
                        sendout.Add("status","error");
                        sendout.Add("text",res);
                    }
                }else if(ac == "dob"){
                    var resetyear = value.ToString().Replace("-", "");
                    if(int.Parse(resetyear) > 24009999){
                        resetyear = (int.Parse(resetyear)-5430000).ToString();
                    }
                    var reset_year = resetyear.ToString().Substring(0,4);
                    var reset_month = resetyear.ToString().Substring(4,2);
                    var reset_day = resetyear.ToString().Substring(resetyear.Length-2,2);
                    var resetyearf = reset_year+"-"+reset_month+"-"+reset_day;
                    var res = UserHelper.UpdateOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_birth",value);
                    if(res == "ok"){
                         UserHelper.setUserLogs($"เปลี่ยนวันเกิดเป็น {value}");
                         sendout.Add("status","ok");
                    } else {
                        sendout.Add("status","error");
                        sendout.Add("text",res);
                    }
                }else if(ac == "uploadavatar"){
                    if(avatar != null){
                        var marks = new string[]{"image/png", "image/jpeg"};
                        foreach (var formFile in avatar)
                        {
                            if (formFile.Length > 0 && marks.Contains(formFile.ContentType))
                            {
                                DateTimeOffset dateTime = DateTimeOffset.Now;
                                var fileUpname = EncryptHelper.RandomStr() + ""+ dateTime.ToUnixTimeMilliseconds().ToString() + ".png";
                                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars/", fileUpname);
                                using (var stream = System.IO.File.Create(uploadPath))
                                {
                                    await formFile.CopyToAsync(stream);
                                    RespondModel res = UserHelper.GetOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_id");
                                    if(res.res_status != 200){
                                        sendout.Add("status","error");
                                        sendout.Add("text",res.res_errortxt);
                                    } else {
                                        var ugetDetai1 = JsonConvert.DeserializeObject<dynamic>(res.res_val)[0];
                                        if(ugetDetai1.sys_usersd_avatar.ToString() != "noimage"){
                                            System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars/", ugetDetai1.sys_usersd_avatar.ToString()));
                                        }
                                        var res2 = UserHelper.UpdateOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_avatar",fileUpname);
                                        if(res2 == "ok"){
                                            UserHelper.setUserLogs($"เปลี่ยนภาพ avatar");
                                            sendout.Add("status","ok");
                                            sendout.Add("filename","/uploads/avatars/"+fileUpname);
                                        } else {
                                            sendout.Add("status","error");
                                            sendout.Add("text",res2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }else if(ac == "deleteavatar"){
                        RespondModel res = UserHelper.GetOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_id");
                        if(res.res_status != 200){
                            sendout.Add("status","error");
                            sendout.Add("text",res.res_errortxt);
                        } else {
                            var ugetDetai1 = JsonConvert.DeserializeObject<dynamic>(res.res_val)[0];
                            if(ugetDetai1.sys_usersd_avatar.ToString() != "noimage"){
                                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars/", ugetDetai1.sys_usersd_avatar.ToString()));
                            }
                            var res2 = UserHelper.UpdateOne(Startup.userd["sys_users_id"].ToString(),"systems_users_detail","sys_usersd_avatar","noimage");
                            if(res2 == "ok"){
                                UserHelper.setUserLogs($"ลบภาพ avatar");
                                sendout.Add("status","ok");
                                sendout.Add("filename","/img/avatar-no-image.png");
                            } else {
                                sendout.Add("status","error");
                                sendout.Add("text",res2);
                            }
                        }

                } else {
                    sendout.Add("status","error");
                    sendout.Add("text","no actions");
                }
                string json = JsonConvert.SerializeObject(sendout, Formatting.Indented);
                return Content(json);
            } else {
                HttpContext.Session.Remove("admin_username");
                HttpContext.Session.Remove("admin_password");
                return Content("Plaese Sigin!");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
