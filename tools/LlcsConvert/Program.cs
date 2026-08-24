using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LlcsConvert
{
    internal static class Program
    {
        private static readonly HashSet<string> SkipTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Address",
            "Country",
            "PhoneNumber",
            "Party",
            "FileSystemStoreObject",
            "FileSystemLinkObject",
            "ImportMap",
            "MappableProperty",
            "Mapping",
            "ReminderInfo",
            "LoopItem",
            "LoopCheck",
            "Event",
            "Person",
            "XPObjectType",
            "ModelDifference",
            "ModelDifferenceAspect",
            "ReportDataV2"
        };

        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
                return 1;
            }

            string command = args[0];
            try
            {
                if (string.Equals(command, "list", StringComparison.OrdinalIgnoreCase) && args.Length >= 2)
                {
                    ListSchema(Path.GetFullPath(args[1]));
                    return 0;
                }
                if (string.Equals(command, "convert", StringComparison.OrdinalIgnoreCase) && args.Length >= 3)
                {
                    ConvertFile(Path.GetFullPath(args[1]), Path.GetFullPath(args[2]), backupOriginal: false);
                    return 0;
                }
                if (string.Equals(command, "convert-dir", StringComparison.OrdinalIgnoreCase) && args.Length >= 2)
                {
                    ConvertDirectory(Path.GetFullPath(args[1]));
                    return 0;
                }
                Usage();
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 3;
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  LlcsConvert list <file.llcs>");
            Console.WriteLine("  LlcsConvert convert <source.llcs> <dest.llcs>");
            Console.WriteLine("  LlcsConvert convert-dir <folder>");
        }

        private static void ConvertDirectory(string folder)
        {
            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException(folder);
            }

            string backupDir = Path.Combine(folder, "sqlce-backup");
            Directory.CreateDirectory(backupDir);

            string[] files = Directory.GetFiles(folder, "*.llcs");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            int converted = 0;
            int skipped = 0;
            foreach (string file in files)
            {
                if (IsSqlite(file))
                {
                    Console.WriteLine("SKIP already SQLite: " + Path.GetFileName(file));
                    skipped++;
                    continue;
                }

                string backupPath = Path.Combine(backupDir, Path.GetFileName(file));
                if (!File.Exists(backupPath))
                {
                    File.Copy(file, backupPath, false);
                    Console.WriteLine("BACKUP " + backupPath);
                }

                string tempPath = file + ".sqlite.tmp";
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                Console.WriteLine("CONVERT " + Path.GetFileName(file));
                ConvertFile(file, tempPath, backupOriginal: false);
                File.Delete(file);
                File.Move(tempPath, file);
                converted++;
                Console.WriteLine("DONE " + Path.GetFileName(file) + " -> SQLite " + new FileInfo(file).Length + " bytes");
            }

            Console.WriteLine("Converted {0}, skipped {1}. SQL Compact originals are in {2}", converted, skipped, backupDir);
        }

        private static void ConvertFile(string source, string dest, bool backupOriginal)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("File not found", source);
            }
            if (IsSqlite(source))
            {
                throw new InvalidOperationException("Source is already SQLite: " + source);
            }
            if (backupOriginal)
            {
                string backup = source + ".sqlce.bak";
                if (!File.Exists(backup))
                {
                    File.Copy(source, backup, false);
                }
            }

            if (File.Exists(dest))
            {
                File.Delete(dest);
            }

            CreateSqliteSchema(dest);

            string sqlce = "Data Source=" + source + ";Max Database Size=4091";
            using (var src = new SqlCeConnection(sqlce))
            using (var dst = new SQLiteConnection("Data Source=" + dest + ";Version=3;"))
            {
                src.Open();
                dst.Open();
                using (var pragma = dst.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA synchronous=OFF; PRAGMA journal_mode=MEMORY; PRAGMA foreign_keys=OFF;";
                    pragma.ExecuteNonQuery();
                }

                using (var tx = dst.BeginTransaction())
                {
                    HashSet<string> sourceTables = GetSqlCeTables(src);
                    Dictionary<string, HashSet<string>> destColumns = GetSqliteTables(dst);
                    Dictionary<Guid, int> loopItems = LoadLoopItems(src, sourceTables);

                    CopyXpObjectType(src, dst, tx, sourceTables);
                    CopyFileLinks(src, dst, tx, sourceTables, destColumns);
                    CopyPersons(src, dst, tx, sourceTables, destColumns);
                    CopyEvents(src, dst, tx, sourceTables, destColumns, loopItems);

                    foreach (string table in sourceTables)
                    {
                        if (SkipTables.Contains(table) || StartsWithSkipPrefix(table))
                        {
                            continue;
                        }
                        if (!destColumns.ContainsKey(table))
                        {
                            Console.WriteLine("  skip unknown table " + table);
                            continue;
                        }
                        int rows = CopyTable(src, dst, tx, table, table, destColumns[table], null);
                        Console.WriteLine("  {0}: {1} rows", table, rows);
                    }

                    ResetSequences(dst, tx, destColumns.Keys);
                    tx.Commit();
                }
            }
        }

        private static bool StartsWithSkipPrefix(string table)
        {
            return table.StartsWith("Kpi", StringComparison.OrdinalIgnoreCase)
                || table.StartsWith("SecuritySystem", StringComparison.OrdinalIgnoreCase)
                || table.StartsWith("Xpand", StringComparison.OrdinalIgnoreCase);
        }

        private static void CreateSqliteSchema(string dest)
        {
            string sqlPath = FindTemplateSql();
            string script = File.ReadAllText(sqlPath);
            using (var dst = new SQLiteConnection("Data Source=" + dest + ";Version=3;"))
            {
                dst.Open();
                using (var cmd = dst.CreateCommand())
                {
                    cmd.CommandText = script;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string FindTemplateSql()
        {
            string[] candidates =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "empty-template.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\empty-template.sql"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\empty-template.sql"))
            };
            foreach (string path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            throw new FileNotFoundException("empty-template.sql not found next to LlcsConvert.exe");
        }

        private static void CopyXpObjectType(SqlCeConnection src, SQLiteConnection dst, SQLiteTransaction tx, HashSet<string> sourceTables)
        {
            if (!sourceTables.Contains("XPObjectType"))
            {
                return;
            }

            int rows = 0;
            using (var read = new SqlCeCommand("SELECT OID, TypeName, AssemblyName FROM [XPObjectType]", src))
            using (var reader = read.ExecuteReader())
            using (var insert = dst.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO [XPObjectType] ([OID], [TypeName], [AssemblyName]) VALUES (@OID, @TypeName, @AssemblyName)";
                var oid = insert.Parameters.Add("@OID", DbType.Int32);
                var typeName = insert.Parameters.Add("@TypeName", DbType.String);
                var assembly = insert.Parameters.Add("@AssemblyName", DbType.String);
                while (reader.Read())
                {
                    string originalType = reader.IsDBNull(1) ? null : Convert.ToString(reader.GetValue(1));
                    string originalAssembly = reader.IsDBNull(2) ? null : Convert.ToString(reader.GetValue(2));
                    oid.Value = reader.GetInt32(0);
                    typeName.Value = (object)RemapTypeName(originalType) ?? DBNull.Value;
                    assembly.Value = (object)RemapAssembly(originalType, originalAssembly) ?? DBNull.Value;
                    insert.ExecuteNonQuery();
                    rows++;
                }
            }
            Console.WriteLine("  XPObjectType: {0} rows", rows);
        }

        private static string RemapTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return typeName;
            }
            if (typeName.IndexOf("FileSystemLinkObject", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Landrys_Loop_Checkout_System.Module.BusinessObjects.FileLinkObject";
            }
            if (string.Equals(typeName, "DevExpress.Persistent.BaseImpl.Person", StringComparison.OrdinalIgnoreCase))
            {
                return "Landrys_Loop_Checkout_System.Module.BusinessObjects.Person";
            }
            return typeName;
        }

        private static string RemapAssembly(string typeName, string assembly)
        {
            string remappedType = RemapTypeName(typeName);
            if (!string.IsNullOrEmpty(remappedType) && remappedType.StartsWith("Landrys_Loop_Checkout_System.Module.", StringComparison.Ordinal))
            {
                return "Landrys_Loop_Checkout_System.Module";
            }
            if (string.IsNullOrEmpty(assembly))
            {
                return assembly;
            }
            if (assembly.StartsWith("DevExpress.Persistent.BaseImpl", StringComparison.OrdinalIgnoreCase))
            {
                return "DevExpress.Persistent.BaseImpl.Xpo.v26.1";
            }
            if (Regex.IsMatch(assembly, @"^DevExpress\.Xpo\.v\d+"))
            {
                return "DevExpress.Xpo.v26.1";
            }
            return assembly;
        }

        private static void CopyFileLinks(SqlCeConnection src, SQLiteConnection dst, SQLiteTransaction tx, HashSet<string> sourceTables, Dictionary<string, HashSet<string>> destColumns)
        {
            if (!sourceTables.Contains("FileSystemLinkObject") || !destColumns.ContainsKey("FileLinkObject"))
            {
                return;
            }

            int rows = CopyTable(src, dst, tx, "FileSystemLinkObject", "FileLinkObject", destColumns["FileLinkObject"], null);
            Console.WriteLine("  FileLinkObject: {0} rows", rows);
        }

        private static void CopyPersons(SqlCeConnection src, SQLiteConnection dst, SQLiteTransaction tx, HashSet<string> sourceTables, Dictionary<string, HashSet<string>> destColumns)
        {
            if (!sourceTables.Contains("Person") || !destColumns.ContainsKey("Person"))
            {
                return;
            }

            string sql = sourceTables.Contains("Party")
                ? "SELECT p.Oid, p.FirstName, p.LastName, p.MiddleName, p.Email, p.Birthday, party.OptimisticLockField, party.GCRecord, party.ObjectType FROM [Person] p LEFT OUTER JOIN [Party] party ON p.Oid = party.Oid"
                : "SELECT * FROM [Person]";

            int rows = CopyQuery(src, dst, tx, sql, "Person", destColumns["Person"]);
            Console.WriteLine("  Person: {0} rows", rows);
        }

        private static void CopyEvents(SqlCeConnection src, SQLiteConnection dst, SQLiteTransaction tx, HashSet<string> sourceTables, Dictionary<string, HashSet<string>> destColumns, Dictionary<Guid, int> loopItems)
        {
            if (!destColumns.ContainsKey("Event"))
            {
                return;
            }

            Dictionary<string, Dictionary<string, object>> byOid = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            if (sourceTables.Contains("Event"))
            {
                LoadRows(src, "SELECT * FROM [Event]", byOid);
            }
            if (sourceTables.Contains("LoopCheck"))
            {
                LoadRows(src, "SELECT * FROM [LoopCheck]", byOid);
            }

            int eventRows = 0;
            int thinRows = 0;
            foreach (Dictionary<string, object> row in byOid.Values)
            {
                object oid = GetRow(row, "Oid");
                if (oid == null)
                {
                    continue;
                }

                object item = ConvertEventItem(GetRow(row, "Item"), loopItems);
                object checkDate = FirstDate(GetRow(row, "CheckDate"), GetRow(row, "ScheduledCheckDate"), GetRow(row, "StartOn"));
                object startOn = FirstDate(GetRow(row, "StartOn"), checkDate);
                object endOn = FirstDate(GetRow(row, "EndOn"), startOn);
                object allDay = GetRow(row, "AllDay");
                if (allDay == null)
                {
                    allDay = true;
                }

                InsertEvent(dst, tx, destColumns["Event"], row, oid, item, checkDate, startOn, endOn, allDay);
                eventRows++;

                if (destColumns.ContainsKey("LoopCheck"))
                {
                    InsertThinLoopCheck(dst, tx, oid, startOn, endOn, GetRow(row, "OptimisticLockField"), GetRow(row, "GCRecord"));
                    thinRows++;
                }
            }

            Console.WriteLine("  Event: {0} rows", eventRows);
            if (thinRows > 0)
            {
                Console.WriteLine("  LoopCheck: {0} rows", thinRows);
            }
        }

        private static void InsertEvent(SQLiteConnection dst, SQLiteTransaction tx, HashSet<string> destCols, Dictionary<string, object> row, object oid, object item, object checkDate, object startOn, object endOn, object allDay)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> pair in row)
            {
                values[pair.Key] = pair.Value;
            }
            values["Oid"] = oid;
            values["Item"] = item;
            values["CheckDate"] = checkDate;
            values["StartOn"] = startOn;
            values["EndOn"] = endOn;
            values["AllDay"] = allDay;

            List<string> cols = new List<string>();
            foreach (string col in destCols)
            {
                if (values.ContainsKey(col))
                {
                    cols.Add(col);
                }
            }

            using (var insert = dst.CreateCommand())
            {
                insert.Transaction = tx;
                StringBuilder names = new StringBuilder();
                StringBuilder parms = new StringBuilder();
                for (int i = 0; i < cols.Count; i++)
                {
                    if (i > 0)
                    {
                        names.Append(", ");
                        parms.Append(", ");
                    }
                    names.Append("[").Append(cols[i]).Append("]");
                    parms.Append("@p").Append(i);
                    insert.Parameters.AddWithValue("@p" + i, ToSqlite(values[cols[i]]));
                }
                insert.CommandText = "INSERT INTO [Event] (" + names + ") VALUES (" + parms + ")";
                insert.ExecuteNonQuery();
            }
        }

        private static void InsertThinLoopCheck(SQLiteConnection dst, SQLiteTransaction tx, object oid, object startOn, object endOn, object lockField, object gc)
        {
            using (var insert = dst.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO [LoopCheck] ([Oid], [StartOn], [EndOn], [OptimisticLockField], [GCRecord]) VALUES (@Oid, @StartOn, @EndOn, @Lock, @GC)";
                insert.Parameters.AddWithValue("@Oid", ToSqlite(oid));
                insert.Parameters.AddWithValue("@StartOn", ToSqlite(startOn));
                insert.Parameters.AddWithValue("@EndOn", ToSqlite(endOn));
                insert.Parameters.AddWithValue("@Lock", ToSqlite(lockField));
                insert.Parameters.AddWithValue("@GC", ToSqlite(gc));
                insert.ExecuteNonQuery();
            }
        }

        private static object ConvertEventItem(object value, Dictionary<Guid, int> loopItems)
        {
            if (value == null)
            {
                return null;
            }
            if (value is Guid)
            {
                Guid guid = (Guid)value;
                int loopOid;
                if (loopItems.TryGetValue(guid, out loopOid))
                {
                    return loopOid;
                }
                return null;
            }
            if (value is string)
            {
                Guid guid;
                if (Guid.TryParse((string)value, out guid))
                {
                    return ConvertEventItem(guid, loopItems);
                }
            }
            if (value is byte[])
            {
                byte[] bytes = (byte[])value;
                if (bytes.Length == 16)
                {
                    return ConvertEventItem(new Guid(bytes), loopItems);
                }
            }
            if (value is short || value is int || value is long)
            {
                return Convert.ToInt32(value);
            }
            return value;
        }

        private static object FirstDate(params object[] values)
        {
            foreach (object value in values)
            {
                if (value is DateTime)
                {
                    DateTime dt = (DateTime)value;
                    if (dt.Year > 1753)
                    {
                        return dt;
                    }
                }
            }
            return null;
        }

        private static object GetRow(Dictionary<string, object> row, string name)
        {
            object value;
            if (row.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

        private static void LoadRows(SqlCeConnection src, string sql, Dictionary<string, Dictionary<string, object>> byOid)
        {
            using (var cmd = new SqlCeCommand(sql, src))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Dictionary<string, object> row = ReadRow(reader);
                    object oid = GetRow(row, "Oid");
                    if (oid == null)
                    {
                        continue;
                    }
                    string key = NormalizeKey(oid);
                    Dictionary<string, object> existing;
                    if (!byOid.TryGetValue(key, out existing))
                    {
                        byOid[key] = row;
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object> pair in row)
                        {
                            if (!existing.ContainsKey(pair.Key) || existing[pair.Key] == null)
                            {
                                existing[pair.Key] = pair.Value;
                            }
                        }
                    }
                }
            }
        }

        private static string NormalizeKey(object oid)
        {
            if (oid is Guid)
            {
                return ((Guid)oid).ToString("D");
            }
            if (oid is byte[])
            {
                return new Guid((byte[])oid).ToString("D");
            }
            return Convert.ToString(oid, CultureInfo.InvariantCulture);
        }

        private static Dictionary<Guid, int> LoadLoopItems(SqlCeConnection src, HashSet<string> sourceTables)
        {
            Dictionary<Guid, int> map = new Dictionary<Guid, int>();
            if (!sourceTables.Contains("LoopItem"))
            {
                return map;
            }
            using (var cmd = new SqlCeCommand("SELECT Oid, Loop FROM [LoopItem] WHERE Loop IS NOT NULL", src))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1))
                    {
                        continue;
                    }
                    Guid oid = ReadGuid(reader.GetValue(0));
                    map[oid] = Convert.ToInt32(reader.GetValue(1));
                }
            }
            Console.WriteLine("  LoopItem map: {0} rows (used only to resolve Event.Item)", map.Count);
            return map;
        }

        private static Guid ReadGuid(object value)
        {
            if (value is Guid)
            {
                return (Guid)value;
            }
            if (value is byte[])
            {
                return new Guid((byte[])value);
            }
            return Guid.Parse(Convert.ToString(value));
        }

        private static int CopyTable(SqlCeConnection src, SQLiteConnection dst, SQLiteTransaction tx, string sourceTable, string destTable, HashSet<string> destCols, string where)
        {
            string sql = "SELECT * FROM [" + sourceTable.Replace("]", "]]") + "]";
            if (!string.IsNullOrEmpty(where))
            {
                sql += " WHERE " + where;
            }
            return CopyQuery(src, dst, tx, sql, destTable, destCols);
        }

        private static int CopyQuery(SqlCeConnection src, SQLiteConnection dst, SQLiteTransaction tx, string sql, string destTable, HashSet<string> destCols)
        {
            int rows = 0;
            using (var read = new SqlCeCommand(sql, src))
            using (var reader = read.ExecuteReader())
            {
                List<string> sourceNames = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    sourceNames.Add(reader.GetName(i));
                }

                List<string> cols = new List<string>();
                List<int> ordinals = new List<int>();
                for (int i = 0; i < sourceNames.Count; i++)
                {
                    string match = MatchColumn(destCols, sourceNames[i]);
                    if (match != null)
                    {
                        cols.Add(match);
                        ordinals.Add(i);
                    }
                }
                if (cols.Count == 0)
                {
                    return 0;
                }

                StringBuilder names = new StringBuilder();
                StringBuilder parms = new StringBuilder();
                for (int i = 0; i < cols.Count; i++)
                {
                    if (i > 0)
                    {
                        names.Append(", ");
                        parms.Append(", ");
                    }
                    names.Append("[").Append(cols[i]).Append("]");
                    parms.Append("@p").Append(i);
                }

                using (var insert = dst.CreateCommand())
                {
                    insert.Transaction = tx;
                    insert.CommandText = "INSERT INTO [" + destTable + "] (" + names + ") VALUES (" + parms + ")";
                    for (int i = 0; i < cols.Count; i++)
                    {
                        insert.Parameters.Add(new SQLiteParameter("@p" + i));
                    }
                    while (reader.Read())
                    {
                        for (int i = 0; i < ordinals.Count; i++)
                        {
                            insert.Parameters[i].Value = ToSqlite(reader.GetValue(ordinals[i]));
                        }
                        insert.ExecuteNonQuery();
                        rows++;
                    }
                }
            }
            return rows;
        }

        private static string MatchColumn(HashSet<string> destCols, string sourceName)
        {
            if (destCols.Contains(sourceName))
            {
                return sourceName;
            }
            foreach (string col in destCols)
            {
                if (string.Equals(col, sourceName, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }
            return null;
        }

        private static Dictionary<string, object> ReadRow(IDataRecord reader)
        {
            Dictionary<string, object> row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[reader.GetName(i)] = value;
            }
            return row;
        }

        private static object ToSqlite(object value)
        {
            if (value == null || value is DBNull)
            {
                return DBNull.Value;
            }
            if (value is Guid)
            {
                return ((Guid)value).ToString("D");
            }
            if (value is byte[])
            {
                return value;
            }
            if (value is bool)
            {
                return ((bool)value) ? 1 : 0;
            }
            if (value is DateTime)
            {
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            }
            return value;
        }

        private static void ResetSequences(SQLiteConnection dst, SQLiteTransaction tx, IEnumerable<string> tables)
        {
            foreach (string table in tables)
            {
                string pk = IntegerIdentityColumn(dst, tx, table);
                if (pk == null)
                {
                    continue;
                }
                using (var cmd = dst.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "SELECT IFNULL(MAX([" + pk + "]), 0) FROM [" + table + "]";
                    long max = Convert.ToInt64(cmd.ExecuteScalar());
                    cmd.CommandText = "INSERT OR REPLACE INTO sqlite_sequence(name, seq) VALUES (@n, @s)";
                    cmd.Parameters.AddWithValue("@n", table);
                    cmd.Parameters.AddWithValue("@s", max);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException)
                    {
                        // sqlite_sequence is created on first AUTOINCREMENT insert.
                    }
                }
            }
        }

        private static string IntegerIdentityColumn(SQLiteConnection dst, SQLiteTransaction tx, string table)
        {
            using (var cmd = dst.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name=@n";
                cmd.Parameters.AddWithValue("@n", table);
                object sqlObj = cmd.ExecuteScalar();
                string sql = sqlObj as string;
                if (string.IsNullOrEmpty(sql) || sql.IndexOf("AUTOINCREMENT", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return null;
                }
            }
            using (var cmd = dst.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "PRAGMA table_info([" + table + "])";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (Convert.ToInt32(reader["pk"]) == 1)
                        {
                            return Convert.ToString(reader["name"]);
                        }
                    }
                }
            }
            return null;
        }

        private static HashSet<string> GetSqlCeTables(SqlCeConnection src)
        {
            HashSet<string> tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DataTable schema = src.GetSchema("Tables");
            foreach (DataRow row in schema.Rows)
            {
                if (string.Equals(Convert.ToString(row["TABLE_TYPE"]), "TABLE", StringComparison.OrdinalIgnoreCase))
                {
                    tables.Add(Convert.ToString(row["TABLE_NAME"]));
                }
            }
            return tables;
        }

        private static Dictionary<string, HashSet<string>> GetSqliteTables(SQLiteConnection dst)
        {
            Dictionary<string, HashSet<string>> tables = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = dst.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables[reader.GetString(0)] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            List<string> names = new List<string>(tables.Keys);
            foreach (string name in names)
            {
                using (var cmd = dst.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info([" + name + "])";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables[name].Add(Convert.ToString(reader["name"]));
                        }
                    }
                }
            }
            return tables;
        }

        private static bool IsSqlite(string path)
        {
            byte[] header = new byte[16];
            using (FileStream stream = File.OpenRead(path))
            {
                int read = stream.Read(header, 0, header.Length);
                if (read < 16)
                {
                    return false;
                }
            }
            return Encoding.ASCII.GetString(header).StartsWith("SQLite format 3", StringComparison.Ordinal);
        }

        private static void ListSchema(string path)
        {
            string cs = "Data Source=" + path + ";Max Database Size=4091";
            using (var connection = new SqlCeConnection(cs))
            {
                connection.Open();
                Console.WriteLine("Opened " + path);
                DataTable tables = connection.GetSchema("Tables");
                foreach (DataRow row in tables.Rows)
                {
                    string table = Convert.ToString(row["TABLE_NAME"]);
                    string type = Convert.ToString(row["TABLE_TYPE"]);
                    if (!string.Equals(type, "TABLE", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    Console.WriteLine("TABLE " + table);
                    using (var cmd = new SqlCeCommand("SELECT * FROM [" + table.Replace("]", "]]") + "] WHERE 1=0", connection))
                    using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        DataTable schema = reader.GetSchemaTable();
                        foreach (DataRow col in schema.Rows)
                        {
                            object dataType = col.Table.Columns.Contains("DataTypeName") ? col["DataTypeName"] : col["DataType"];
                            Console.WriteLine("  {0} {1} nullable={2}", col["ColumnName"], dataType, col["AllowDBNull"]);
                        }
                    }
                    using (var count = new SqlCeCommand("SELECT COUNT(*) FROM [" + table.Replace("]", "]]") + "]", connection))
                    {
                        Console.WriteLine("  rows={0}", count.ExecuteScalar());
                    }
                }
            }
        }
    }
}
