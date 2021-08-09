using v29.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace v29.Helpers
{
    public static class BanHelper
    {

        public static string GetIP()
        {
            string ip = Startup.fHttpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            return ip;
        }

        public static string checkBan()
        {
            var ip = GetIP();
            DateTime now = DateTime.Now;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $"SELECT SQL_CALC_FOUND_ROWS DATE_FORMAT(sys_ban_until, '%d/%m/%Y %H:%i:%s') AS sys_ban_until,sys_ban_prefix,sys_ban_val FROM systems_banip WHERE sys_ban_ip = '{ip}' AND sys_ban_cnt >= {Startup.Configuration["MaxBanCount"]} AND sys_ban_until > @sys_ban_until LIMIT 1";
                cmd.Parameters.AddWithValue("@sys_ban_until", now.ToString("yyyy-MM-dd HH:mm:ss"));
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = "SELECT FOUND_ROWS()";
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                cmd.CommandText = $"UPDATE systems_banip SET sys_ban_cnt=1 WHERE sys_ban_cnt >= {Startup.Configuration["MaxBanCount"]} AND sys_ban_until < @sys_ban_until";
                cmd.ExecuteNonQuery();
                conn.Close();
                if (resCnt == 0)
                {
                    return "";
                }
                else
                {
                    return "IP ถูกระงับถึงเวลา " + dataTable.Rows[0]["sys_ban_until"].ToString() + "<br/> สาเหตุ " + dataTable.Rows[0]["sys_ban_val"].ToString();
                }
            }
            catch (MySqlException ex)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                return ex.Message;
            }
        }

        public static string coreGetBanRemains()
        {
            try
            {
                var ip = GetIP();
                string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
                MySqlConnection conn = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("", conn);
                conn.Open();
                cmd.CommandText = $"SELECT SQL_CALC_FOUND_ROWS * FROM systems_banip WHERE sys_ban_ip = '{ip}' LIMIT 1";
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = "SELECT FOUND_ROWS()";
                var resCnt1 = int.Parse(cmd.ExecuteScalar().ToString());
                if (resCnt1 < 1)
                {
                    return "Error[banHelper coreGetBanRemains] : IP Not Found";
                }
                else
                {
                    var intX1 = int.Parse(Startup.Configuration["MaxBanCount"]);
                    var intX2 = int.Parse(dataTable.Rows[0]["sys_ban_cnt"].ToString());
                    var remain = (intX1 - intX2 + 1).ToString();
                    return remain;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string coreSetBan(string reason = "", bool critical = false, bool clearban = false)
        {
            Random rnd = new Random();
            DateTime now = DateTime.Now;
            var prefix = now.ToString("yyyyMMddHHmmss") + "" + rnd.Next(100000, 999999);
            var ip = GetIP();
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = $"SELECT COUNT(sys_ban_ip) FROM systems_banip WHERE sys_ban_ip = '{ip}' LIMIT 1";
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                if (resCnt == 0)
                {
                    cmd.CommandText = $@"INSERT INTO systems_banip VALUES (@sys_ban_ip, 1, NOW(), @sys_ban_prefix, '')";
                    cmd.Parameters.AddWithValue("@sys_ban_ip", ip.TrimStart().TrimEnd());
                    cmd.Parameters.AddWithValue("@sys_ban_prefix", prefix.TrimStart().TrimEnd());
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = $"SELECT * FROM systems_banip WHERE sys_ban_ip = '{ip}' LIMIT 1";
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                cmd.CommandText = $@"INSERT INTO systems_logs VALUES (@sys_logs_prefix,@sys_logs_type,@sys_logs_location,@sys_logs_val1,@sys_logs_val2,@sys_logs_val3,NOW())";
                cmd.Parameters.AddWithValue("@sys_logs_prefix", prefix.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_logs_type", 0);
                cmd.Parameters.AddWithValue("@sys_logs_location", 0);
                cmd.Parameters.AddWithValue("@sys_logs_val1", reason.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_logs_val2", Startup.fHttpContextAccessor.HttpContext.Request.Path.ToString().TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_logs_val3", "[IP]" + ip.TrimStart().TrimEnd());
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                var bancount = int.Parse(dataTable.Rows[0]["sys_ban_cnt"].ToString()) + 1;
                var banuntil = now.AddMinutes(15);
                if (critical)
                {
                    banuntil = now.AddMinutes(150);
                    bancount = 5;
                }
                if (clearban)
                {
                    cmd.CommandText = $"UPDATE systems_banip SET sys_ban_cnt=1 WHERE sys_ban_ip = @sys_ban_ip";
                    cmd.Parameters.AddWithValue("@sys_ban_ip", ip.TrimStart().TrimEnd());
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    UserHelper.setUserLogs($"ปลดแบน IP [{ip.TrimStart().TrimEnd()}]", "99");
                }
                else
                { // NOTE ถ้าไม่ใช่ Clear Ban
                    cmd.CommandText = $@"UPDATE systems_banip SET sys_ban_cnt=@sys_ban_cnt,sys_ban_until = @sys_ban_until, sys_ban_prefix=@sys_ban_prefix,sys_ban_val = @sys_ban_val WHERE sys_ban_ip = @sys_ban_ip LIMIT 1";
                    cmd.Parameters.AddWithValue("@sys_ban_ip", ip.TrimStart().TrimEnd());
                    cmd.Parameters.AddWithValue("@sys_ban_cnt", bancount.ToString().TrimStart().TrimEnd());
                    cmd.Parameters.AddWithValue("@sys_ban_until", banuntil.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@sys_ban_prefix", prefix.TrimStart().TrimEnd());
                    cmd.Parameters.AddWithValue("@sys_ban_val", reason.TrimStart().TrimEnd());
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                return "";
            }
            catch (MySqlException ex)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                return ex.Message;
            }
        }

        public static string GetBanList(string draw, string start, string length, string search = "", string orderby = "0", string ordertype = "asc")
        {
            string whereStr = "";
            string orderbyStr = "";
            if (search != "")
            {
                whereStr = $" WHERE sys_ban_ip LIKE('%{search}%') OR sys_logs_val1 LIKE('%{search}%')";
            }
            switch (orderby)
            {
                case "0":
                    orderbyStr = "sys_ban_ip";
                    break;
                case "1":
                    orderbyStr = "sys_logs_val1";
                    break;
                case "2":
                    orderbyStr = "sys_ban_cnt";
                    break;
                default:
                    orderbyStr = "sys_ban_ip";
                    break;
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            var query = $@"SELECT SQL_CALC_FOUND_ROWS sys_ban_ip,sys_ban_cnt,DATE_FORMAT(sys_ban_until,'%Y/%m/%d %H:%i:%S') AS sys_ban_until,sys_logs_val1
            FROM systems_banip LEFT JOIN systems_logs ON sys_ban_prefix=sys_logs_prefix{whereStr} ORDER BY {orderbyStr} {ordertype} LIMIT {start},{length}";
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
                Dictionary<string, dynamic> outdate = new Dictionary<string, dynamic>();
                outdate.Add("draw", draw);
                outdate.Add("recordsFiltered", resCnt);
                outdate.Add("recordsTotal", resCnt);
                outdate.Add("data", dataTable);
                return JsonConvert.SerializeObject(outdate);
            }
            catch (MySqlException ex)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                return ex.Message;
            }
        }

        public static RespondModel UpdateBanState(string id)
        {
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                RespondModel restext = new RespondModel();
                cmd.CommandText = $@"UPDATE systems_banip SET sys_ban_cnt = 1 WHERE sys_ban_ip = @sys_ban_ip LIMIT 1";
                cmd.Parameters.AddWithValue("@sys_ban_ip", id.TrimStart().TrimEnd());
                cmd.ExecuteNonQuery();
                UserHelper.setUserLogs("ปลดแบน IP " + id.TrimStart().TrimEnd());
                restext.res_status = 200;
                restext.res_errortxt = "ok";
                conn.Close();
                return restext;
            }
            catch (MySqlException ex)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                RespondModel restext = new RespondModel();
                restext.res_status = 500;
                restext.res_errortxt = ex.Message;
                return restext;
            }
        }
        public static string GetBanLogList(string draw, string start, string length, string search = "", string orderby = "0", string ordertype = "asc")
        {
            string whereStr = "";
            string orderbyStr = "";
            switch (orderby)
            {
                case "0":
                    orderbyStr = "sys_logs_ctime";
                    break;
                case "1":
                    orderbyStr = "sys_logs_val1";
                    break;
                case "2":
                    orderbyStr = "sys_logs_val3";
                    break;
                default:
                    orderbyStr = "sys_logs_ctime";
                    break;
            }
            if (search != "")
            {
                whereStr = $" WHERE sys_logs_ctime LIKE('%{search}%') OR sys_logs_val1 LIKE('%{search}%') OR sys_logs_val3 LIKE('%{search}%')";
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            var query = $@"SELECT SQL_CALC_FOUND_ROWS DATE_FORMAT(sys_logs_ctime,'%Y/%m/%d %H:%i:%S') AS sys_logs_ctime,sys_logs_val1,sys_logs_val3
            FROM systems_logs{whereStr} ORDER BY {orderbyStr} {ordertype} LIMIT {start},{length}";
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
                Dictionary<string, dynamic> outdate = new Dictionary<string, dynamic>();
                outdate.Add("draw", draw);
                outdate.Add("recordsFiltered", resCnt);
                outdate.Add("recordsTotal", resCnt);
                outdate.Add("data", dataTable);
                return JsonConvert.SerializeObject(outdate);
            }
            catch (MySqlException ex)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                return ex.Message;
            }
        }
        public static RespondModel ClearBanLogs(string cleanup)
        {
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            RespondModel restext = new RespondModel();
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime cleanday = DateTime.Now.AddDays(-1);
            var msg = "1 วัน";
            switch (cleanup)
            {
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
                cmd.CommandText = $@"DELETE FROM systems_logs WHERE sys_logs_ctime <= @sys_logs_ctime";
                cmd.Parameters.AddWithValue("@sys_logs_ctime", cleanday.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                UserHelper.setUserLogs($"ล้างประวัติการเข้าใช้เหลือ {msg}", "99");
                restext.res_status = 200;
                restext.res_errortxt = "ok";
                conn.Close();
                return restext;
            }
            catch (MySqlException ex)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                restext.res_status = 500;
                restext.res_errortxt = ex.Message;
                return restext;
            }
        }
    }

}