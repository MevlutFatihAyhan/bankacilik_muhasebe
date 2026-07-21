using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

namespace BankAPI.Services
{
    public class AdresService
    {
        private readonly string _connectionString;

        public AdresService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        // Yeni adres ekle — PKG_MUSTERI.PRC_ADRES_EKLE
        public void AdresEkle(MusteriAdres adres)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_ADRES_EKLE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = adres.MusteriID;
                    cmd.Parameters.Add("P_ADRES_BASLIK", OracleDbType.Varchar2).Value = adres.ADRES_BASLIK;
                    cmd.Parameters.Add("P_ULKE", OracleDbType.Varchar2).Value = adres.ULKE;
                    cmd.Parameters.Add("P_SEHIR", OracleDbType.Varchar2).Value = adres.SEHIR;
                    cmd.Parameters.Add("P_ILCE", OracleDbType.Varchar2).Value = adres.ILCE;
                    cmd.Parameters.Add("P_POSTA_KODU", OracleDbType.Varchar2).Value = adres.POSTA_KODU;
                    cmd.Parameters.Add("P_ACIK_ADRES", OracleDbType.Varchar2).Value = adres.ACIK_ADRES;

                    OracleParameter outIdParam = new OracleParameter("P_ADRES_ID", OracleDbType.Decimal);
                    outIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outIdParam);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Adres güncelle — PKG_MUSTERI.PRC_ADRES_GUNCELLE
        public void AdresGuncelle(MusteriAdres adres)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_ADRES_GUNCELLE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_ADRES_ID", OracleDbType.Decimal).Value = adres.AdresID;
                    cmd.Parameters.Add("P_ADRES_BASLIK", OracleDbType.Varchar2).Value = adres.ADRES_BASLIK;
                    cmd.Parameters.Add("P_ULKE", OracleDbType.Varchar2).Value = adres.ULKE;
                    cmd.Parameters.Add("P_SEHIR", OracleDbType.Varchar2).Value = adres.SEHIR;
                    cmd.Parameters.Add("P_ILCE", OracleDbType.Varchar2).Value = adres.ILCE;
                    cmd.Parameters.Add("P_POSTA_KODU", OracleDbType.Varchar2).Value = adres.POSTA_KODU;
                    cmd.Parameters.Add("P_ACIK_ADRES", OracleDbType.Varchar2).Value = adres.ACIK_ADRES;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Adres sil — PKG_MUSTERI.PRC_ADRES_SIL
        public void AdresSil(decimal adresId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_ADRES_SIL", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_ADRES_ID", OracleDbType.Decimal).Value = adresId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Bir müşterinin tüm adreslerini getir — PKG_MUSTERI.PRC_MUSTERI_ADRESLERI
        public List<MusteriAdres> MusteriAdresleriGetir(decimal musteriId)
        {
            List<MusteriAdres> adresListesi = new List<MusteriAdres>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_ADRESLERI", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = musteriId;

                    OracleParameter outCursor = new OracleParameter("P_RESULT", OracleDbType.RefCursor);
                    outCursor.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outCursor);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            adresListesi.Add(MapAdres(reader));
                        }
                    }
                }
            }
            return adresListesi;
        }

        // Tek adres getir — PKG_MUSTERI.PRC_ADRES_GETIR
        public MusteriAdres AdresGetir(decimal adresId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_ADRES_GETIR", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_ADRES_ID", OracleDbType.Decimal).Value = adresId;

                    OracleParameter outCursor = new OracleParameter("P_RESULT", OracleDbType.RefCursor);
                    outCursor.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outCursor);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAdres(reader);
                        }
                    }
                }
            }
            return null;
        }

        private static MusteriAdres MapAdres(OracleDataReader reader)
        {
            return new MusteriAdres
            {
                AdresID = Convert.ToDecimal(reader["ADRES_ID"]),
                MusteriID = Convert.ToDecimal(reader["MUSTERI_ID"]),
                ADRES_BASLIK = reader["ADRES_BASLIK"]?.ToString(),
                ULKE = reader["ULKE"]?.ToString(),
                SEHIR = reader["SEHIR"]?.ToString(),
                ILCE = reader["ILCE"]?.ToString(),
                POSTA_KODU = reader["POSTA_KODU"]?.ToString(),
                ACIK_ADRES = reader["ACIK_ADRES"]?.ToString(),
                OLUSTURMA_TARIHI = reader["OLUSTURMA_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["OLUSTURMA_TARIHI"]) : (DateTime?)null,
                GUNCELLEME_TARIHI = reader["GUNCELLEME_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["GUNCELLEME_TARIHI"]) : (DateTime?)null
            };
        }
    }
}
