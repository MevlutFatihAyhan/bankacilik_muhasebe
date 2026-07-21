
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

        public void MusteriEkle(Musteri yeniMusteri){
            using (OracleConnection connection = new OracleConnection(_connectionString)){
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_EKLE", connection)){
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true; 
                    cmd.Parameters.Add("P_AD", OracleDbType.Varchar2).Value = yeniMusteri.AD;
                    cmd.Parameters.Add("P_SOYAD", OracleDbType.Varchar2).Value = yeniMusteri.SOYAD;
                    cmd.Parameters.Add("P_MUSTERI_TIPI", OracleDbType.Int32).Value = yeniMusteri.MUSTERI_TIPI;
                    cmd.Parameters.Add("P_TCKN_VKN", OracleDbType.Varchar2).Value = yeniMusteri.TCKN_VKN;
                    cmd.Parameters.Add("P_EMAIL", OracleDbType.Varchar2).Value = yeniMusteri.EMAIL;
                    cmd.Parameters.Add("P_TELEFON", OracleDbType.Varchar2).Value = yeniMusteri.TELEFON;
                    cmd.Parameters.Add("P_AKTIF_MI", OracleDbType.Int32).Value = yeniMusteri.AKTIFMI;
                    
                    // ID'yi geri döndüren OUT parametresi
                    OracleParameter outIdParam = new OracleParameter("P_MUSTERI_ID", OracleDbType.Decimal);
                    outIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outIdParam);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public void MusteriGuncelleme(Musteri musteri){
            using (OracleConnection connection = new OracleConnection(_connectionString)){
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_GUNCELLE", connection)){
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = musteri.MusteriID;
                    cmd.Parameters.Add("P_AD", OracleDbType.Varchar2).Value = musteri.AD;
                    cmd.Parameters.Add("P_SOYAD", OracleDbType.Varchar2).Value = musteri.SOYAD;
                    cmd.Parameters.Add("P_MUSTERI_TIPI", OracleDbType.Int32).Value = musteri.MUSTERI_TIPI;
                    cmd.Parameters.Add("P_TCKN_VKN", OracleDbType.Varchar2).Value = musteri.TCKN_VKN;
                    cmd.Parameters.Add("P_EMAIL", OracleDbType.Varchar2).Value = musteri.EMAIL;
                    cmd.Parameters.Add("P_TELEFON", OracleDbType.Varchar2).Value = musteri.TELEFON;
                    cmd.Parameters.Add("P_AKTIF_MI", OracleDbType.Int32).Value = musteri.AKTIFMI;
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<Musteri> MusterileriGetir(){
            List<Musteri> musteriListesi = new List<Musteri>();
            using (OracleConnection connection = new OracleConnection(_connectionString)) {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_LISTE", connection)) {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;

                    // Oracle RefCursor'ı karşılamak için OUT parametresi tanımlıyoruz
                    OracleParameter outCursor = new OracleParameter("P_RESULT", OracleDbType.RefCursor);
                    outCursor.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outCursor);

                    using (OracleDataReader reader = cmd.ExecuteReader()) {
                        while(reader.Read()) {
                            Musteri musteri = new Musteri();
                            musteri.MusteriID = Convert.ToDecimal(reader["MUSTERI_ID"]);
                            musteri.AD = reader["AD"].ToString();
                            musteri.SOYAD = reader["SOYAD"].ToString();
                            musteri.MUSTERI_TIPI = Convert.ToInt32(reader["MUSTERI_TIPI"]);
                            musteri.TCKN_VKN = reader["TCKN_VKN"]?.ToString();
                            musteri.EMAIL = reader["EMAIL"].ToString();
                            musteri.TELEFON = reader["TELEFON"].ToString();
                            musteri.AKTIFMI = Convert.ToInt32(reader["AKTIF_MI"]);
                            musteriListesi.Add(musteri);
                        }
                    }
                }
            } 
            return musteriListesi;           
        }

        public void MusteriSil(decimal musteriId){
            using (OracleConnection connection = new OracleConnection(_connectionString)){
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_MUSTERI.PRC_MUSTERI_SIL", connection)){
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("P_MUSTERI_ID", OracleDbType.Decimal).Value = musteriId;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
