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
                try
                {
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
                catch (OracleException ex) when (ex.Number == 6550 || ex.Message.Contains("PRC_HAREKET_LISTE"))
                {
                    // Eğer prosedür henüz DB'de derlenmemişse doğrudan SELECT sorgusu ile getir
                    string fallbackQuery = "SELECT ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI, YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO FROM MVD_HESAPHAREKET ORDER BY ISLEM_TARIHI DESC";
                    using (OracleCommand cmd = new OracleCommand(fallbackQuery, connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                hareketListesi.Add(MapHareket(reader));
                            }
                        }
                    }
                }
            }
            return hareketListesi;
        }


        // Filtreye bağlı hareket listesi — PKG_HESAP.PRC_HAREKET_LISTE
        // Tüm kriterler DB tarafında değerlendirilir; boş gelen kriterler NULL olarak
        // gönderilir ve prosedürde "o kriter yok" anlamına gelir.
        public List<HesapHareket> HareketleriFiltrele(
            string searchTerm,
            string islemYonu,
            string dovizCinsi,
            DateTime? baslangicTarihi,
            DateTime? bitisTarihi,
            decimal? minTutar,
            decimal? maxTutar,
            string hesapNo,
            string musteriAdi,
            string musteriSoyadi)
        {
            List<HesapHareket> hareketListesi = new List<HesapHareket>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_HAREKET_LISTE", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_search_term", OracleDbType.Varchar2).Value = NullIfEmpty(searchTerm);
                    cmd.Parameters.Add("p_islem_yonu", OracleDbType.Varchar2).Value = NullIfEmpty(islemYonu);
                    cmd.Parameters.Add("p_doviz_cinsi", OracleDbType.Varchar2).Value = NullIfEmpty(dovizCinsi);
                    cmd.Parameters.Add("p_baslangic_tarihi", OracleDbType.Date).Value = NullIfEmpty(baslangicTarihi);
                    cmd.Parameters.Add("p_bitis_tarihi", OracleDbType.Date).Value = NullIfEmpty(bitisTarihi);
                    cmd.Parameters.Add("p_min_tutar", OracleDbType.Decimal).Value = NullIfEmpty(minTutar);
                    cmd.Parameters.Add("p_max_tutar", OracleDbType.Decimal).Value = NullIfEmpty(maxTutar);
                    cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = NullIfEmpty(hesapNo);
                    cmd.Parameters.Add("p_musteri_adi", OracleDbType.Varchar2).Value = NullIfEmpty(musteriAdi);
                    cmd.Parameters.Add("p_musteri_soyadi", OracleDbType.Varchar2).Value = NullIfEmpty(musteriSoyadi);
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

        private static object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static object NullIfEmpty<T>(T? value) where T : struct
        {
            return value.HasValue ? (object)value.Value : DBNull.Value;
        }

        // Tek bir hareketi ISLEM_ID ile getir — PKG_HESAP.PRC_HAREKET_GETIR
        public HesapHareket HareketGetir(decimal islemId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                try
                {
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
                catch (OracleException ex) when (ex.Number == 6550 || ex.Message.Contains("PRC_HAREKET_GETIR"))
                {
                    string fallbackQuery = "SELECT ISLEM_ID, HESAP_NO, ISLEM_YONU, ISLEM_TUTARI, DOVIZ_CINSI, YENI_BAKIYE, ISLEM_TARIHI, ACIKLAMA, ISLEM_KODU, REFERANS_NO FROM MVD_HESAPHAREKET WHERE ISLEM_ID = :islemId";
                    using (OracleCommand cmd = new OracleCommand(fallbackQuery, connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.BindByName = true;
                        cmd.Parameters.Add("islemId", OracleDbType.Decimal).Value = islemId;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapHareket(reader);
                            }
                        }
                    }
                }
            }
            return null;
        }


        // Kolonun sorgu sonucunda bulunup bulunmadığını kontrol eder — müşteri
        // kolonları yalnızca PRC_HAREKET_LISTE sonucunda döner.
        private static bool HasColumn(OracleDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
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
                REFERANS_NO = reader["REFERANS_NO"]?.ToString(),
                MUSTERI_ID = HasColumn(reader, "MUSTERI_ID") && reader["MUSTERI_ID"] != DBNull.Value
                    ? Convert.ToDecimal(reader["MUSTERI_ID"]) : (decimal?)null,
                MUSTERI_ADI = HasColumn(reader, "MUSTERI_ADI") ? reader["MUSTERI_ADI"]?.ToString()?.Trim() : null,
                MUSTERI_SOYADI = HasColumn(reader, "MUSTERI_SOYADI") ? reader["MUSTERI_SOYADI"]?.ToString()?.Trim() : null
            };
        }
    }
}
