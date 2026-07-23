using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

namespace BankAPI.Services
{
    public class AdminService{
        private readonly string _connectionString;

        public AdminService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        public List<Admin> AdminleriGetir(){
            List<Admin> AdminListesi = new List<Admin>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ADMIN_ID, ADMIN_KULLANICI_ADI, ADMIN_SIFRE FROM ADMIN";
                using (OracleCommand cmd = new OracleCommand(query,connection))
                {
                    cmd.CommandType = CommandType.Text;
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AdminListesi.Add(new Admin
                            {
                                AdminId = reader["ADMIN_ID"] != DBNull.Value ? Convert.ToInt32(reader["ADMIN_ID"]) : 0,
                                AdminKullaniciAdi = reader["ADMIN_KULLANICI_ADI"] != DBNull.Value ? reader["ADMIN_KULLANICI_ADI"].ToString() : string.Empty,
                                AdminSifre = reader["ADMIN_SIFRE"] != DBNull.Value ? reader["ADMIN_SIFRE"].ToString() : string.Empty
                            });
                        }
                    }        
                }

            }
            return AdminListesi;
        } 
    }
    
}