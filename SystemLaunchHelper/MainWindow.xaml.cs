﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Path = System.IO.Path;

namespace SystemLaunchHelper
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExecutionCoreFunction();
        }

        private void ExecutionCoreFunction()
        {
            int ExitCode = 0;

            try
            {
                IEnumerable<string> ActivationArgs = Environment.GetCommandLineArgs().Skip(1);

                if (ActivationArgs.FirstOrDefault() == "-Command")
                {
                    switch (ActivationArgs.LastOrDefault())
                    {
                        case "InterceptWinE":
                            {
                                using (Process CurrentProcess = Process.GetCurrentProcess())
                                {
                                    string CurrentPath = CurrentProcess.MainModule.FileName;
                                    string TempFilePath = Path.Combine(Path.GetTempPath(), @$"{Guid.NewGuid()}.reg");

                                    try
                                    {
                                        using (FileStream TempFileStream = File.Open(TempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                        using (FileStream RegStream = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RegFiles\Intercept_WIN_E.reg"), FileMode.Open, FileAccess.Read, FileShare.Read))
                                        using (StreamReader Reader = new StreamReader(RegStream))
                                        {
                                            string Content = Reader.ReadToEnd();

                                            using (StreamWriter Writer = new StreamWriter(TempFileStream, Encoding.Unicode))
                                            {
                                                Writer.Write(Content.Replace("<FillActualAliasPathInHere>", $"{CurrentPath.Replace(@"\", @"\\")} %1"));
                                            }
                                        }

                                        using (Process RegisterProcess = Process.Start(new ProcessStartInfo
                                        {
                                            FileName = "regedit.exe",
                                            Verb = "runas",
                                            CreateNoWindow = true,
                                            UseShellExecute = true,
                                            Arguments = $"/s \"{TempFilePath}\"",
                                        }))
                                        {
                                            RegisterProcess.WaitForExit();
                                        }
                                    }
                                    finally
                                    {
                                        if (File.Exists(TempFilePath))
                                        {
                                            File.Delete(TempFilePath);
                                        }
                                    }

                                    bool IsRegistryCheckingSuccess = true;

                                    try
                                    {
                                        using (RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Folder", false)?.OpenSubKey("shell", false)?.OpenSubKey("opennewwindow", false)?.OpenSubKey("command", false))
                                        {
                                            if (Key != null)
                                            {
                                                if (!Convert.ToString(Key.GetValue(string.Empty)).Equals($"{CurrentPath} %1", StringComparison.OrdinalIgnoreCase) || Key.GetValue("DelegateExecute") != null)
                                                {
                                                    IsRegistryCheckingSuccess = false;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
#if DEBUG
                                        if (Debugger.IsAttached)
                                        {
                                            Debugger.Break();
                                        }
                                        else
                                        {
                                            Debugger.Launch();
                                        }

                                        Debug.WriteLine($"Registry checking failed, message: {ex.Message}");
#endif
                                    }

                                    if (!IsRegistryCheckingSuccess)
                                    {
                                        ExitCode = 2;
                                    }
                                }

                                break;
                            }
                        case "InterceptFolder":
                            {
                                using (Process CurrentProcess = Process.GetCurrentProcess())
                                {
                                    string CurrentPath = CurrentProcess.MainModule.FileName;
                                    string TempFilePath = Path.Combine(Path.GetTempPath(), @$"{Guid.NewGuid()}.reg");

                                    try
                                    {
                                        using (FileStream TempFileStream = File.Open(TempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                        using (FileStream RegStream = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RegFiles\Intercept_Folder.reg"), FileMode.Open, FileAccess.Read, FileShare.Read))
                                        using (StreamReader Reader = new StreamReader(RegStream))
                                        {
                                            string Content = Reader.ReadToEnd();

                                            using (StreamWriter Writer = new StreamWriter(TempFileStream, Encoding.Unicode))
                                            {
                                                Writer.Write(Content.Replace("<FillActualAliasPathInHere>", $"{CurrentPath.Replace(@"\", @"\\")} %1"));
                                            }
                                        }

                                        using (Process RegisterProcess = Process.Start(new ProcessStartInfo
                                        {
                                            FileName = "regedit.exe",
                                            Verb = "runas",
                                            CreateNoWindow = true,
                                            UseShellExecute = true,
                                            Arguments = $"/s \"{TempFilePath}\"",
                                        }))
                                        {
                                            RegisterProcess.WaitForExit();
                                        }
                                    }
                                    finally
                                    {
                                        if (File.Exists(TempFilePath))
                                        {
                                            File.Delete(TempFilePath);
                                        }
                                    }

                                    bool IsRegistryCheckingSuccess = true;

                                    try
                                    {
                                        using (RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Directory", false)?.OpenSubKey("shell", false)?.OpenSubKey("open", false)?.OpenSubKey("command", false))
                                        {
                                            if (Key != null)
                                            {
                                                if (!Convert.ToString(Key.GetValue(string.Empty)).Equals($"{CurrentPath} %1", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    IsRegistryCheckingSuccess = false;
                                                }
                                            }
                                        }

                                        using (RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Drive", false)?.OpenSubKey("shell", false)?.OpenSubKey("open", false)?.OpenSubKey("command", false))
                                        {
                                            if (Key != null)
                                            {
                                                if (!Convert.ToString(Key.GetValue(string.Empty)).Equals($"{CurrentPath} %1", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    IsRegistryCheckingSuccess = false;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
#if DEBUG
                                        if (Debugger.IsAttached)
                                        {
                                            Debugger.Break();
                                        }
                                        else
                                        {
                                            Debugger.Launch();
                                        }

                                        Debug.WriteLine($"Registry checking failed, message: {ex.Message}");
#endif                                    
                                    }

                                    if (!IsRegistryCheckingSuccess)
                                    {
                                        ExitCode = 1;
                                    }
                                }

                                break;
                            }
                        case "RestoreWinE":
                            {
                                using (Process RegisterProcess = Process.Start(new ProcessStartInfo
                                {
                                    FileName = "regedit.exe",
                                    Verb = "runas",
                                    CreateNoWindow = true,
                                    UseShellExecute = true,
                                    Arguments = $"/s \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RegFiles\Restore_WIN_E.reg")}\"",
                                }))
                                {
                                    RegisterProcess.WaitForExit();
                                }

                                bool IsRegistryCheckingSuccess = true;

                                try
                                {
                                    using (RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Folder", false)?.OpenSubKey("shell", false)?.OpenSubKey("opennewwindow", false)?.OpenSubKey("command", false))
                                    {
                                        if (Key != null)
                                        {
                                            if (Convert.ToString(Key.GetValue("DelegateExecute")) != "{11dbb47c-a525-400b-9e80-a54615a090c0}" || !string.IsNullOrEmpty(Convert.ToString(Key.GetValue(string.Empty))))
                                            {
                                                IsRegistryCheckingSuccess = false;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
#if DEBUG
                                    if (Debugger.IsAttached)
                                    {
                                        Debugger.Break();
                                    }
                                    else
                                    {
                                        Debugger.Launch();
                                    }

                                    Debug.WriteLine($"Registry checking failed, message: {ex.Message}");
#endif
                                }

                                if (!IsRegistryCheckingSuccess)
                                {
                                    ExitCode = 1;
                                }

                                break;
                            }
                        case "RestoreFolder":
                            {
                                using (Process RegisterProcess = Process.Start(new ProcessStartInfo
                                {
                                    FileName = "regedit.exe",
                                    Verb = "runas",
                                    CreateNoWindow = true,
                                    UseShellExecute = true,
                                    Arguments = $"/s \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RegFiles\Restore_Folder.reg")}\"",
                                }))
                                {
                                    RegisterProcess.WaitForExit();
                                }

                                bool IsRegistryCheckingSuccess = true;

                                try
                                {
                                    using (RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Folder", false)?.OpenSubKey("Directory", false)?.OpenSubKey("open", false)?.OpenSubKey("command", false))
                                    {
                                        if (Key != null)
                                        {
                                            if (Convert.ToString(Key.GetValue("DelegateExecute")) != "{11dbb47c-a525-400b-9e80-a54615a090c0}" || !string.IsNullOrEmpty(Convert.ToString(Key.GetValue(string.Empty))))
                                            {
                                                IsRegistryCheckingSuccess = false;
                                            }
                                        }
                                    }

                                    using (RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Drive", false)?.OpenSubKey("shell", false)?.OpenSubKey("open", false)?.OpenSubKey("command", false))
                                    {
                                        if (Key != null)
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(Key.GetValue(string.Empty))))
                                            {
                                                IsRegistryCheckingSuccess = false;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
#if DEBUG
                                    if (Debugger.IsAttached)
                                    {
                                        Debugger.Break();
                                    }
                                    else
                                    {
                                        Debugger.Launch();
                                    }

                                    Debug.WriteLine($"Registry checking failed, message: {ex.Message}");
#endif
                                }

                                if (!IsRegistryCheckingSuccess)
                                {
                                    ExitCode = 1;
                                }

                                break;
                            }
                    }
                }
                else
                {
                    string AliasLocation = string.Empty;
                    string TargetPath = ActivationArgs.FirstOrDefault() ?? string.Empty;

                    try
                    {
                        using (Process Pro = Process.Start(new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = "-Command \"Get-Command RX-Explorer | Format-List -Property Source\"",
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        }))
                        {
                            try
                            {
                                string OutputString = Pro.StandardOutput.ReadToEnd();

                                if (!string.IsNullOrWhiteSpace(OutputString))
                                {
                                    string Path = OutputString.Replace(Environment.NewLine, string.Empty).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                                    if (File.Exists(Path))
                                    {
                                        AliasLocation = Path;
                                    }
                                }
                            }
                            finally
                            {
                                if (!Pro.WaitForExit(5000))
                                {
                                    Pro.Kill();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                        else
                        {
                            Debugger.Launch();
                        }

                        Debug.WriteLine($"Could not get alias location by Powershell, message: {ex.Message}");
#endif
                    }

                    if (string.IsNullOrEmpty(AliasLocation))
                    {
                        string[] EnvironmentVariables = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User)
                                                                   .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                                                   .Concat(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine)
                                                                                      .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                                                                   .Distinct()
                                                                   .ToArray();

                        if (EnvironmentVariables.Where((Var) => Var.Contains("WindowsApps")).Select((Var) => Path.Combine(Var, "RX-Explorer.exe")).FirstOrDefault((Path) => File.Exists(Path)) is string Location)
                        {
                            AliasLocation = Location;
                        }
                        else
                        {
                            string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                            if (!string.IsNullOrEmpty(AppDataPath) && Directory.Exists(AppDataPath))
                            {
                                string WindowsAppsPath = Path.Combine(AppDataPath, "Microsoft", "WindowsApps");

                                if (Directory.Exists(WindowsAppsPath))
                                {
                                    string RXPath = Path.Combine(WindowsAppsPath, "RX-Explorer.exe");

                                    if (File.Exists(RXPath))
                                    {
                                        AliasLocation = RXPath;
                                    }
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(AliasLocation))
                    {
                        string TipText = CultureInfo.CurrentCulture.Name switch
                        {
                            "zh-Hans" => "检测到RX文件管理器已被卸载，但卸载前已启用与系统集成相关功能，这可能导致Windows文件管理器无法正常使用，您是否希望还原为默认设置?",
                            "zh-Hant" => "檢測到RX档案總管已卸載，但卸載前已啟用系統集成相關功能，可能會破壞Windows資源管理器。 您想恢復到默認設置嗎?",
                            "fr-FR" => "Il est détecté que le RX-Explorer a été désinstallé, mais les fonctions liées à l'intégration du système ont été activées avant la désinstallation, ce qui peut casser l'Explorateur Windows. Voulez-vous restaurer les paramètres par défaut?",
                            "es" => "Se detecta que el RX-Explorer se ha desinstalado, pero las funciones relacionadas con la integración del sistema se han habilitado antes de la desinstalación, lo que puede dañar el Explorador de Windows. ¿Desea restaurar la configuración predeterminada?",
                            "de-DE" => "Es wurde festgestellt, dass der RX-Explorer deinstalliert wurde, aber die systemintegrationsbezogenen Funktionen vor der Deinstallation aktiviert wurden, was den Windows Explorer beschädigen kann. Möchten Sie die Standardeinstellungen wiederherstellen?",
                            _ => "It is detected that the RX-Explorer has been uninstalled, but the system integration-related functions have been enabled before uninstalling, which may broken the Windows Explorer. Do you want to restore to the default settings?"
                        };

                        string TipHeader = CultureInfo.CurrentCulture.Name switch
                        {
                            "zh-Hans" => "警告",
                            "zh-Hant" => "警告",
                            "fr-FR" => "Avertissement",
                            "es" => "Advertencia",
                            "de-DE" => "Warnung",
                            _ => "Warning"
                        };

                        if (MessageBox.Show(TipText, TipHeader, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        {
                            using (Process RegisterProcess = Process.Start(new ProcessStartInfo
                            {
                                FileName = "regedit.exe",
                                Verb = "runas",
                                CreateNoWindow = true,
                                UseShellExecute = true,
                                Arguments = $"/s \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RegFiles\RestoreAll.reg")}\"",
                            }))
                            {
                                RegisterProcess.WaitForExit();
                            }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-Command \"Start-Sleep -Seconds 5;Remove-Item -Path '{AppDomain.CurrentDomain.BaseDirectory}' -Recurse -Force\"",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            }).Dispose();
                        }

                        Process.Start("explorer.exe", $"\"{TargetPath}\"").Dispose();
                    }
                    else
                    {
                        Process.Start(AliasLocation, $"\"{TargetPath}\"").Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ExitCode = 2;

#if DEBUG
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    Debugger.Launch();
                }

                Debug.WriteLine($"Unexpected exception was thew, message: {ex.Message}");
#endif
            }
            finally
            {
                Application.Current.Shutdown(ExitCode);
            }
        }
    }
}
