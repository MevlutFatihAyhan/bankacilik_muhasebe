import oracledb

DB_USER = "ADMIN"
DB_PASSWORD = "MFZNAyhan2005."
DB_DSN = "(description= (retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1521)(host=adb.eu-frankfurt-1.oraclecloud.com))(connect_data=(service_name=gd33e9612e30a83_mfabank_medium.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)))"

try:
    connection = oracledb.connect(user=DB_USER, password=DB_PASSWORD, dsn=DB_DSN)
    cursor = connection.cursor()
    cursor.execute("SELECT NVL(MAX(MUSTERI_ID), 0) FROM MST_MUSTERI")
    max_id = cursor.fetchone()[0]
    print(f"MAX_ID={max_id}")
    
    cursor.execute("SELECT NVL(MAX(ADRES_ID), 0) FROM MST_MUSTERIADRES")
    print(f"MAX_ADRES_ID={cursor.fetchone()[0]}")
    cursor.close()
    connection.close()
except Exception as e:
    print(f"Hata: {e}")
