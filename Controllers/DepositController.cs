using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using v29.Models;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using v29.Helpers;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace v29.Controllers
{
    public class DepositController : Controller
    {
       AccountController accountController = new AccountController();

        private IConfiguration Configuration;
 
        public DepositController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        private bool checkSessionUser(){
            
            return UserHelper.CheckSignin();
        }

        public IActionResult Index()
        {
             
            if (checkSessionUser()){
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
        }

        public IActionResult Dep_Dashboard()
        {
                
            if (checkSessionUser()){ 
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
            
        }

        public IActionResult Dep_Transaction()
        {
                
            if (checkSessionUser()){ 
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
            
        }
   
        public IActionResult Dep_Transaction_Bank()
        {
            if (checkSessionUser()){
                return View();
            }
            else
            {
                return RedirectToAction("Signin", "Account");
            }
            
        }

        
        ////// #################  API  ######################///////////
       
        [Route("/api/Get_Dep_Transaction_List")]
        [HttpGet]
        public IActionResult Get_Dep_Transaction_List(int action_id){
            
            DataTable dtResult = new DataTable("table");
            string constr = this.Configuration.GetConnectionString("localString");
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                
                string query = "CALL GET_TRANS_DEPOSIT("+ action_id+")";
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

         [Route("/api/Get_Dep_Transaction_Bank_List")]
        [HttpGet]
        public IActionResult Get_Dep_Transaction_Bank_List(int action_id){
            
            DataTable dtResult = new DataTable("table");
            string constr = this.Configuration.GetConnectionString("localString");
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                
                string query = "CALL GET_TRANS_DEPOSIT_BANK("+ action_id+")";
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
