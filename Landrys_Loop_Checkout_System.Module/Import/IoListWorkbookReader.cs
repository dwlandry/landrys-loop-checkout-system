using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;

namespace Landrys_Loop_Checkout_System.Module.Import
{
    public static class IoListWorkbookReader
    {
        public static IoListTable Read(string filePath)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                IXLWorksheet sheet = SelectSheet(workbook);
                int headerRow = FindHeaderRow(sheet);
                var columns = ReadHeader(sheet, headerRow);
                if (columns.Count == 0)
                {
                    throw new InvalidOperationException("No header row was found in the first sheet.");
                }

                var rows = new List<IReadOnlyDictionary<string, string>>();
                int lastRow = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
                for (int r = headerRow + 1; r <= lastRow; r++)
                {
                    var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    bool any = false;
                    for (int c = 0; c < columns.Count; c++)
                    {
                        string value = CellText(sheet.Cell(r, c + 1));
                        if (!string.IsNullOrEmpty(value))
                        {
                            any = true;
                        }
                        row[columns[c]] = value;
                    }
                    if (any)
                    {
                        rows.Add(row);
                    }
                }

                return new IoListTable
                {
                    SheetName = sheet.Name,
                    Columns = columns,
                    Rows = rows
                };
            }
        }

        private static IXLWorksheet SelectSheet(XLWorkbook workbook)
        {
            foreach (IXLWorksheet sheet in workbook.Worksheets)
            {
                string name = sheet.Name ?? string.Empty;
                if (name.IndexOf("IO", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Instrument", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Tag", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return sheet;
                }
            }
            return workbook.Worksheets.First();
        }

        private static int FindHeaderRow(IXLWorksheet sheet)
        {
            int lastRow = Math.Min(sheet.LastRowUsed()?.RowNumber() ?? 1, 20);
            for (int r = 1; r <= lastRow; r++)
            {
                var headers = ReadHeader(sheet, r);
                IoListColumnMap guess = IoListColumnMap.Guess(headers);
                if (guess.GetColumn(IoListFields.TagNumber) != null)
                {
                    return r;
                }
            }
            return 1;
        }

        private static List<string> ReadHeader(IXLWorksheet sheet, int row)
        {
            var columns = new List<string>();
            int lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 1; c <= lastCol; c++)
            {
                string name = CellText(sheet.Cell(row, c));
                if (string.IsNullOrEmpty(name))
                {
                    name = "Column " + c;
                }
                string unique = name;
                int suffix = 2;
                while (!usedNames.Add(unique))
                {
                    unique = name + " (" + suffix + ")";
                    suffix++;
                }
                columns.Add(unique);
            }
            return columns;
        }

        private static string CellText(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
            {
                return null;
            }
            string text = cell.GetFormattedString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            return text.Trim();
        }
    }
}
