using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

namespace BankAPI.Services
{
    public class HesapHareketService
    {
        private readonly string _connectionString;

        public HesapHareketService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        // Yeni hareket ekle — PKG_HESAP ve Transaction yönetimi ile
        public void HareketEkle(HesapHareket hareket)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                // Bakiye kontrolü ve kilitlenme DB tarafında FOR UPDATE ile yapılır.
                // Biz C# tarafında Transaction başlatarak Atomicity'i sağlıyoruz.
                using (OracleTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HAREKET_EKLE", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = hareket.HESAP_NO;
                            cmd.Parameters.Add("p_islem_yonu", OracleDbType.Varchar2).Value = hareket.ISLEM_YONU;
                            cmd.Parameters.Add("p_islem_tutari", OracleDbType.Decimal).Value = hareket.ISLEM_TUTARI;
                            cmd.Parameters.Add("p_doviz_cinsi", OracleDbType.Varchar2).Value = hareket.DOVIZ_CINSI;
                            cmd.Parameters.Add("p_aciklama", OracleDbType.Varchar2).Value = hareket.ACIKLAMA;
                            cmd.Parameters.Add("p_islem_kodu", OracleDbType.Varchar2).Value = hareket.ISLEM_KODU;
                            cmd.Parameters.Add("p_referans_no", OracleDbType.Varchar2).Value = hareket.REFERANS_NO;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Bir hesabın tüm hareketlerini getir — PKG_HESAP.PRC_HESAP_HAREKETLERI (Inline SQL kaldırıldı)
        public List<HesapHareket> HesapHareketleriGetir(string hesapNo)
        {
            List<HesapHareket> hareketListesi = new List<HesapHareket>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HESAP_HAREKETLERI", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = hesapNo;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);
                    
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hareketListesi.Add(MapHareket(reader));
                        }
                    }
                }
            }
            return hareketListesi;
        }

        // Tüm hesap hareketlerini getir — PKG_HESAP.PRC_HAREKET_LISTE
        public List<HesapHareket> TumHareketleriGetir()
        {
            List<HesapHareket> hareketListesi = new List<HesapHareket>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HAREKET_LISTE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hareketListesi.Add(MapHareket(reader));
                        }
                    }
                }
            }
            return hareketListesi;
        }

        // Tek bir hareketi ISLEM_ID ile getir — PKG_HESAP.PRC_HAREKET_GETIR
        public HesapHareket HareketGetir(decimal islemId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HAREKET_GETIR", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_islem_id", OracleDbType.Decimal).Value = islemId;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapHareket(reader);
                        }
                    }
                }
            }
            return null;
        }

        private static HesapHareket MapHareket(OracleDataReader reader)
        {
            return new HesapHareket
            {
                ISLEM_ID = Convert.ToDecimal(reader["ISLEM_ID"]),
                HESAP_NO = reader["HESAP_NO"]?.ToString()?.Trim(),
                ISLEM_YONU = reader["ISLEM_YONU"]?.ToString()?.Trim(),
                ISLEM_TUTARI = Convert.ToDecimal(reader["ISLEM_TUTARI"]),
                DOVIZ_CINSI = reader["DOVIZ_CINSI"]?.ToString()?.Trim(),
                YENI_BAKIYE = reader["YENI_BAKIYE"] != DBNull.Value ? Convert.ToDecimal(reader["YENI_BAKIYE"]) : 0,
                ISLEM_TARIHI = Convert.ToDateTime(reader["ISLEM_TARIHI"]),
                ACIKLAMA = reader["ACIKLAMA"]?.ToString(),
                ISLEM_KODU = reader["ISLEM_KODU"]?.ToString(),
                REFERANS_NO = reader["REFERANS_NO"]?.ToString()
            };
        }
    }
}
