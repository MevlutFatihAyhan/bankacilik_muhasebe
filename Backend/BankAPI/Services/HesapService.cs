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

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                
                // 2. Hesap No Otomatik Oluşturma (Boş veya geçici ise)
                if (string.IsNullOrWhiteSpace(hesap.HESAP_NO) || hesap.HESAP_NO.StartsWith("ACC", StringComparison.OrdinalIgnoreCase))
                {
                    int musteriTipi = GetMusteriTipi(hesap.MUSTERI_ID, connection);
                    hesap.HESAP_NO = IbanHelper.GenerateAccountNo(musteriTipi);
                }

                // 3. IBAN Otomatik Oluşturma ve Doğrulama (Boş, geçici 'TR0000...' ise veya geçersiz ise)
                if (string.IsNullOrWhiteSpace(hesap.IBAN) || 
                    hesap.IBAN == "TR000000000000000000000000" || 
                    !IbanHelper.ValidateTrIban(hesap.IBAN))
                {
                    hesap.IBAN = IbanHelper.GenerateTrIban(hesap.HESAP_NO);
                }

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
            decimal? maxBakiye,
            string hesapNo,
            string musteriAdi,
            string musteriSoyadi)
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
                    cmd.Parameters.Add("p_hesap_no", OracleDbType.Varchar2).Value = NullIfEmpty(hesapNo);
                    cmd.Parameters.Add("p_musteri_adi", OracleDbType.Varchar2).Value = NullIfEmpty(musteriAdi);
                    cmd.Parameters.Add("p_musteri_soyadi", OracleDbType.Varchar2).Value = NullIfEmpty(musteriSoyadi);
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

        // Para transferi — PKG_HESAP.PRC_PARA_TRANSFERI
        // Tüm iş kuralları (IBAN kontrolü, hesap durumu, döviz uyumu, bakiye yeterliliği)
        // ve bakiye güncellemesi DB tarafındadır; burada sadece parametre hazırlığı,
        // transaction yönetimi ve OUT parametrelerinin okunması yapılır.
        public ParaTransferiSonuc ParaTransferi(ParaTransferiRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // IBAN'lar boşluklardan arındırılıp büyük harfe çevrilir; kullanıcı
            // "TR12 3456 ..." şeklinde yazdığında da hesap bulunabilsin diye.
            string gonderenIban = NormalizeIban(request.GonderenIban);
            string aliciIban = NormalizeIban(request.AliciIban);

            var sonuc = new ParaTransferiSonuc();

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                // İki hesap hareketi tek bir bütün olarak yazılmalı: prosedür COMMIT/ROLLBACK
                // yapmaz, kararı işlem koduna bakarak buradaki transaction verir.
                using (OracleTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (OracleCommand cmd = new OracleCommand("PKG_HESAP.PRC_PARA_TRANSFERI", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.BindByName = true;

                            // IN parametreleri
                            cmd.Parameters.Add("p_gonderen_iban", OracleDbType.Varchar2).Value = gonderenIban;
                            cmd.Parameters.Add("p_alici_iban", OracleDbType.Varchar2).Value = aliciIban;
                            cmd.Parameters.Add("p_tutar", OracleDbType.Decimal).Value = request.Tutar;
                            cmd.Parameters.Add("p_aciklama", OracleDbType.Varchar2).Value = NullIfEmpty(request.Aciklama);

                            // OUT parametreleri (Varchar2 için boyut belirtmek zorunlu)
                            var outKodu = new OracleParameter("p_islem_kodu", OracleDbType.Varchar2, 10)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outKodu);

                            var outMesaj = new OracleParameter("p_hata_mesaji", OracleDbType.Varchar2, 500)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outMesaj);

                            var outReferans = new OracleParameter("p_referans_no", OracleDbType.Varchar2, 50)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outReferans);

                            var outGonderenIslemId = new OracleParameter("p_gonderen_islem_id", OracleDbType.Decimal)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outGonderenIslemId);

                            var outAliciIslemId = new OracleParameter("p_alici_islem_id", OracleDbType.Decimal)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outAliciIslemId);

                            cmd.ExecuteNonQuery();

                            sonuc.IslemKodu = OracleMetinDegeri(outKodu);
                            sonuc.Mesaj = OracleMetinDegeri(outMesaj);
                            sonuc.ReferansNo = OracleMetinDegeri(outReferans);
                            sonuc.GonderenIslemId = OracleSayiDegeri(outGonderenIslemId);
                            sonuc.AliciIslemId = OracleSayiDegeri(outAliciIslemId);
                        }

                        // Sadece işlem kodu '0' ise kalıcı hale getirilir; iş kuralı ihlalinde
                        // veya DB hatasında (kod '500') hiçbir hareket yazılmamış olmalıdır.
                        if (sonuc.Basarili)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }

                        return sonuc;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Transfer işlemi veritabanında gerçekleştirilemedi: {ex.Message}", ex);
                    }
                }
            }
        }

        private static string NormalizeIban(string iban)
        {
            return string.IsNullOrWhiteSpace(iban)
                ? null
                : iban.Replace(" ", string.Empty).Trim().ToUpperInvariant();
        }

        // OracleString'in null hali ToString() ile "null" metnine dönüştüğü için
        // OUT parametreleri her zaman tip kontrolüyle okunur.
        private static string OracleMetinDegeri(OracleParameter parametre)
        {
            if (parametre.Value == null || parametre.Value is DBNull)
            {
                return null;
            }

            if (parametre.Value is Oracle.ManagedDataAccess.Types.OracleString oracleString)
            {
                return oracleString.IsNull ? null : oracleString.Value;
            }

            return parametre.Value.ToString();
        }

        private static decimal? OracleSayiDegeri(OracleParameter parametre)
        {
            if (parametre.Value == null || parametre.Value is DBNull)
            {
                return null;
            }

            if (parametre.Value is Oracle.ManagedDataAccess.Types.OracleDecimal oracleDecimal)
            {
                return oracleDecimal.IsNull ? (decimal?)null : oracleDecimal.Value;
            }

            return Convert.ToDecimal(parametre.Value);
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

        private int GetMusteriTipi(decimal musteriId, OracleConnection connection)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("SELECT MUSTERI_TIPI FROM MST_MUSTERI WHERE MUSTERI_ID = :p_musteri_id", connection))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_musteri_id", OracleDbType.Decimal).Value = musteriId;
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch
            {
                // Ignore DB errors and default to individual
            }
            return 1; // Default Bireysel
        }
    }
}
