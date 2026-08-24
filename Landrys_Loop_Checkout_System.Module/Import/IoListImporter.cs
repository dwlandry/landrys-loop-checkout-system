using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;

namespace Landrys_Loop_Checkout_System.Module.Import
{
    public class IoListImportResult
    {
        public int InstrumentsCreated { get; set; }
        public int InstrumentsUpdated { get; set; }
        public int LoopsCreated { get; set; }
        public int RowsSkipped { get; set; }
        public int RowsFailed { get; set; }
        public List<string> Errors { get; } = new List<string>();

        public override string ToString()
        {
            string text = string.Format(
                "Created {0} instruments, updated {1}, created {2} loops. Skipped {3} rows.",
                InstrumentsCreated, InstrumentsUpdated, LoopsCreated, RowsSkipped);
            if (RowsFailed > 0)
            {
                text += " Failed " + RowsFailed + " rows.";
            }
            if (Errors.Count > 0)
            {
                text += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, Errors);
            }
            return text;
        }
    }

    public static class IoListImporter
    {
        public static IoListImportResult Import(IObjectSpace objectSpace, IoListTable table, IoListColumnMap map)
        {
            var result = new IoListImportResult();
            if (table == null || map == null || map.GetColumn(IoListFields.TagNumber) == null)
            {
                result.RowsFailed = 1;
                result.Errors.Add("Tag Number must be mapped to a column.");
                return result;
            }

            var loops = IndexBy(objectSpace.GetObjects<Loop>(), loop => loop.LoopNumber);
            var instruments = IndexBy(objectSpace.GetObjects<Instrument>(), instrument => instrument.TagNumber);
            var ioTypes = IndexBy(objectSpace.GetObjects<IOType>(), item => item.Name);
            var areas = IndexBy(objectSpace.GetObjects<Area>(), item => item.Name);
            var pids = IndexBy(objectSpace.GetObjects<PID>(), item => item.Number);
            var boxes = IndexBy(objectSpace.GetObjects<JunctionBox>(), item => item.Number);
            var plans = IndexBy(objectSpace.GetObjects<LocationPlan>(), item => item.Number);
            var drawings = IndexBy(objectSpace.GetObjects<LoopDrawing>(), item => item.Number);
            var systems = IndexBy(objectSpace.GetObjects<ControlSystem>(), item => item.Name);
            var types = IndexBy(objectSpace.GetObjects<InstrumentType>(), item => item.Name);
            var companies = IndexBy(objectSpace.GetObjects<Company>(), item => item.Name);

            LoopCheckStatus defaultStatus = null;
            foreach (LoopCheckStatus status in objectSpace.GetObjects<LoopCheckStatus>())
            {
                if (string.Equals(status.Description, "Not Ready for Check", StringComparison.OrdinalIgnoreCase))
                {
                    defaultStatus = status;
                    break;
                }
            }

            int rowNumber = 1;
            foreach (var row in table.Rows)
            {
                rowNumber++;
                try
                {
                    string tag = table.GetValue(row, map, IoListFields.TagNumber);
                    if (string.IsNullOrEmpty(tag))
                    {
                        result.RowsSkipped++;
                        continue;
                    }

                    bool created;
                    Instrument instrument = GetOrCreate(instruments, tag, () => objectSpace.CreateObject<Instrument>(), out created);
                    instrument.TagNumber = tag;
                    instrument.ServiceDescription = Coalesce(table.GetValue(row, map, IoListFields.ServiceDescription), instrument.ServiceDescription);
                    instrument.Calibration = Coalesce(table.GetValue(row, map, IoListFields.Calibration), instrument.Calibration);

                    string loopNumber = table.GetValue(row, map, IoListFields.LoopNumber);
                    if (!string.IsNullOrEmpty(loopNumber))
                    {
                        bool loopCreated;
                        Loop loop = GetOrCreate(loops, loopNumber, () => objectSpace.CreateObject<Loop>(), out loopCreated);
                        loop.LoopNumber = loopNumber;
                        loop.Description = Coalesce(table.GetValue(row, map, IoListFields.LoopDescription), loop.Description);
                        if (loopCreated)
                        {
                            loop.LoopCheckStatus = defaultStatus;
                            result.LoopsCreated++;
                        }

                        string areaNames = table.GetValue(row, map, IoListFields.Area);
                        if (!string.IsNullOrEmpty(areaNames))
                        {
                            foreach (string areaName in SplitNames(areaNames))
                            {
                                Area area = GetOrCreate(areas, areaName, () => objectSpace.CreateObject<Area>(), out _);
                                area.Name = areaName;
                                AddArea(loop, area);
                            }
                        }

                        string provider = table.GetValue(row, map, IoListFields.LoopProvider);
                        if (!string.IsNullOrEmpty(provider))
                        {
                            loop.LoopProvider = GetOrCreate(companies, provider, () => objectSpace.CreateObject<Company>(), out _);
                            loop.LoopProvider.Name = provider;
                        }

                        instrument.Loop = loop;
                    }

                    SetNamed(ioTypes, table.GetValue(row, map, IoListFields.IOType), () => objectSpace.CreateObject<IOType>(),
                        createdType => createdType.Name = table.GetValue(row, map, IoListFields.IOType),
                        value => instrument.IOType = value);
                    SetNamed(pids, table.GetValue(row, map, IoListFields.PID), () => objectSpace.CreateObject<PID>(),
                        createdPid => createdPid.Number = table.GetValue(row, map, IoListFields.PID),
                        value => instrument.PID = value);
                    SetNamed(boxes, table.GetValue(row, map, IoListFields.JunctionBox), () => objectSpace.CreateObject<JunctionBox>(),
                        createdBox => createdBox.Number = table.GetValue(row, map, IoListFields.JunctionBox),
                        value => instrument.JunctionBox = value);
                    SetNamed(plans, table.GetValue(row, map, IoListFields.LocationPlan), () => objectSpace.CreateObject<LocationPlan>(),
                        createdPlan => createdPlan.Number = table.GetValue(row, map, IoListFields.LocationPlan),
                        value => instrument.LocationPlan = value);
                    SetNamed(drawings, table.GetValue(row, map, IoListFields.LoopDrawing), () => objectSpace.CreateObject<LoopDrawing>(),
                        createdDrawing => createdDrawing.Number = table.GetValue(row, map, IoListFields.LoopDrawing),
                        value => instrument.LoopDrawing = value);
                    SetNamed(systems, table.GetValue(row, map, IoListFields.ControlSystem), () => objectSpace.CreateObject<ControlSystem>(),
                        createdSystem => createdSystem.Name = table.GetValue(row, map, IoListFields.ControlSystem),
                        value => instrument.ControlSystem = value);
                    SetNamed(types, table.GetValue(row, map, IoListFields.InstrumentType), () => objectSpace.CreateObject<InstrumentType>(),
                        createdType => createdType.Name = table.GetValue(row, map, IoListFields.InstrumentType),
                        value => instrument.InstrumentType = value);

                    string companyName = table.GetValue(row, map, IoListFields.ResponsibleCompany);
                    if (!string.IsNullOrEmpty(companyName))
                    {
                        instrument.ResponsibleCompany = GetOrCreate(companies, companyName, () => objectSpace.CreateObject<Company>(), out _);
                        instrument.ResponsibleCompany.Name = companyName;
                    }

                    if (created)
                    {
                        result.InstrumentsCreated++;
                    }
                    else
                    {
                        result.InstrumentsUpdated++;
                    }
                }
                catch (Exception ex)
                {
                    result.RowsFailed++;
                    if (result.Errors.Count < 20)
                    {
                        result.Errors.Add("Row " + rowNumber + ": " + ex.Message);
                    }
                }
            }

            objectSpace.CommitChanges();
            return result;
        }

        private static Dictionary<string, T> IndexBy<T>(IEnumerable<T> items, Func<T, string> keySelector)
        {
            var index = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (T item in items)
            {
                string key = keySelector(item);
                if (!string.IsNullOrWhiteSpace(key) && !index.ContainsKey(key))
                {
                    index[key] = item;
                }
            }
            return index;
        }

        private static T GetOrCreate<T>(Dictionary<string, T> index, string key, Func<T> create, out bool created)
        {
            if (index.TryGetValue(key, out T existing))
            {
                created = false;
                return existing;
            }
            T item = create();
            index[key] = item;
            created = true;
            return item;
        }

        private static void SetNamed<T>(Dictionary<string, T> index, string key, Func<T> create, Action<T> initialize, Action<T> assign)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            T item = GetOrCreate(index, key, create, out bool created);
            if (created)
            {
                initialize(item);
            }
            assign(item);
        }

        private static void AddArea(Loop loop, Area area)
        {
            foreach (Area existing in loop.Areas)
            {
                if (existing == area)
                {
                    return;
                }
            }
            loop.Areas.Add(area);
        }

        private static IEnumerable<string> SplitNames(string value)
        {
            foreach (string part in value.Split(new[] { ';', ',', '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string name = part.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    yield return name;
                }
            }
        }

        private static string Coalesce(string incoming, string existing)
        {
            return string.IsNullOrEmpty(incoming) ? existing : incoming;
        }
    }
}
