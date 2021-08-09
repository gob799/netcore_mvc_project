using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using v29.Models;

namespace v29.Helpers
{
    public static class PermissionsIHelp {
        private static PermissionModel createPerm(string pid,string headid,string inherid,string pname){
            PermissionModel pitem = new PermissionModel();
            pitem.pid = pid;
            pitem.headid = headid;
            pitem.inherid = inherid;
            pitem.pname = pname;
            return pitem;
        }
        public static List<PermissionModel> PermList(){
            List<PermissionModel> perm = new List<PermissionModel>();
            perm.Add(createPerm("1","0","0","ระบบหลัก"));
            perm.Add(createPerm("2","1","","เปิดดู IP ถูกระงับ"));
            perm.Add(createPerm("3","1","2","ปลดล๊อค IP ถูกระงับ"));
            perm.Add(createPerm("4","1","","เปิดดูประวัติการเข้าใช้"));
            perm.Add(createPerm("5","1","4","ล้างประวัติการเข้าใช้"));
            perm.Add(createPerm("6","1","","ดูประวัติการเข้าใช้"));
            perm.Add(createPerm("7","1","6","ล้างประวัติการเข้าใช้"));

            perm.Add(createPerm("10","0","0","สิทธิ์การใช้งานระบบ"));
            perm.Add(createPerm("11","10","","เปิดดู"));
            perm.Add(createPerm("12","10","11","เพิ่มสิทธิ์"));
            perm.Add(createPerm("13","10","11","แก้ไขสิทธิ์"));
            perm.Add(createPerm("14","10","11","เปิด-ปิดการใช้งาน"));
            perm.Add(createPerm("15","10","11","ลบสิทธิ์"));

            perm.Add(createPerm("100","0","0","ผู้ใช้งานระบบ"));
            perm.Add(createPerm("101","100","","เปิดดู"));
            perm.Add(createPerm("108","100","101","เพิ่มใหม่"));
            perm.Add(createPerm("102","100","101","แก้ไขข้อมูล"));
            perm.Add(createPerm("103","100","102","แก้ไขรหัสผ่าน"));
            perm.Add(createPerm("104","100","102","แก้ไขสิทธิ์ใช้งานระบบ"));
            perm.Add(createPerm("105","100","102","เปิด-ปิดการใช้งาน"));
            perm.Add(createPerm("106","100","101","ดูประวัติผู้ใช้ทำรายการ"));
            perm.Add(createPerm("107","100","106","ล้างประวัติผู้ใช้ทำรายการ"));

            perm.Add(createPerm("20","0","0","Main Menu"));
            perm.Add(createPerm("21","20","0","ระบบฝาก"));
            perm.Add(createPerm("22","20","0","ระบบถอน"));
            perm.Add(createPerm("23","20","0","ตั้งค่าระบบ"));

            
            
            return perm;
        }

         public static string GetList(string draw,string start,string length,string search = "",string orderby="0",string ordertype="asc"){
            string whereStr = "";
            string orderbyStr = "";
            if(search != ""){
                whereStr = $" WHERE sys_groups_id LIKE('%{search}%') OR sys_groups_title LIKE('%{search}%')";
            }
            switch (orderby)
            {
                case "0":
                    orderbyStr = "sys_groups_id";
                    break;
                case "1":
                    orderbyStr = "sys_groups_title";
                    break;
                case "2":
                    orderbyStr = "cntusers";
                    break;
                case "3":
                    orderbyStr = "sys_groups_disabled";
                    break;
                default:
                    orderbyStr = "sys_groups_utime";
                    break;
            }
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            var query = $@"SELECT SQL_CALC_FOUND_ROWS sys_groups_id,sys_groups_title,sys_groups_disabled,
            (SELECT COUNT(sys_users_id) FROM systems_users WHERE sys_users_group=sys_groups_id) AS cntusers,"""" AS sys_groups_btn
            FROM systems_users_groups{whereStr} ORDER BY {orderbyStr} {ordertype} LIMIT {start},{length}";
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

        public static RespondModel GetOne(string id){
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            var query = $"SELECT SQL_CALC_FOUND_ROWS * FROM systems_users_groups WHERE sys_groups_id='{id}'";
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
                RespondModel restext = new RespondModel();
                if(resCnt == 0){
                    restext.res_status = 500;
                    restext.res_errortxt = "Not Found";
                } else {
                    restext.res_status = 200;
                    restext.res_val = JsonConvert.SerializeObject(dataTable);
                }
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

        public static RespondModel InsertOne(string name,string perms){
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime now = DateTime.Now;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = "SELECT COUNT(sys_groups_id) FROM systems_users_groups WHERE sys_groups_title = @sys_groups_title";
                cmd.Parameters.AddWithValue("@sys_groups_title",name.TrimStart().TrimEnd());
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                RespondModel restext = new RespondModel();
                if(resCnt > 0){
                    restext.res_status = 500;
                    restext.res_errortxt = "พบชื่อนี้แล้ว";
                } else {
                    cmd.CommandText = $@"INSERT INTO systems_users_groups
                        (sys_groups_title, sys_groups_subtitle, sys_groups_perms, sys_groups_accesslevel, sys_groups_disabled, sys_groups_cby, sys_groups_ctime, sys_groups_uby, sys_groups_utime)
                    VALUES (@sys_groups_title, '', @sys_groups_perms, 100, 0, '{userd.sys_users_username}', '{now.ToString("yyyy-MM-dd HH:mm:ss")}', '{userd.sys_users_username}', '{now.ToString("yyyy-MM-dd HH:mm:ss")}')";
                    cmd.Parameters.AddWithValue("@sys_groups_perms",perms.TrimStart().TrimEnd());
                    cmd.ExecuteNonQuery();
                    restext.res_status = 200;
                    restext.res_errortxt = "ok";
                }
                conn.Close();
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

        public static RespondModel UpdateOne(string id,string name,string perms){
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime now = DateTime.Now;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = "SELECT COUNT(sys_groups_id) FROM systems_users_groups WHERE sys_groups_title = @sys_groups_title AND sys_groups_id != @sys_groups_id";
                cmd.Parameters.AddWithValue("@sys_groups_id",id.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_groups_title",name.TrimStart().TrimEnd());
                cmd.Parameters.AddWithValue("@sys_groups_perms",perms.TrimStart().TrimEnd());
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                RespondModel restext = new RespondModel();
                if(resCnt > 0){
                    restext.res_status = 500;
                    restext.res_errortxt = "พบชื่อนี้แล้ว";
                } else {
                    cmd.CommandText = $@"UPDATE systems_users_groups
                        SET sys_groups_title = @sys_groups_title,sys_groups_perms = @sys_groups_perms ,sys_groups_uby='{userd.sys_users_username}',sys_groups_utime=NOW()
                        WHERE sys_groups_id = @sys_groups_id LIMIT 1";
                    cmd.ExecuteNonQuery();
                    restext.res_status = 200;
                    restext.res_errortxt = "ok";
                }
                conn.Close();
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
                cmd.CommandText = "SELECT sys_groups_disabled FROM systems_users_groups WHERE sys_groups_id = @sys_groups_id";
                cmd.Parameters.AddWithValue("@sys_groups_id",id.TrimStart().TrimEnd());
                var resCnt = int.Parse(cmd.ExecuteScalar().ToString());
                RespondModel restext = new RespondModel();
                if(resCnt.ToString() == "1"){
                    vState = "0";
                }
                cmd.Parameters.AddWithValue("@sys_groups_disabled",vState.TrimStart().TrimEnd());
                cmd.CommandText = $@"UPDATE systems_users_groups SET sys_groups_disabled = @sys_groups_disabled WHERE sys_groups_id = @sys_groups_id LIMIT 1";
                cmd.ExecuteNonQuery();
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
                RespondModel restext = new RespondModel();
                    restext.res_status = 500;
                    restext.res_errortxt = ex.Message;
                return restext;
            }
        }
        public static RespondModel DeleteOne(string id){
            var userd = Startup.userd;
            string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime now = DateTime.Now;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = "SELECT COUNT(sys_users_id) FROM systems_users WHERE sys_users_group = @sys_users_group";
                cmd.Parameters.AddWithValue("@sys_users_group",id.TrimStart().TrimEnd());
                var resCntUsed = int.Parse(cmd.ExecuteScalar().ToString());
                RespondModel restext = new RespondModel();
                if(resCntUsed > 0){
                    restext.res_status = 500;
                    restext.res_errortxt = "มีผู้ใช้งานสิทธิอยู่";
                    return restext;
                }
                cmd.Parameters.Clear();
                cmd.CommandText = $@"DELETE FROM systems_users_groups WHERE sys_groups_id = @sys_groups_id LIMIT 1";
                cmd.Parameters.AddWithValue("@sys_groups_id",id.TrimStart().TrimEnd());
                cmd.ExecuteNonQuery();
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
                RespondModel restext = new RespondModel();
                    restext.res_status = 500;
                    restext.res_errortxt = ex.Message;
                return restext;
            }
        }

        public static DataTable GetListDropdown(){
             string constr = Startup.Configuration.GetConnectionString(Startup.Configuration["SelectConStr"]);
            MySqlConnection conn = new MySqlConnection(constr);
            DateTime now = DateTime.Now;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("", conn);
                cmd.CommandText = "SELECT * FROM systems_users_groups WHERE sys_groups_disabled = 0";
                MySqlDataReader dataReader = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                conn.Close();
                return dataTable;
            }
            catch (MySqlException)
            {
                if(conn != null){
                     conn.Close();
                }
                var dataTable = new DataTable();
                return dataTable;
            }
        }

    }
}