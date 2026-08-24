# Landry's Loop Checkout System

Windows desktop app for tracking instrumentation loop checkout on industrial jobs: loops, I/O tags, drawings, status, and check schedules.

It is a DevExpress **XAF WinForms** app on **.NET 8** and **DevExpress 26.1.3**. Each job is a branded `*.llcs` file (SQLite).

## Requirements

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (or `dotnet build`)
- DevExpress 26.1.3 XAF WinForms. The license belongs on the machine at `%AppData%\DevExpress\DevExpress_License.txt` — do not put it in this repository.

NuGet sources are in `NuGet.config` (nuget.org plus the local DevExpress 26.1 offline folder).

## Build and run

Close the running app before you rebuild; it locks its DLLs.

```powershell
dotnet build "Landrys Loop Checkout System.sln" -c Debug
.\Landrys_Loop_Checkout_System.Win\bin\Debug\net8.0-windows\Landrys_Loop_Checkout_System.Win.exe
```

Sign-in uses Windows Active Directory. The first run creates your user and assigns the Administrators role.

## Job files

| Item | Detail |
|---|---|
| Format | `*.llcs` (SQLite) |
| Default new job | `%LocalAppData%\LandrysLoopCheckout\LandrysLoopCheckout.llcs` |
| Open / create | **File → Open Job…** / **File → Create New Job…** |

The first launch registers `.llcs` for the current Windows user so double-clicking a job file opens it.

Converted sample jobs live in `Datafile\`. `15-1516_Loop Check.llcs` is the Isocracker job.

## I/O list import

**File → Import I/O List…** reads an `.xlsx` workbook, maps columns, and upserts loops/instruments by tag and loop number.

- Sample workbook: `Landrys_Loop_Checkout_System.Win\SampleIoList.xlsx`
- Remembered mapping: `%LocalAppData%\LandrysLoopCheckout\io-import-mapping.json`

## Old SQL Compact jobs

Jobs from the .NET Framework era were SQL Compact, not SQLite. Convert a folder with `tools\LlcsConvert` (.NET Framework 4.8, 32-bit):

```powershell
dotnet build tools\LlcsConvert\LlcsConvert.csproj -c Release
.\tools\LlcsConvert\bin\Release\net48\LlcsConvert.exe convert-dir .\Datafile
```

Originals are copied to `sqlce-backup` next to the files (that folder is gitignored). Already-SQLite files are skipped.

## Solution layout

| Project | Target | Role |
|---|---|---|
| `Landrys_Loop_Checkout_System.Module` | `net8.0` | Business objects, reports, I/O import |
| `Landrys_Loop_Checkout_System.Module.Win` | `net8.0-windows` | WinForms controllers and dialogs |
| `Landrys_Loop_Checkout_System.Win` | `net8.0-windows` | Application host |

`tools\LlcsConvert` is a separate console tool and is not in the main solution.
