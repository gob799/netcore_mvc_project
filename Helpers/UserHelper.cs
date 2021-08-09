using System;
using System.Data;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json.Linq;
using v29.Models;
using System.Collections.Generic;

namespace v29.Helpers
{
    public static class UserHelper {
        public static bool CheckSignin()
        {
            var session = Startup.fHttpContextAccessor.HttpContext.Session;
            if(Startup.Configuration["DevState"] == "true"){
                System.Console.WriteLine("DevState");
                TakeSignin("admin","admin");
                return true;
            }
            if(session.GetString("admin_username") == null || session.GetString("admin_username").Length < 1 || session.GetString("admin_username") == "" ||
            session.GetString("admin_password") == null || session.GetString("admin_password").Length < 1 || session.GetString("admin_password") == ""){
                return false;
            } else {
                if(TakeSignin(session.GetString("admin_username"),session.GetString("admin_password")) == "ok"){
                    return true;
                } else {
                    return false;
                }
            }
        }
        public static string TakeSignin(string vusername, string vpassword)
        {
            var timemow = DateTime.Now.ToString("yyyyMMddHHmmss");
            var hash = EncryptHelper.Base64Decode("ZGdlbg==");
            var subhash = EncryptHelper.Base64Decode("QA==");
            var md5pass = EncryptHelper.CreateMD5(vpassword);
            var msg = "";
            var checkBaned = BanHelper.checkBan();
            if(checkBaned.Length > 0){
                return checkBaned;
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            if(!String.IsNullOrEmpty(Startup.fHttpContextAccessor.HttpContext.Session.GetString("tokens"))){
                var session = Startup.fHttpContextAccessor.HttpContext.Session;
                var tokens = session.GetString("tokens");
                if(tokens.Length > 40){
                        var enc1 = tokens.Substring(0,32);
                        var enc2 = tokens.Substring(32,14);
                        int endIndex = tokens.Length - 6;
                        var enc3 = tokens.Substring(endIndex);
                        var checker1 = EncryptHelper.CreateMD5(session.GetString("admin_username")+session.GetString("admin_password")+enc2);
                        var checker2 = EncryptHelper.CreateMD5(checker1+enc2);
                        if(enc1 == checker1 && enc3 == checker2.Substring(checker2.Length-6) && long.Parse(timemow) <= long.Parse(enc2)){
                            session.SetString("admin_username",session.GetString("admin_username"));
                            session.SetString("admin_password",session.GetString("admin_password"));
                            session.SetString("tokens",createTokens());
                            Startup.userd = GetUserD();
                            return "ok";
                        }
                }
            }
            try
            {
                MySqlConnection conn = new MySqlConnection(constr);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                //หา Username
                cmd.CommandText = $"SELECT COUNT(sys_users_id) FROM systems_users WHERE sys_users_username = '{vusername}'";
                if(int.Parse(cmd.ExecuteScalar().ToString()) < 1){
                    BanHelper.coreSetBan("ไม่พบชื่อผู้ใช้ " + vusername + "");
                    msg += "ไม่พบชื่อผู้ใช้";
                    msg += "<br>IP : "+ BanHelper.GetIP() +" คงเหลือ " + BanHelper.coreGetBanRemains() + " ครั้งก่อนถูกระงับ";
                    return  msg;
                }
                //หา Username + passowrd
                cmd.CommandText = $"SELECT SQL_CALC_FOUND_ROWS * FROM systems_users WHERE sys_users_username = '{vusername}' AND sys_users_password = '{md5pass}'";
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = "SELECT FOUND_ROWS()";
                if(int.Parse(cmd.ExecuteScalar().ToString()) < 1){
                    BanHelper.coreSetBan("รหัสผ่านไม่ถูกต้อง " + vusername + "");
                    msg += "รหัสผ่านไม่ถูกต้อง";
                    msg += "<br>IP : "+ BanHelper.GetIP() +" คงเหลือ " + BanHelper.coreGetBanRemains() + " ครั้งก่อนถูกระงับ";
                    return  msg;
                }
                //หา Username + passowrd + ถูกรระงับ + Admin
                if(int.Parse(dataTable.Rows[0]["sys_users_accesslevel"].ToString()) >= 250){ //Admin
                    var hassha = EncryptHelper.CreateMD5(dataTable.Rows[0]["sys_users_username"].ToString() + hash + dataTable.Rows[0]["sys_users_accesslevel"].ToString() +subhash);
                    cmd.CommandText = $"SELECT COUNT(sys_users_id) FROM systems_users WHERE sys_users_username = '{vusername}' AND sys_users_password = '{md5pass}' AND sys_users_accesshash = '{hassha}'";
                    if(int.Parse(cmd.ExecuteScalar().ToString()) < 1){
                        BanHelper.coreSetBan("มีการกำหนดสิทธิไม่ถูกต้อง " + vusername + "",true);
                        msg += "มีการกำหนดสิทธิไม่ถูกต้อง";
                        msg += "<br>IP : "+ BanHelper.GetIP() +" คงเหลือ " + BanHelper.coreGetBanRemains() + " ครั้งก่อนถูกระงับ";
                        return  msg;
                    } else {
                        cmd.CommandText = $"UPDATE systems_users SET sys_users_lastseen = NOW() WHERE sys_users_username = '{vusername}' LIMIT 1";
                        cmd.ExecuteNonQuery();
                        Startup.fHttpContextAccessor.HttpContext.Session.SetString("admin_username",vusername);
                        Startup.fHttpContextAccessor.HttpContext.Session.SetString("admin_password",vpassword);
                        Startup.fHttpContextAccessor.HttpContext.Session.SetString("tokens",createTokens());
                        Startup.userd = GetUserD();
                        setUserLogs("เข้าสู่ระบบ");
                        return "ok";
                    }
                } else {
                    cmd.CommandText = $"SELECT COUNT(sys_users_id) FROM systems_users WHERE sys_users_username = '{vusername}' AND sys_users_password = '{md5pass}' AND sys_users_disabled = 0";
                    if(int.Parse(cmd.ExecuteScalar().ToString()) < 1){
                        BanHelper.coreSetBan("ชื่อผู้ใช้ถูกระงับการใช้งาน " + vusername + "");
                        msg += "ชื่อผู้ใช้ถูกระงับการใช้งาน";
                        msg += "<br>IP : "+ BanHelper.GetIP() +" คงเหลือ " + BanHelper.coreGetBanRemains() + " ครั้งก่อนถูกระงับ";
                        return  msg;
                    }
                    cmd.CommandText = $"SELECT COUNT(sys_groups_id) FROM systems_users_groups WHERE sys_groups_id = '{dataTable.Rows[0]["sys_users_group"].ToString()}' AND sys_groups_disabled = 0";
                    if(int.Parse(cmd.ExecuteScalar().ToString()) < 1){
                        BanHelper.coreSetBan("สิทธการใช้งานถูกยกเลิก " + vusername + "");
                        msg += "สิทธการใช้งานถูกยกเลิก";
                        msg += "<br>IP : "+ BanHelper.GetIP() +" คงเหลือ " + BanHelper.coreGetBanRemains() + " ครั้งก่อนถูกระงับ";
                        return  msg;
                    }
                    cmd.CommandText = $"UPDATE systems_users SET sys_users_lastseen = NOW() WHERE sys_users_username = '{vusername}' LIMIT 1";
                    cmd.ExecuteNonQuery();
                    Startup.fHttpContextAccessor.HttpContext.Session.SetString("admin_username",vusername);
                    Startup.fHttpContextAccessor.HttpContext.Session.SetString("admin_password",vpassword);
                    Startup.fHttpContextAccessor.HttpContext.Session.SetString("tokens",createTokens());
                    Startup.userd = GetUserD();
                    setUserLogs("เข้าสู่ระบบ");
                    return "ok";
                }
            }
            catch (System.Exception ex)
            {
                return ex.ToString();
            }
        }
        private static string createTokens(){
            var session = Startup.fHttpContextAccessor.HttpContext.Session;
            var timeadded = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");
            var md5out = EncryptHelper.CreateMD5(session.GetString("admin_username")+session.GetString("admin_password")+timeadded)+timeadded;
            var enc = EncryptHelper.CreateMD5(md5out);
            enc = enc.Substring(enc.Length-6);
            return (md5out+enc).ToUpper();
        }
        public static dynamic GetUserD(int uid=0)
        {
                dynamic detail = null;
                dynamic detail1 = null;
                dynamic detail2 = null;
                string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
                MySqlConnection conn = new MySqlConnection(constr);
                if(uid == 0){
                    var md5pass = EncryptHelper.CreateMD5(Startup.fHttpContextAccessor.HttpContext.Session.GetString("admin_password"));
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("", conn);
                        cmd.CommandText = $@"SELECT * FROM systems_users WHERE sys_users_username = '{Startup.fHttpContextAccessor.HttpContext.Session.GetString("admin_username")}' And sys_users_password = '{md5pass}'";
                        MySqlDataReader dataReader = cmd.ExecuteReader();
                        var dataTable = new DataTable();
                        dataTable.Load(dataReader);
                        var getdetail = JsonConvert.SerializeObject(dataTable);
                        detail = JsonConvert.DeserializeObject<dynamic>(getdetail)[0];
                        dataTable = new DataTable();
                        cmd.CommandText = $@"SELECT * FROM systems_users_detail WHERE sys_usersd_id = '{detail["sys_users_id"]}'";
                        dataReader = cmd.ExecuteReader();
                        dataTable.Load(dataReader);
                        var getdetail1 = JsonConvert.SerializeObject(dataTable);
                        detail1 = JsonConvert.DeserializeObject<dynamic>(getdetail1)[0];
                        dataTable = new DataTable();
                        if(detail["sys_users_group"].ToString() == "0" && detail["sys_users_accesslevel"].ToString() == "250"){
                            dataReader.Close();
                            var str250 = @"
                                [{""sys_groups_id"":""0"",""sys_groups_title"":""Root Administrator"",""sys_groups_subtitle"":null,
                                ""sys_groups_perms"":"""",""sys_groups_accesslevel"":""250"",""sys_groups_disabled"":""0"",""sys_groups_cby"":""Root"",
                                ""sys_groups_ctime"":""2000-01-01 00:00:00"",""sys_groups_uby"":""Root"",""sys_groups_utime"":""2000-01-01 00:00:00""}]
                            ";
                            var ugetDetai2 = JsonConvert.DeserializeObject<dynamic>(str250);
                            detail2 = ugetDetai2[0];
                        } else {
                            cmd.CommandText = $@"SELECT * FROM systems_users_groups WHERE sys_groups_id = '{detail["sys_users_group"]}'";
                            dataReader = cmd.ExecuteReader();
                            dataTable.Load(dataReader);
                            var getdetail2 = JsonConvert.SerializeObject(dataTable);
                            detail2 = JsonConvert.DeserializeObject<dynamic>(getdetail2)[0];
                        }
                    } catch (Exception ex){
                        if(conn != null){
                            conn.Close();
                        }
                        return ex.Message;
                    }
                } else {
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("", conn);
                        cmd.CommandText = $@"SELECT * FROM systems_users WHERE sys_users_id = '{uid}'";
                        MySqlDataReader dataReader = cmd.ExecuteReader();
                        var dataTable = new DataTable();
                        dataTable.Load(dataReader);
                        var getdetail = JsonConvert.SerializeObject(dataTable);
                        detail = JsonConvert.DeserializeObject<dynamic>(getdetail)[0];
                        dataTable = new DataTable();
                        cmd.CommandText = $@"SELECT * FROM systems_users_detail WHERE sys_usersd_id = '{detail["sys_users_id"]}'";
                        dataReader = cmd.ExecuteReader();
                        dataTable.Load(dataReader);
                        var getdetail1 = JsonConvert.SerializeObject(dataTable);
                        detail1 = JsonConvert.DeserializeObject<dynamic>(getdetail1)[0];
                        dataTable = new DataTable();
                        if(detail["sys_users_group"].ToString() == "0" && detail["sys_users_accesslevel"].ToString() == "250"){
                            dataReader.Close();
                            var str250 = @"
                                [{""sys_groups_id"":""0"",""sys_groups_title"":""Root Administrator"",""sys_groups_subtitle"":null,
                                ""sys_groups_perms"":"""",""sys_groups_accesslevel"":""250"",""sys_groups_disabled"":""0"",""sys_groups_cby"":""Root"",
                                ""sys_groups_ctime"":""2000-01-01 00:00:00"",""sys_groups_uby"":""Root"",""sys_groups_utime"":""2000-01-01 00:00:00""}]
                            ";
                            var ugetDetai2 = JsonConvert.DeserializeObject<dynamic>(str250);
                            detail2 = ugetDetai2[0];
                        } else {
                            cmd.CommandText = $@"SELECT * FROM systems_users_groups WHERE sys_groups_id = '{detail["sys_users_group"]}'";
                            dataReader = cmd.ExecuteReader();
                            dataTable.Load(dataReader);
                            var getdetail2 = JsonConvert.SerializeObject(dataTable);
                            detail2 = JsonConvert.DeserializeObject<dynamic>(getdetail2)[0];
                        }
                    } catch (Exception ex){
                        if(conn != null){
                            conn.Close();
                        }
                        return ex.Message;
                    }
                }
                if(conn != null){
                    conn.Close();
                }
                var result = new JObject();
                result.Merge(detail);
                result.Merge(detail1);
                result.Merge(detail2);
                return result;
        }
        public static string setUserLogs(string reason="",string logsType="0"){
            Random rnd = new Random();
            var ip = BanHelper.GetIP();
            DateTime now = DateTime.Now;
            var prefix = now.ToString("yyyyMMddHHmmss")+""+rnd.Next(100000, 999999);
             string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try{
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $@"INSERT systems_users_logs VALUES(@ulogs_id, @ulogs_type, @ulogs_val, @ulogs_uid, @ulogs_by, @ulogs_ip, NOW())";
                cmd.Parameters.AddWithValue("@ulogs_id",prefix.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@ulogs_type",logsType);
                cmd.Parameters.AddWithValue("@ulogs_val",reason.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@ulogs_uid",Startup.userd["sys_users_id"]);
                cmd.Parameters.AddWithValue("@ulogs_by",Startup.userd["sys_users_username"]);
                cmd.Parameters.AddWithValue("@ulogs_ip",ip.TrimStart().TrimEnd());
                cmd.ExecuteNonQuery();
                return "";
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                return ex.Message;
            }
        }
        public static bool CheckPerm(string permid){
            if(int.Parse(Startup.userd["sys_users_accesslevel"].ToString()) >= 250){
                return true;
            } else {
                var permDataInnerget = Startup.userd["sys_groups_perms"].ToString().Split("|");
                List<string> permDataInner = new List<string>(permDataInnerget);
                if(permDataInner.Contains(permid)){
                    return true;
                } else {
                    return false;
                }
            }
        }
        public static RespondModel GetOne(string uid,string table,string pmkey){
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $"SELECT SQL_CALC_FOUND_ROWS * FROM {table} WHERE {pmkey}=@{pmkey} LIMIT 1";
                cmd.Parameters.AddWithValue("@"+pmkey,uid);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = "SELECT FOUND_ROWS()";
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();
                RespondModel resd = new RespondModel();
                if(resCnt > 0){
                    resd.res_status = 200;
                    resd.res_val = JsonConvert.SerializeObject(dataTable);
                    return resd;
                } else {
                    resd.res_status = 500;
                    resd.res_errortxt = "Not found";
                    return resd;
                }
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                RespondModel resd = new RespondModel();
                resd.res_status = 500;
                resd.res_errortxt = ex.Message;
                return resd;
            }
        }
        public static string UpdateOne(string uid,string table,string col,string val){
            var pmKey = "sys_users_id";
            if(table == "systems_users_detail"){
                pmKey = "sys_usersd_id";
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $"UPDATE {table} SET {col}=@{col} WHERE {pmKey}=@{pmKey} LIMIT 1";
                cmd.Parameters.AddWithValue("@"+pmKey,uid);
                cmd.Parameters.AddWithValue("@"+col,val);
                cmd.ExecuteNonQuery();
                conn.Close();
                return "ok";
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                return ex.Message;
            }
        }
        public static string GetUserLogList(string draw,string start,string length,string search = "",string orderby="0",string ordertype="asc"){
            string whereStr = "";
            string orderbyStr = "";
            switch (orderby)
            {
                case "0":
                    orderbyStr = "ulogs_time";
                    break;
                case "1":
                    orderbyStr = "ulogs_val";
                    break;
                case "2":
                    orderbyStr = "ulogs_uid";
                    break;
                case "3":
                    orderbyStr = "ulogs_ip";
                    break;
                default:
                    orderbyStr = "ulogs_time";
                    break;
            }
            if(search != ""){
                whereStr = $" WHERE ulogs_val LIKE('%{search}%') OR ulogs_by LIKE('%{search}%') OR ulogs_uid LIKE('%{search}%') OR ulogs_ip LIKE('%{search}%')";
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            var query = $@"SELECT SQL_CALC_FOUND_ROWS DATE_FORMAT(ulogs_time,'%Y/%m/%d %H:%i:%S') AS ulogs_time,CONCAT('[',ulogs_uid,'] ',ulogs_by,' ',ulogs_val) AS ulogs_val,CONCAT('[',ulogs_uid,'] ',ulogs_by) AS ulogs_bytext,ulogs_ip
            FROM systems_users_logs{whereStr} ORDER BY {orderbyStr} {ordertype} LIMIT {start},{length}";
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = "SELECT FOUND_ROWS()";
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();
                Dictionary<string,dynamic> outdate = new Dictionary<string, dynamic>();
                outdate.Add("draw",draw);
                outdate.Add("recordsFiltered",resCnt);
                outdate.Add("recordsTotal",resCnt);
                outdate.Add("data",dataTable);
                return JsonConvert.SerializeObject(outdate);
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                return ex.Message;
            }
        }
        public static RespondModel ClearUserLogs(string cleanup){
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            RespondModel restext = new RespondModel();
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime cleanday = DateTime.Now.AddDays(-1);
            var msg = "1 วัน";
            switch (cleanup)
            {
                case "0":
                    cleanday = DateTime.Now;
                    msg = "0 วัน";
                    break;
                case "7":
                    cleanday = DateTime.Now.AddDays(-7);
                    msg = "7 วัน";
                    break;
                case "30":
                    cleanday = DateTime.Now.AddMonths(-1);
                    msg = "1 เดือน";
                    break;
                default:
                    cleanday = DateTime.Now.AddDays(-1);
                    break;
            }
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $@"DELETE FROM systems_users_logs WHERE ulogs_time <= @ulogs_time AND ulogs_type != 99";
                cmd.Parameters.AddWithValue("@ulogs_time",cleanday.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                UserHelper.setUserLogs($"ล้างประวัติผู้ใช้ทำรายการเหลือ {msg}","99");
                restext.res_status = 200;
                restext.res_errortxt = "ok";
                conn.Close();
                return restext;
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                restext.res_status = 500;
                restext.res_errortxt = ex.Message;
                return restext;
            }
        }
        public static RespondModel Insert(string username,string password,string group,string fname,string lname,string dob,string gender){
            var hash = EncryptHelper.Base64Decode("ZGdlbg==");
            var subhash = EncryptHelper.Base64Decode("QA==");
            var md5pass = EncryptHelper.CreateMD5(password);
            var hassha = EncryptHelper.CreateMD5(username.ToString() + hash + "100" +subhash);
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            RespondModel restext = new RespondModel();
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $@"SELECT COUNT(sys_users_id) FROM systems_users WHERE sys_users_username = @sys_users_username";
                cmd.Parameters.AddWithValue("@sys_users_username",username.TrimStart().TrimEnd());
                var res1 = int.Parse(cmd.ExecuteScalar().ToString());
                if(res1 > 0){
                    conn.Close();
                    restext.res_status = 500;
                    restext.res_errortxt = "พบผู้ใช้นี้แล้ว";
                    return restext;
                }
                cmd.Parameters.Clear();

                cmd.CommandText = $@"INSERT INTO systems_users (sys_users_id, sys_users_username, sys_users_password, sys_users_tokens, sys_users_tel, sys_users_group, sys_users_accesslevel, sys_users_accesshash, sys_users_lang, sys_users_settings, sys_users_disabled, sys_users_cby, sys_users_ctime, sys_users_uby, sys_users_utime, sys_users_lastseen) VALUES
                (NULL, @sys_users_username, @sys_users_password, NULL, '', @sys_users_group, '100', @sys_users_accesshash, 'th', NULL, '0', @sys_users_cby, NOW(), @sys_users_uby, NOW(), NULL);";
                cmd.Parameters.AddWithValue("@sys_users_username",username.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_users_password",md5pass.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_users_group",group.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_users_accesshash",hassha.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_users_cby",userd["sys_users_username"].ToString().TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_users_uby",userd["sys_users_username"].ToString().TrimEnd());
                cmd.ExecuteNonQuery();
                long imageId = cmd.LastInsertedId;
                var resetyear = long.Parse(dob.Replace("-",""));
                if(resetyear > 24009999L){
                    resetyear-=5430000L;
                }
                var reset_year = resetyear.ToString().Substring(0,4);
                var reset_month = resetyear.ToString().Substring(4,2);
                var reset_day = resetyear.ToString().Substring(resetyear.ToString().Length-2);
                var resetyearv = reset_year+"-"+reset_month+"-"+reset_day;
                cmd.CommandText = $"INSERT INTO systems_users_detail (sys_usersd_id, sys_usersd_fname, sys_usersd_lname, sys_usersd_avatar, sys_usersd_gender, sys_usersd_birth) VALUES ({imageId.ToString()}, @sys_usersd_fname, @sys_usersd_lname, 'noimage', @sys_usersd_gender, @sys_usersd_birth);";
                cmd.Parameters.AddWithValue("@sys_usersd_fname",fname.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_usersd_lname",lname.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_usersd_gender",gender.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_usersd_birth",resetyearv.TrimStart().TrimEnd());
                cmd.ExecuteNonQuery();
                UserHelper.setUserLogs($"เพิ่มผู้ใช้งาน Username : {username}");
                restext.res_status = 200;
                restext.res_errortxt = "ok";
                conn.Close();
                return restext;
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                restext.res_status = 500;
                restext.res_errortxt = ex.Message;
                return restext;
            }
    }
        public static RespondModel UpdateDetail(string uid,string fname,string lname,string gender,string birth,string password,string group){
            var resetyear = long.Parse(birth.Replace("-",""));
            if(resetyear > 24009999L){
                resetyear-=5430000L;
            }
            var reset_year = resetyear.ToString().Substring(0,4);
            var reset_month = resetyear.ToString().Substring(4,2);
            var reset_day = resetyear.ToString().Substring(resetyear.ToString().Length-2);
            var resetyearv = reset_year+"-"+reset_month+"-"+reset_day;
            System.Console.WriteLine(resetyearv);
            RespondModel restext = new RespondModel();
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $"UPDATE systems_users_detail SET sys_usersd_fname=@sys_usersd_fname,sys_usersd_lname=@sys_usersd_lname,sys_usersd_gender=@sys_usersd_gender,sys_usersd_birth=@sys_usersd_birth WHERE sys_usersd_id=@sys_usersd_id LIMIT 1";
                cmd.Parameters.AddWithValue("@sys_usersd_id",uid);
                cmd.Parameters.AddWithValue("@sys_usersd_fname",fname.Trim());
                cmd.Parameters.AddWithValue("@sys_usersd_lname",lname.Trim());
                cmd.Parameters.AddWithValue("@sys_usersd_gender",gender);
                cmd.Parameters.AddWithValue("@sys_usersd_birth",resetyearv);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.CommandText = $"UPDATE systems_users SET sys_users_utime=NOW(),sys_users_uby='{Startup.userd["sys_users_username"].ToString()}' WHERE sys_users_id=@sys_users_id LIMIT 1";
                cmd.Parameters.AddWithValue("@sys_users_id",uid);
                cmd.ExecuteNonQuery();
                if(password != ""){
                    var md5pass = EncryptHelper.CreateMD5(password);
                    cmd.Parameters.Clear();
                    cmd.CommandText = $"UPDATE systems_users SET sys_users_password=@sys_users_password,sys_users_utime=NOW(),sys_users_uby='{Startup.userd["sys_users_username"].ToString()}' WHERE sys_users_id=@sys_users_id LIMIT 1";
                    cmd.Parameters.AddWithValue("@sys_users_id",uid);
                    cmd.Parameters.AddWithValue("@sys_users_password",md5pass);
                    cmd.ExecuteNonQuery();
                }
                 if(group != ""){
                    cmd.Parameters.Clear();
                    cmd.CommandText = $"UPDATE systems_users SET sys_users_group=@sys_users_group,sys_users_utime=NOW(),sys_users_uby='{Startup.userd["sys_users_username"].ToString()}' WHERE sys_users_id=@sys_users_id LIMIT 1";
                    cmd.Parameters.AddWithValue("@sys_users_id",uid);
                    cmd.Parameters.AddWithValue("@sys_users_group",group);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                UserHelper.setUserLogs($"แก้ไขผู้ใช้งาน ID : {uid}");
                restext.res_status = 200;
                restext.res_val ="ok";
                return restext;
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                restext.res_status = 500;
                restext.res_errortxt = ex.Message;
                return restext;
            }
        }
        public static string GetList(string draw,string start,string length,string search = "",string orderby="0",string ordertype="asc"){
            string whereStr = "";
            string orderbyStr = "";
            switch (orderby)
            {
                case "0":
                    orderbyStr = "sys_users_id";
                    break;
                case "1":
                    orderbyStr = "sys_users_username";
                    break;
                case "2":
                    orderbyStr = "sys_users_fullname";
                    break;
                case "3":
                    orderbyStr = "sys_groups_title";
                    break;
                case "4":
                    orderbyStr = "sys_users_disabled";
                    break;
                default:
                    orderbyStr = "sys_users_id";
                    break;
            }
            if(search != ""){
                whereStr = $" WHERE sys_users_username LIKE('%{search}%') OR sys_usersd_fname LIKE('%{search}%') OR sys_usersd_lname LIKE('%{search}%') OR sys_groups_title LIKE('%{search}%')";
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            var query = $@"SELECT SQL_CALC_FOUND_ROWS sys_users_id,sys_users_username,CONCAT(sys_usersd_fname,' ',sys_usersd_lname) AS sys_users_fullname,sys_groups_title,sys_users_disabled,'' AS sys_groups_btn,sys_users_accesslevel
            FROM systems_users LEFT JOIN systems_users_detail ON sys_users_id=sys_usersd_id LEFT JOIN systems_users_groups ON sys_users_group=sys_groups_id{whereStr} ORDER BY {orderbyStr} {ordertype} LIMIT {start},{length}";
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = "SELECT FOUND_ROWS()";
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();
                Dictionary<string,dynamic> outdate = new Dictionary<string, dynamic>();
                outdate.Add("draw",draw);
                outdate.Add("recordsFiltered",resCnt);
                outdate.Add("recordsTotal",resCnt);
                outdate.Add("data",dataTable);
                return JsonConvert.SerializeObject(outdate);
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                return ex.Message;
            }
        }
        public static RespondModel UpdateState(string id){
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime now = DateTime.Now;
            try
            {
                var vState = "1";
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = "SELECT sys_users_disabled FROM systems_users WHERE sys_users_id = @sys_users_id";
                cmd.Parameters.AddWithValue("@sys_users_id",id.TrimStart().TrimEnd());
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                RespondModel restext = new RespondModel();
                if(resCnt.ToString() == "1"){
                    vState = "0";
                }
                cmd.Parameters.AddWithValue("@sys_users_disabled",vState.TrimStart().TrimEnd());
                cmd.CommandText = $@"UPDATE systems_users SET sys_users_disabled = @sys_users_disabled WHERE sys_users_id = @sys_users_id LIMIT 1";
                cmd.ExecuteNonQuery();
                restext.res_status = 200;
                restext.res_errortxt = "ok";
                conn.Close();
                if(vState == "1")
                    UserHelper.setUserLogs($"ยกเลิกการใช้งาน ID : {id}");
                if(vState == "0")
                    UserHelper.setUserLogs($"เปิดใช้งาน ID : {id}");
                return restext;
            }
            catch (MySqlException ex)
            {
                if(conn != null){
                     conn.Close();
                }
                RespondModel restext = new RespondModel();
                    restext.res_status = 500;
                    restext.res_errortxt = ex.Message;
                return restext;
            }
        }
    }
}