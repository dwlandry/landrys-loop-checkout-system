using System.Collections.Generic;

namespace Landrys_Loop_Checkout_System.Module.Import
{
    public class IoListTable
    {
        public string SheetName { get; set; }
        public IReadOnlyList<string> Columns { get; set; }
        public IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; set; }

        public string GetValue(IReadOnlyDictionary<string, string> row, IoListColumnMap map, string fieldId)
        {
            string column = map?.GetColumn(fieldId);
            if (column == null || row == null)
            {
                return null;
            }
            if (row.TryGetValue(column, out string value))
            {
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            foreach (var pair in row)
            {
                if (string.Equals(pair.Key, column, System.StringComparison.OrdinalIgnoreCase))
                {
                    return string.IsNullOrWhiteSpace(pair.Value) ? null : pair.Value.Trim();
                }
            }
            return null;
        }
    }
}
