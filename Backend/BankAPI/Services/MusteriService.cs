using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

namespace BankAPI.Services
{
    public class MusteriService
    {
        // Bağlantı dizemizi tutacağımız özel (private) değişken
        private readonly string _connectionString;

        // Sınıf oluşturulduğunda .NET otomatik olarak IConfiguration'ı buraya gönderir (Dependency Injection)
        public MusteriService(IConfiguration configuration)
        {
            // appsettings.json'daki "OracleConnection" isimli dizeyi okuyup değişkene atıyoruz
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        // Yeni müşteri ekle — PKG_MUSTERI.PRC_MUSTERI_EKLE
        public void MusteriEkle(Musteri yeniMusteri)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_EKLE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_AD", OracleDbType.Varchar2).Value = yeniMusteri.AD;
                    cmd.Parameters.Add("P_SOYAD", OracleDbType.Varchar2).Value = yeniMusteri.SOYAD;
                    cmd.Parameters.Add("P_MUSTERI_TIPI", OracleDbType.Int32).Value = yeniMusteri.MUSTERI_TIPI;
                    cmd.Parameters.Add("P_KIMLIK_NO", OracleDbType.Varchar2).Value = yeniMusteri.KIMLIK_NO;
                    cmd.Parameters.Add("P_EMAIL", OracleDbType.Varchar2).Value = yeniMusteri.EMAIL;
                    cmd.Parameters.Add("P_TELEFON", OracleDbType.Varchar2).Value = yeniMusteri.TELEFON;
                    cmd.Parameters.Add("P_AKTIF_MI", OracleDbType.Int32).Value = yeniMusteri.AKTIF_MI == 0 ? 1 : yeniMusteri.AKTIF_MI;

                    // ID'yi geri döndüren OUT parametresi
                    OracleParameter outIdParam = new OracleParameter("P_MUSTERI_ID", OracleDbType.Decimal);
                    outIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outIdParam);

                    cmd.ExecuteNonQuery();

                    if (outIdParam.Value != null && outIdParam.Value != DBNull.Value)
                    {
                        yeniMusteri.MusteriID = ((Oracle.ManagedDataAccess.Types.OracleDecimal)outIdParam.Value).Value;
                    }
                }
            }
        }

        // Müşteri güncelle — PKG_MUSTERI.PRC_MUSTERI_GUNCELLE
        public void MusteriGuncelleme(Musteri musteri)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_GUNCELLE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = musteri.MusteriID;
                    cmd.Parameters.Add("P_AD", OracleDbType.Varchar2).Value = musteri.AD;
                    cmd.Parameters.Add("P_SOYAD", OracleDbType.Varchar2).Value = musteri.SOYAD;
                    cmd.Parameters.Add("P_MUSTERI_TIPI", OracleDbType.Int32).Value = musteri.MUSTERI_TIPI;
                    cmd.Parameters.Add("P_KIMLIK_NO", OracleDbType.Varchar2).Value = musteri.KIMLIK_NO;
                    cmd.Parameters.Add("P_EMAIL", OracleDbType.Varchar2).Value = musteri.EMAIL;
                    cmd.Parameters.Add("P_TELEFON", OracleDbType.Varchar2).Value = musteri.TELEFON;
                    cmd.Parameters.Add("P_AKTIF_MI", OracleDbType.Int32).Value = musteri.AKTIF_MI;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Tüm müşteriler — PKG_MUSTERI.PRC_MUSTERI_LISTE
        public List<Musteri> MusterileriGetir()
        {
            List<Musteri> musteriListesi = new List<Musteri>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_LISTE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;

                    // Oracle RefCursor'ı karşılamak için OUT parametresi tanımlıyoruz
                    OracleParameter outCursor = new OracleParameter("P_RESULT", OracleDbType.RefCursor);
                    outCursor.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outCursor);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            musteriListesi.Add(MapMusteri(reader));
                        }
                    }
                }
            }
            return musteriListesi;
        }

        // Tek müşteri — PKG_MUSTERI.PRC_MUSTERI_GETIR
        public Musteri MusteriGetir(decimal musteriId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_GETIR", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = musteriId;

                    OracleParameter outCursor = new OracleParameter("P_RESULT", OracleDbType.RefCursor);
                    outCursor.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outCursor);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapMusteri(reader);
                        }
                    }
                }
            }
            return null;
        }

        // Müşteri sil — PKG_MUSTERI.PRC_MUSTERI_SIL
        public void MusteriSil(decimal musteriId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_SIL", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = musteriId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static Musteri MapMusteri(OracleDataReader reader)
        {
            return new Musteri
            {
                MusteriID = Convert.ToDecimal(reader["MUSTERI_ID"]),
                AD = reader["AD"]?.ToString(),
                SOYAD = reader["SOYAD"]?.ToString(),
                MUSTERI_TIPI = Convert.ToInt32(reader["MUSTERI_TIPI"]),
                KIMLIK_NO = reader["KIMLIK_NO"]?.ToString(),
                EMAIL = reader["EMAIL"]?.ToString(),
                TELEFON = reader["TELEFON"]?.ToString(),
                AKTIF_MI = Convert.ToInt32(reader["AKTIF_MI"]),
                OLUSTURMA_TARIHI = reader["OLUSTURMA_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["OLUSTURMA_TARIHI"]) : (DateTime?)null,
                GUNCELLEME_TARIHI = reader["GUNCELLEME_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["GUNCELLEME_TARIHI"]) : (DateTime?)null
            };
        }
    }
}
