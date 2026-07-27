using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

using BankAPI.Helpers;

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
            // 1. Döviz Cinsi Normalizasyonu (VARCHAR2(3) hatasını önlemek için EUR, USD, TRY, XAU dönüşümü)
            hesap.DOVIZ_CINSI = IbanHelper.NormalizeDovizCinsi(hesap.DOVIZ_CINSI);

            // 2. Hesap No Otomatik Oluşturma (Boş veya geçici ise)
            if (string.IsNullOrWhiteSpace(hesap.HESAP_NO) || hesap.HESAP_NO.StartsWith("ACC", StringComparison.OrdinalIgnoreCase))
            {
                hesap.HESAP_NO = IbanHelper.GenerateAccountNo();
            }

            // 3. IBAN Otomatik Oluşturma ve Doğrulama (Boş, geçici 'TR0000...' ise veya geçersiz ise)
            if (string.IsNullOrWhiteSpace(hesap.IBAN) || 
                hesap.IBAN == "TR000000000000000000000000" || 
                !IbanHelper.ValidateTrIban(hesap.IBAN))
            {
                hesap.IBAN = IbanHelper.GenerateTrIban(hesap.HESAP_NO);
            }
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

        // Tüm hesapları getir — PKG_HESAP.PRC_HESAP_LISTE
        public List<Hesap> TumHesaplariGetir()
        {
            List<Hesap> hesapListesi = new List<Hesap>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HESAP_LISTE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
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

        // Filtreye bağlı hesap listesi — PKG_HESAP.PRC_HESAP_LISTE
        // Tüm kriterler DB tarafında değerlendirilir; boş gelen kriterler NULL olarak
        // gönderilir ve prosedürde "o kriter yok" anlamına gelir.
        public List<Hesap> HesaplariFiltrele(
            string searchTerm,
            int? musteriTipi,
            string hesapTuru,
            string dovizCinsi,
            int? durum,
            decimal? minBakiye,
            decimal? maxBakiye)
        {
            List<Hesap> hesapListesi = new List<Hesap>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HESAP_LISTE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_search_term", OracleDbType.Varchar2).Value = NullIfEmpty(searchTerm);
                    cmd.Parameters.Add("p_musteri_tipi", OracleDbType.Int32).Value = NullIfEmpty(musteriTipi);
                    cmd.Parameters.Add("p_hesap_turu", OracleDbType.Varchar2).Value = NullIfEmpty(hesapTuru);
                    cmd.Parameters.Add("p_doviz_cinsi", OracleDbType.Varchar2).Value =
                        string.IsNullOrWhiteSpace(dovizCinsi) ? (object)DBNull.Value : IbanHelper.NormalizeDovizCinsi(dovizCinsi);
                    cmd.Parameters.Add("p_durum", OracleDbType.Int32).Value = NullIfEmpty(durum);
                    cmd.Parameters.Add("p_min_bakiye", OracleDbType.Decimal).Value = NullIfEmpty(minBakiye);
                    cmd.Parameters.Add("p_max_bakiye", OracleDbType.Decimal).Value = NullIfEmpty(maxBakiye);
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

        private static object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static object NullIfEmpty<T>(T? value) where T : struct
        {
            return value.HasValue ? (object)value.Value : DBNull.Value;
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

        // Para Transferi İşlemi — PRC_PARA_TRANSFERI (Stored Procedure çağrısı)
        public (string IslemKodu, string HataMesaji) ParaTransferi(string gonderenIban, string aliciIban, decimal tutar, string aciklama)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                // Transfer işlemi kendi içinde tutarlı olmalı, bu yüzden Transaction başlatıyoruz.
                // Gerçi SP içinde de transaction yapılabilirdi ama best practice olarak C# yönetir.
                using (OracleTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string islemKodu = "";
                        string hataMesaji = "";

                        using (OracleCommand cmd = new OracleCommand("PRC_PARA_TRANSFERI", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.BindByName = true;
                            
                            // IN parametreleri
                            cmd.Parameters.Add("p_gonderen_iban", OracleDbType.Varchar2).Value = gonderenIban;
                            cmd.Parameters.Add("p_alici_iban", OracleDbType.Varchar2).Value = aliciIban;
                            cmd.Parameters.Add("p_tutar", OracleDbType.Decimal).Value = tutar;
                            cmd.Parameters.Add("p_aciklama", OracleDbType.Varchar2).Value = aciklama;
                            
                            // OUT parametreleri (Boyut belirtmek önemlidir)
                            var outKodu = new OracleParameter("p_islem_kodu", OracleDbType.Varchar2, 10);
                            outKodu.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outKodu);
                            
                            var outMesaj = new OracleParameter("p_hata_mesaji", OracleDbType.Varchar2, 255);
                            outMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outMesaj);
                            
                            cmd.ExecuteNonQuery();
                            
                            // OUT değerlerini oku
                            islemKodu = outKodu.Value?.ToString();
                            hataMesaji = outMesaj.Value?.ToString();
                        }
                        
                        // Kodu 0 ise işlem başarılıdır, Commit atarız.
                        if (islemKodu == "0")
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }

                        return (islemKodu, hataMesaji);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Transfer işlemi veritabanında gerçekleştirilemedi: {ex.Message}");
                    }
                }
            }
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
