using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using v29.Models;
using v29.Helpers;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace v29.Controllers
{
   

    public class WithdrawController : Controller
    {
        private IConfiguration Configuration;
        
 
        public WithdrawController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        

        private bool checkSessionUser(){
            
            return UserHelper.CheckSignin();
        }

        // public IActionResult Index()
        // {
             
        //     if (checkSessionUser()){
        
        //         List<WithdrawModel> lists = new List<WithdrawModel>();
        
        //         string constr = this.Configuration.GetConnectionString("localString");
        //         using (MySqlConnection con = new MySqlConnection(constr))
        //         {
                    
        //             string query = "CALL GET_TRANS_WITHDRAW()";
        //             using (MySqlCommand cmd = new MySqlCommand(query))
        //             {
        //                 cmd.Connection = con;
        //                 con.Open();
        //                 using (MySqlDataReader sdr = cmd.ExecuteReader())
        //                 {
        //                     while (sdr.Read())
        //                     {
        //                         lists.Add(new WithdrawModel
        //                         {
        //                             //data1 = Convert.ToInt32(sdr["sys_users_username"]),
        //                             wtd_id = Convert.ToInt32(sdr["wtd_id"]),
        //                             wtd_botname = sdr["wtd_botname"].ToString(),
        //                             wtd_code = sdr["wtd_code"].ToString(),
        //                             wtd_username = sdr["wtd_username"].ToString(),
        //                             wtd_bank = sdr["wtd_bank"].ToString(),
        //                             wtd_bankacc = sdr["wtd_bankacc"].ToString(),
        //                             wtd_amount = sdr["wtd_amount"].ToString(),
        //                             wtd_status = sdr["wtd_status"].ToString(),
        //                             wtd_ctime = sdr["wtd_ctime"].ToString(),
        //                             wtd_bottime = sdr["wtd_bottime"].ToString(),
        //                             wtd_memberid = sdr["wtd_memberid"].ToString(),
        //                             wtd_status_desc = sdr["wtd_status_desc"].ToString()

                                
        //                         });
        //                     }
        //                 }
        //                 con.Close();
        //             }
        //         }
        
        //         return View(lists);
        //     }
        //     else
        //     {
        //         return RedirectToAction("Signin", "Account");
        //     }
            
        // }

        [Route("/api/Get_WD_List")]
        [HttpGet]
        public IActionResult Get_WD_List(int action_id){
            
            

            DataTable dtResult = new DataTable("table");
            string constr = this.Configuration.GetConnectionString("localString");
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                
                string query = "CALL GET_TRANS_WITHDRAW("+ action_id+")";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                        MySqlDataAdapter daAdapter = null;
                        daAdapter = new MySqlDataAdapter(cmd);
                        daAdapter.Fill(dtResult);
                    
                        con.Close();
                }
            }
            return Json(Newtonsoft.Json.JsonConvert.SerializeObject(dtResult), new JsonSerializerOptions{ WriteIndented = true, });

            
        }

       
        public IActionResult Settings_amnt()
        {
            if (checkSessionUser()){
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
        }
        //  public IActionResult WD_list_after()
        // {
             
        //     if (checkSessionUser()){
        
        //         List<WithdrawModel> lists = new List<WithdrawModel>();
        
        //         string constr = this.Configuration.GetConnectionString("localString");
        //         using (MySqlConnection con = new MySqlConnection(constr))
        //         {
                    
        //             string query = "CALL GET_TRANS_WITHDRAW(7)";
        //             using (MySqlCommand cmd = new MySqlCommand(query))
        //             {
        //                 cmd.Connection = con;
        //                 con.Open();
        //                 using (MySqlDataReader sdr = cmd.ExecuteReader())
        //                 {
        //                     while (sdr.Read())
        //                     {
        //                         lists.Add(new WithdrawModel
        //                         {
        //                             //data1 = Convert.ToInt32(sdr["sys_users_username"]),
        //                             wtd_id = Convert.ToInt32(sdr["wtd_id"]),
        //                             wtd_botname = sdr["wtd_botname"].ToString(),
        //                             wtd_code = sdr["wtd_code"].ToString(),
        //                             wtd_username = sdr["wtd_username"].ToString(),
        //                             wtd_bank = sdr["wtd_bank"].ToString(),
        //                             wtd_bankacc = sdr["wtd_bankacc"].ToString(),
        //                             wtd_amount = sdr["wtd_amount"].ToString(),
        //                             wtd_status = sdr["wtd_status"].ToString(),
        //                             wtd_ctime = sdr["wtd_ctime"].ToString(),
        //                             wtd_bottime = sdr["wtd_bottime"].ToString(),
        //                             wtd_memberid = sdr["wtd_memberid"].ToString(),
        //                             wtd_status_desc = sdr["wtd_status_desc"].ToString()

                                
        //                         });
        //                     }
        //                 }
        //                 con.Close();
        //             }
        //         }
        
        //         return View(lists);
        //     }
        //     else
        //     {
        //         return RedirectToAction("Signin", "Account");
        //     }
            
        // }

        //  public IActionResult WD_list_before()
        // {
             
        //     if (checkSessionUser()){
        
        //         List<WithdrawModel> lists = new List<WithdrawModel>();
        
        //         string constr = this.Configuration.GetConnectionString("localString");
        //         using (MySqlConnection con = new MySqlConnection(constr))
        //         {
                    
        //             string query = "CALL GET_TRANS_WITHDRAW(7)";
        //             using (MySqlCommand cmd = new MySqlCommand(query))
        //             {
        //                 cmd.Connection = con;
        //                 con.Open();
        //                 using (MySqlDataReader sdr = cmd.ExecuteReader())
        //                 {
        //                     while (sdr.Read())
        //                     {
        //                         lists.Add(new WithdrawModel
        //                         {
        //                             //data1 = Convert.ToInt32(sdr["sys_users_username"]),
        //                             wtd_id = Convert.ToInt32(sdr["wtd_id"]),
        //                             wtd_botname = sdr["wtd_botname"].ToString(),
        //                             wtd_code = sdr["wtd_code"].ToString(),
        //                             wtd_username = sdr["wtd_username"].ToString(),
        //                             wtd_bank = sdr["wtd_bank"].ToString(),
        //                             wtd_bankacc = sdr["wtd_bankacc"].ToString(),
        //                             wtd_amount = sdr["wtd_amount"].ToString(),
        //                             wtd_status = sdr["wtd_status"].ToString(),
        //                             wtd_ctime = sdr["wtd_ctime"].ToString(),
        //                             wtd_bottime = sdr["wtd_bottime"].ToString(),
        //                             wtd_memberid = sdr["wtd_memberid"].ToString(),
        //                             wtd_status_desc = sdr["wtd_status_desc"].ToString()

                                
        //                         });
        //                     }
        //                 }
        //                 con.Close();
        //             }
        //         }
        
        //         return View(lists);
        //     }
        //     else
        //     {
        //         return RedirectToAction("Signin", "Account");
        //     }
            
        // }
        public IActionResult WD_list_after()
        {
             
            if (checkSessionUser()){ 
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
            
        }

         public IActionResult WD_list_before()
        {
             
           if (checkSessionUser()){ 
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
            
        }
        
        public IActionResult WD_Dashboard()
        {
            
             
            if (checkSessionUser()){
        
                
        
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
            
        }

       
        [Route("Confirm_Action")]
        [HttpPost]
        public IActionResult Confirm_Action(int wtd_id,int action_id){

            int iCode = -1;
            string sMessage = "ไม่สามารถดำเนินการได้";
            //var con;
            try{
                if (checkSessionUser()){
                    string username = "Test";//HttpContext.session.GetString("admin_username");
                    string constr = this.Configuration.GetConnectionString("localString");
                    // using (MySqlConnection con = new MySqlConnection(constr))
                    // {
                    //     string query = "CALL ACTION_CONFIRM_WITHDRAW("+wtd_id+",'1')";
                    //     using (MySqlCommand cmd = new MySqlCommand(query))
                    //     {
                    //         cmd.Connection = con;
                    //         con.Open();
                    //         cmd.ExecuteNonQuery();
                    //         con.Close();

                            
                    //         iCode = 1;
                    //         sMessage = "Successfully";

                    //         //return RedirectToAction("Index", "Withdraw");
                    //     }

                       
                    // }

                     using ( var con = new MySqlConnection(constr))
                        {
                            con.Open();
                            using (var cmd = new MySqlCommand("ACTION_CONFIRM_WITHDRAW", con) { CommandType = CommandType.StoredProcedure })
                            {
                                cmd.Parameters.AddWithValue("vID", wtd_id).MySqlDbType = MySqlDbType.Int32;
                                cmd.Parameters.AddWithValue("vAction", action_id).MySqlDbType = MySqlDbType.Int32;
                                cmd.Parameters.AddWithValue("vUserName", username).MySqlDbType = MySqlDbType.VarChar;
                                
                                cmd.Parameters.Add("vResultStatusID", MySqlDbType.Int32).Direction = ParameterDirection.Output;
                                cmd.Parameters.Add("vResultMessage", MySqlDbType.VarChar).Direction = ParameterDirection.Output;
                                cmd.ExecuteNonQuery();

                                //console.writeln("SUCCESS");
                                iCode = (int)cmd.Parameters["vResultStatusID"].Value;
                                sMessage = (string)cmd.Parameters["vResultMessage"].Value;
                                //object o = command.ExecuteScalar();
                               // var returnVal = cmd.ExecuteNonQuery();
                            }
                            con.Close();
                        }
                }else{
                    iCode = 0;
                    sMessage = "Access Deny.";
                    //return View();
                }
            }catch(Exception ex){
                Console.WriteLine("Withdraw Controller Error : "+ex.ToString());
            }finally{
                
            }

            var jsondata =  Newtonsoft.Json.JsonConvert.SerializeObject(new {ResultCode = iCode,ResultMsg = sMessage});
            
            return Json(jsondata, new JsonSerializerOptions{ WriteIndented = true, });
            
           
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
