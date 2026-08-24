using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Landrys_Loop_Checkout_System.Module.Import
{
    public class IoListField
    {
        public string Id { get; }
        public string Caption { get; }
        public bool Required { get; }
        public bool Primary { get; }
        public string[] Aliases { get; }

        public IoListField(string id, string caption, bool required, bool primary, params string[] aliases)
        {
            Id = id;
            Caption = caption;
            Required = required;
            Primary = primary;
            Aliases = aliases.OrderByDescending(a => a.Length).ToArray();
        }
    }

    public static class IoListFields
    {
        public const string TagNumber = "TagNumber";
        public const string LoopNumber = "LoopNumber";
        public const string LoopDescription = "LoopDescription";
        public const string ServiceDescription = "ServiceDescription";
        public const string Calibration = "Calibration";
        public const string IOType = "IOType";
        public const string Area = "Area";
        public const string PID = "PID";
        public const string JunctionBox = "JunctionBox";
        public const string LocationPlan = "LocationPlan";
        public const string LoopDrawing = "LoopDrawing";
        public const string ControlSystem = "ControlSystem";
        public const string InstrumentType = "InstrumentType";
        public const string ResponsibleCompany = "ResponsibleCompany";
        public const string LoopProvider = "LoopProvider";

        public static readonly IReadOnlyList<IoListField> All = new[]
        {
            new IoListField(TagNumber, "Tag Number", true, true,
                "tag number", "tag no", "instrument tag", "io tag", "i/o tag", "tag"),
            new IoListField(LoopNumber, "Loop Number", false, true,
                "loop number", "loop no", "loop"),
            new IoListField(IOType, "I/O Type", false, true,
                "io type", "i/o type", "iotype", "signal type"),
            new IoListField(ServiceDescription, "Description", false, true,
                "service description", "instrument description", "description"),
            new IoListField(PID, "P&ID", false, true,
                "p&id number", "p&id", "p & id", "pid number", "pid"),
            new IoListField(Area, "Area", false, true,
                "plant area", "area"),
            new IoListField(JunctionBox, "Junction Box", false, true,
                "junction box", "j-box", "jb number", "jbox", "jb"),
            new IoListField(LoopDescription, "Loop Description", false, false,
                "loop description", "loop desc"),
            new IoListField(Calibration, "Calibration", false, false,
                "calibration", "cal range", "range"),
            new IoListField(LocationPlan, "Location Plan", false, false,
                "location plan", "loc plan", "location drawing"),
            new IoListField(LoopDrawing, "Loop Drawing", false, false,
                "loop drawing", "loop dwg", "loop diagram"),
            new IoListField(ControlSystem, "Control System", false, false,
                "control system"),
            new IoListField(InstrumentType, "Instrument Type", false, false,
                "instrument type", "inst type", "isa type"),
            new IoListField(ResponsibleCompany, "Responsible Company", false, false,
                "responsible company", "contractor"),
            new IoListField(LoopProvider, "Loop Provider", false, false,
                "loop provider", "company providing loop"),
        };
    }

    public class IoListColumnMap
    {
        public const string None = "(none)";

        public Dictionary<string, string> Columns { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string GetColumn(string fieldId)
        {
            if (Columns != null && Columns.TryGetValue(fieldId, out string column) && !string.IsNullOrWhiteSpace(column))
            {
                return column;
            }
            return null;
        }

        public void SetColumn(string fieldId, string column)
        {
            if (string.IsNullOrWhiteSpace(column) || column == None)
            {
                Columns.Remove(fieldId);
            }
            else
            {
                Columns[fieldId] = column;
            }
        }

        public IReadOnlyList<string> DuplicateSourceColumns()
        {
            return Columns.Values
                .GroupBy(v => v, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
        }

        public static IoListColumnMap Guess(IEnumerable<string> headers)
        {
            var map = new IoListColumnMap();
            var remaining = headers
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (IoListField field in IoListFields.All)
            {
                string match = FindHeader(remaining, field);
                if (match != null)
                {
                    map.SetColumn(field.Id, match);
                    remaining.RemoveAll(h => string.Equals(h, match, StringComparison.OrdinalIgnoreCase));
                }
            }
            return map;
        }

        public static IoListColumnMap Merge(IoListColumnMap saved, IEnumerable<string> headers)
        {
            var available = new HashSet<string>(
                headers.Where(h => !string.IsNullOrWhiteSpace(h)).Select(h => h.Trim()),
                StringComparer.OrdinalIgnoreCase);
            var guessed = Guess(available);
            var merged = new IoListColumnMap();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (IoListField field in IoListFields.All)
            {
                string savedColumn = saved?.GetColumn(field.Id);
                string guessedColumn = guessed.GetColumn(field.Id);
                string chosen = null;
                if (savedColumn != null && available.Contains(savedColumn) && used.Add(savedColumn))
                {
                    chosen = savedColumn;
                }
                else if (guessedColumn != null && used.Add(guessedColumn))
                {
                    chosen = guessedColumn;
                }
                if (chosen != null)
                {
                    merged.SetColumn(field.Id, chosen);
                }
            }
            return merged;
        }

        public static string MappingFilePath
        {
            get { return Path.Combine(JobDatabase.AppDataFolder, "io-import-mapping.json"); }
        }

        public static IoListColumnMap LoadSaved()
        {
            try
            {
                string path = MappingFilePath;
                if (!File.Exists(path))
                {
                    return new IoListColumnMap();
                }
                return JsonSerializer.Deserialize<IoListColumnMap>(File.ReadAllText(path)) ?? new IoListColumnMap();
            }
            catch
            {
                return new IoListColumnMap();
            }
        }

        public void Save()
        {
            string path = MappingFilePath;
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static string FindHeader(List<string> headers, IoListField field)
        {
            foreach (string alias in field.Aliases)
            {
                string exact = headers.FirstOrDefault(h => Normalize(h) == Normalize(alias));
                if (exact != null)
                {
                    return exact;
                }
            }
            foreach (string alias in field.Aliases)
            {
                string[] aliasTokens = Tokens(alias);
                if (aliasTokens.Length < 2)
                {
                    continue;
                }
                string phrase = headers.FirstOrDefault(h => TokensMatch(Tokens(h), aliasTokens));
                if (phrase != null)
                {
                    return phrase;
                }
            }
            return null;
        }

        private static bool TokensMatch(string[] headerTokens, string[] aliasTokens)
        {
            if (headerTokens.Length < aliasTokens.Length)
            {
                return false;
            }
            for (int i = 0; i <= headerTokens.Length - aliasTokens.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < aliasTokens.Length; j++)
                {
                    if (headerTokens[i + j] != aliasTokens[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return true;
                }
            }
            return false;
        }

        private static string[] Tokens(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }
            return value
                .ToLowerInvariant()
                .Split(new[] { ' ', '\t', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Normalize)
                .Where(t => t.Length > 0)
                .ToArray();
        }

        internal static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            return new string(value.Trim().ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == '/' || ch == '&').ToArray());
        }
    }
}
