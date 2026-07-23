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

        public List<Admin> AdminleriGetir()
        {
            List<Admin> AdminListesi = new List<Admin>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ADMIN_ID, KULLANICI_ADI, SIFRE FROM MVD_ADMIN";
                using (OracleCommand cmd = new OracleCommand(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AdminListesi.Add(new Admin
                            {
                                AdminId = reader["ADMIN_ID"] != DBNull.Value ? Convert.ToInt32(reader["ADMIN_ID"]) : 0,
                                AdminKullaniciAdi = reader["KULLANICI_ADI"] != DBNull.Value ? reader["KULLANICI_ADI"].ToString() : string.Empty,
                                AdminSifre = reader["SIFRE"] != DBNull.Value ? reader["SIFRE"].ToString() : string.Empty
                            });
                        }
                    }
                }
            }
            return AdminListesi;
        }

        public Admin AdminGirisYap(string kullaniciAdi, string sifre)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ADMIN_ID, KULLANICI_ADI, SIFRE FROM MVD_ADMIN WHERE UPPER(TRIM(KULLANICI_ADI)) = UPPER(TRIM(:kullaniciAdi)) AND TRIM(SIFRE) = TRIM(:sifre)";
                using (OracleCommand cmd = new OracleCommand(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.BindByName = true;
                    cmd.Parameters.Add(new OracleParameter("kullaniciAdi", kullaniciAdi));
                    cmd.Parameters.Add(new OracleParameter("sifre", sifre));
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Admin
                            {
                                AdminId = reader["ADMIN_ID"] != DBNull.Value ? Convert.ToInt32(reader["ADMIN_ID"]) : 0,
                                AdminKullaniciAdi = reader["KULLANICI_ADI"] != DBNull.Value ? reader["KULLANICI_ADI"].ToString() : string.Empty,
                                AdminSifre = reader["SIFRE"] != DBNull.Value ? reader["SIFRE"].ToString() : string.Empty
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}