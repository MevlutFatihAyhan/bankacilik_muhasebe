using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

namespace BankAPI.Services
{
    public class HesapService
    {
        private readonly string _connectionString;

        public HesapService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        // Yeni hesap aç — PKG_HESAP üzerinden ve transaction yönetimiyle
        public void HesapEkle(Hesap hesap)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HESAP_EKLE", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = hesap.HESAP_NO;
                            cmd.Parameters.Add("p_musteri_id", OracleDbType.Decimal).Value = hesap.MUSTERI_ID;
                            cmd.Parameters.Add("p_iban", OracleDbType.Varchar2).Value = hesap.IBAN;
                            cmd.Parameters.Add("p_hesap_turu", OracleDbType.Varchar2).Value = hesap.HESAP_TURU;
                            cmd.Parameters.Add("p_doviz_cinsi", OracleDbType.Varchar2).Value = hesap.DOVIZ_CINSI;
                            cmd.Parameters.Add("p_bakiye", OracleDbType.Decimal).Value = hesap.BAKIYE;
                            cmd.Parameters.Add("p_durum", OracleDbType.Int32).Value = hesap.DURUM;
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

        // Hesap durumu güncelle — PKG_HESAP.PRC_HESAP_DURUM_GUNCELLE
        public void HesapDurumGuncelle(string hesapNo, int durum)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HESAP_DURUM_GUNCELLE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = hesapNo;
                    cmd.Parameters.Add("p_durum", OracleDbType.Int32).Value = durum;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Tek hesap getir — PKG_HESAP.PRC_HESAP_GETIR
        public Hesap HesapGetir(string hesapNo)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HESAP_GETIR", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = hesapNo;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapHesap(reader);
                        }
                    }
                }
            }
            return null;
        }

        // Bir müşterinin tüm hesaplarını getir — PKG_HESAP.PRC_MUSTERI_HESAPLARI (Inline SQL kaldırıldı)
        public List<Hesap> MusteriHesaplariGetir(decimal musteriId)
        {
            List<Hesap> hesapListesi = new List<Hesap>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_MUSTERI_HESAPLARI", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_musteri_id", OracleDbType.Decimal).Value = musteriId;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);
                    
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hesapListesi.Add(MapHesap(reader));
                        }
                    }
                }
            }
            return hesapListesi;
        }

        private static Hesap MapHesap(OracleDataReader reader)
        {
            return new Hesap
            {
                HESAP_NO = reader["HESAP_NO"]?.ToString()?.Trim(),
                MUSTERI_ID = Convert.ToDecimal(reader["MUSTERI_ID"]),
                IBAN = reader["IBAN"]?.ToString()?.Trim(),
                HESAP_TURU = reader["HESAP_TURU"]?.ToString(),
                DOVIZ_CINSI = reader["DOVIZ_CINSI"]?.ToString()?.Trim(),
                BAKIYE = Convert.ToDecimal(reader["BAKIYE"]),
                DURUM = Convert.ToInt32(reader["DURUM"])
            };
        }
    }
}
