using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Landrys_Loop_Checkout_System.Module.Import;

namespace Landrys_Loop_Checkout_System.Module.Win.Forms
{
    public class ImportIoListForm : XtraForm
    {
        private readonly IoListTable _table;
        private readonly Dictionary<string, ComboBoxEdit> _combos = new Dictionary<string, ComboBoxEdit>();
        private Panel _additionalPanel;
        private DataGridView _preview;
        private LabelControl _warning;
        private SimpleButton _importButton;
        private bool _updating;

        public IoListColumnMap Map { get; private set; }

        public ImportIoListForm(IoListTable table, IoListColumnMap map)
        {
            _table = table;
            Map = map ?? new IoListColumnMap();

            Text = "Import I/O List";
            Width = 760;
            Height = 680;
            MinimumSize = new Size(640, 520);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            Padding = new Padding(12);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 280));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var summary = new LabelControl
            {
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 8),
                Text = string.Format(
                    "{0} rows from sheet \"{1}\". Existing tags and loops are updated instead of duplicated.",
                    table.Rows.Count,
                    table.SheetName)
            };

            _warning = new LabelControl
            {
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.FromArgb(180, 80, 0) },
                Padding = new Padding(0, 0, 0, 8),
                Visible = false
            };

            var mappingGroup = new GroupControl
            {
                Text = "Column mapping",
                Dock = DockStyle.Fill
            };
            var mappingBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                ColumnCount = 1,
                Padding = new Padding(10, 8, 10, 8)
            };
            mappingBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mappingBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mappingBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var primaryPanel = CreateFieldTable(IoListFields.All.Where(f => f.Primary));
            _additionalPanel = new Panel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Visible = false,
                AutoScroll = true,
                Padding = new Padding(0, 8, 0, 0)
            };
            _additionalPanel.Controls.Add(CreateFieldTable(IoListFields.All.Where(f => !f.Primary)));

            var more = new CheckEdit
            {
                Text = "Show additional fields",
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 8, 0, 0)
            };
            more.CheckedChanged += (s, e) =>
            {
                _additionalPanel.Visible = more.Checked;
                root.RowStyles[2].Height = more.Checked ? 520 : 280;
            };

            mappingBody.Controls.Add(primaryPanel);
            mappingBody.Controls.Add(more);
            mappingBody.Controls.Add(_additionalPanel);
            mappingGroup.Controls.Add(mappingBody);

            var previewGroup = new GroupControl
            {
                Text = "Preview",
                Dock = DockStyle.Fill
            };
            _preview = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            _preview.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            _preview.ColumnHeadersDefaultCellStyle.Font = new Font(_preview.Font, FontStyle.Bold);
            previewGroup.Controls.Add(_preview);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(0, 12, 0, 0),
                WrapContents = false
            };
            _importButton = new SimpleButton { Text = "Import", Width = 90, Height = 28 };
            var cancel = new SimpleButton { Text = "Cancel", Width = 90, Height = 28, DialogResult = DialogResult.Cancel };
            _importButton.Click += ImportButton_Click;
            buttons.Controls.Add(_importButton);
            buttons.Controls.Add(cancel);
            AcceptButton = _importButton;
            CancelButton = cancel;

            root.Controls.Add(summary, 0, 0);
            root.Controls.Add(_warning, 0, 1);
            root.Controls.Add(mappingGroup, 0, 2);
            root.Controls.Add(previewGroup, 0, 3);
            root.Controls.Add(buttons, 0, 4);
            Controls.Add(root);

            ApplyMapToCombos();
            UpdatePreview();
        }

        private TableLayoutPanel CreateFieldTable(IEnumerable<IoListField> fields)
        {
            var table = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                ColumnCount = 2,
                Padding = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var choices = new List<string> { IoListColumnMap.None };
            choices.AddRange(_table.Columns);

            int row = 0;
            foreach (IoListField field in fields)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                var label = new LabelControl
                {
                    Text = field.Caption + (field.Required ? " *" : ""),
                    AutoSizeMode = LabelAutoSizeMode.Default,
                    Anchor = AnchorStyles.Left
                };
                var combo = new ComboBoxEdit
                {
                    Dock = DockStyle.Fill,
                    Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
                };
                combo.Properties.Items.AddRange(choices.ToArray());
                combo.EditValueChanged += (s, e) => OnMappingChanged(field.Id);
                _combos[field.Id] = combo;
                table.Controls.Add(label, 0, row);
                table.Controls.Add(combo, 1, row);
                row++;
            }
            return table;
        }

        private void ApplyMapToCombos()
        {
            _updating = true;
            try
            {
                foreach (var pair in _combos)
                {
                    string selected = Map.GetColumn(pair.Key);
                    pair.Value.EditValue = selected != null && _table.Columns.Any(c => string.Equals(c, selected, StringComparison.OrdinalIgnoreCase))
                        ? selected
                        : IoListColumnMap.None;
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private void OnMappingChanged(string fieldId)
        {
            if (_updating)
            {
                return;
            }

            string column = ComboValue(_combos[fieldId]);
            if (!string.IsNullOrEmpty(column))
            {
                _updating = true;
                try
                {
                    foreach (var pair in _combos)
                    {
                        if (pair.Key == fieldId)
                        {
                            continue;
                        }
                        if (string.Equals(ComboValue(pair.Value), column, StringComparison.OrdinalIgnoreCase))
                        {
                            pair.Value.EditValue = IoListColumnMap.None;
                        }
                    }
                }
                finally
                {
                    _updating = false;
                }
            }

            UpdatePreview();
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            CaptureMap();
            if (Map.GetColumn(IoListFields.TagNumber) == null)
            {
                XtraMessageBox.Show(this, "Map Tag Number before importing.", Text);
                return;
            }
            var duplicates = Map.DuplicateSourceColumns();
            if (duplicates.Count > 0)
            {
                XtraMessageBox.Show(this, "Each Excel column can map to only one field. \"" + duplicates[0] + "\" is used more than once.", Text);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CaptureMap()
        {
            Map = new IoListColumnMap();
            foreach (var pair in _combos)
            {
                Map.SetColumn(pair.Key, ComboValue(pair.Value));
            }
        }

        private static string ComboValue(ComboBoxEdit combo)
        {
            string value = combo.EditValue as string;
            if (string.IsNullOrWhiteSpace(value) || value == IoListColumnMap.None)
            {
                return null;
            }
            return value;
        }

        private void UpdatePreview()
        {
            CaptureMap();
            _preview.Columns.Clear();
            _preview.Rows.Clear();

            var previewFields = IoListFields.All.Where(f => f.Primary || Map.GetColumn(f.Id) != null).ToList();
            foreach (IoListField field in previewFields)
            {
                _preview.Columns.Add(field.Id, field.Caption);
            }

            foreach (var row in _table.Rows.Take(20))
            {
                object[] values = previewFields.Select(f => (object)_table.GetValue(row, Map, f.Id)).ToArray();
                _preview.Rows.Add(values);
            }

            _importButton.Enabled = Map.GetColumn(IoListFields.TagNumber) != null;
            var duplicates = Map.DuplicateSourceColumns();
            if (duplicates.Count > 0)
            {
                _warning.Text = "\"" + duplicates[0] + "\" is mapped to more than one field. Each Excel column can only fill one destination.";
                _warning.Visible = true;
            }
            else
            {
                _warning.Visible = false;
            }
        }
    }
}
