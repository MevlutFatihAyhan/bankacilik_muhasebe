-- Baglanan kullanicinin semasindaki uygulama nesnelerini listeler
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name LIKE 'MST%'
    OR object_name LIKE 'MVD%'
    OR object_name LIKE 'PKG_%'
    OR object_name LIKE 'FN_%'
    OR object_name LIKE 'SEQ_%'
 ORDER BY object_type, object_name;
