SELECT object_name, object_type, owner FROM dba_objects WHERE object_name LIKE 'MST%' OR object_name LIKE 'MVD%' OR object_name LIKE 'SP_MEVDUAT%' OR object_name LIKE 'PKG_%';
EXIT;
