using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Configuration;
using System.Reflection;
using System.Threading;

namespace CNCLRRRRR.FasterRunner
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Console.SetOut(new StreamWriter(StartupPath + "\\FasterRunner.log"));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            

            ReadAllSettings();
            Program.GetGameLocation();
            Console.WriteLine("Guess EntryFolderLocation:" + Program.EntryFolderLocation);
            bool flag = !Program.GameLocationIsExists;
            if (flag)
            {
                MessageBox.Show(string.Format("Cannot find {0} or game is broken.", Program.TargetEntryName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                Program.TargetGameLocation = Program.EntryFolderLocation + "\\Data\\" + Program.TargetGameName;
                Console.WriteLine("Guess TargetGameLocation:" + Program.EntryFolderLocation);
                bool flag2 = !Program.GetModSkudefLocation();
                if (flag2)
                {
                    MessageBox.Show(string.Format("Cannot find mod skudef:\n{0}", Program.ModSkudefLocation), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    Program.strBuild.Append(" -modconfig \"" + Program.ModSkudefLocation + "\" ");
                    Console.WriteLine("Guess ModSkudefLocation:" + Program.ModSkudefLocation);
                    bool flag3 = !Program.GetGameSkudefLocation();
                    if (flag3)
                    {
                        MessageBox.Show(string.Format("Cannot find game skudef:\n{0}", Program.GameSkudefLocation), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        Program.strBuild.Append(" -config \"" + Program.GameSkudefLocation + "\" ");
                        Console.WriteLine("Guess GameSkudefLocation:" + Program.GameSkudefLocation);
                        //bool modRuntimeLocation = Program.GetModRuntimeLocation();
                        //if (modRuntimeLocation)
                        //{
                        //    Console.WriteLine("Guess ModRuntimeLocation(New TargetGameLocation):" + Program.TargetGameLocation);
                        //}
                        Program.AppendCustomArguments(args);
                        Program.AppendConfigArguments();
                        try
                        {
                            Program.Runner();
                        }
                        catch (Exception e)
                        {
                            Program.DebugInfo(e);
                            return;
                        }
                        try
                        {
                            Program.SaveLocation(Program.EntryFolderLocation);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}", ex.Message);
                        }
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("[Main]Done:{0}ms",sw.ElapsedMilliseconds);
            Console.Out.Flush();


        }

        private static void ReadAllSettings()
        {
            Configuration cfg;
            try
            {
                cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


                Program.BootLoaderName = cfg.AppSettings.Settings["BootLoaderName"].Value;
                Program.TargetEntryName = cfg.AppSettings.Settings["TargetEntryName"].Value;
                Program.GameRunver = cfg.AppSettings.Settings["GameRunver"].Value;
                Program.TargetGameNamePrefix = cfg.AppSettings.Settings["TargetGameNamePrefix"].Value;
                Program.TargetGameNameSuffix = cfg.AppSettings.Settings["TargetGameNameSuffix"].Value;
                
                Program.ModSkudefName = cfg.AppSettings.Settings["ModSkudefName"].Value;

                Program.LegcayModFolder = Convert.ToBoolean(cfg.AppSettings.Settings["LegcayModFolder"].Value);

                Program.GameSkudefLanguage = cfg.AppSettings.Settings["GameSkudefLanguage"].Value;
                Program.ConfigurationArguments = cfg.AppSettings.Settings["ConfigurationArguments"].Value;

                Program.RegistryLocation = cfg.AppSettings.Settings["RegistryLocation"].Value.Split(';');
                Program.RegistryValue = cfg.AppSettings.Settings["RegistryValue"].Value.Split(';');
                Program.ValidateGameFileNames = cfg.AppSettings.Settings["ValidateGameFileNames"].Value.Split(';');


                if (LegcayModFolder)
                {
                    Program.ModFolderName = cfg.AppSettings.Settings["ModFolderName"].Value;
                    Console.WriteLine("[ReadAllSettings]ModFolderName:"+ModFolderName);
                }
                else
                {
                    Program.ModFolderName = "";
                    Console.WriteLine("[ReadAllSettings]ModFolderName:Null");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Cannot find FasterRunner Configuration or this file is broken."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }




        }

        private static void Runner()
        {
            Process process = new Process();
            bool flag = Program.CustomGameSupport();
            if (flag)
            {
                process.StartInfo.WorkingDirectory = Program.StartupPath + "\\" + Program.ModFolderName;
            }
            else
            {
                process.StartInfo.WorkingDirectory = Program.EntryFolderLocation + "\\Data";
            }
            try
            {
                Program.Steam();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
            process.StartInfo.FileName = Program.TargetGameLocation;
            process.StartInfo.Arguments = Program.strBuild.ToString();
            process.StartInfo.UseShellExecute = false;
            Console.WriteLine("Filename:" + process.StartInfo.FileName.ToString());
            Console.WriteLine("Args:" + process.StartInfo.Arguments.ToString());
            Console.WriteLine("Workpath:" + process.StartInfo.WorkingDirectory.ToString());

            Console.WriteLine("CMD Command:(You can copy it and paste it in cmd.exe to running game when you want to test!):{0} {1}",process.StartInfo.FileName.ToString(),process.StartInfo.Arguments.ToString());

            process.Start();
        }

        private static void Steam()
        {
            using (StreamWriter streamWriter = new StreamWriter(Program.EntryFolderLocation + "\\Steam_appid.txt"))
            {
                streamWriter.WriteLine("17480");
            }
            using (StreamWriter streamWriter2 = new StreamWriter(Program.EntryFolderLocation + "\\Data\\Steam_appid.txt"))
            {
                streamWriter2.WriteLine("17480");
            }
        }

        private static bool CustomGameSupport()
        {
            FileInfo fileInfo = new FileInfo(string.Concat(new string[]
            {
                Program.StartupPath,
                "\\",
                Program.ModFolderName,
                "\\",
                Program.TargetGameName
            }));
            bool exists = fileInfo.Exists;
            bool result;
            
            if (exists)
            {
                Program.TargetGameLocation = fileInfo.FullName;
                Console.WriteLine("[CustomGameSupport]Custom Game Yes:" + fileInfo.FullName);
                result = true;
            }
            else
            {
                Console.WriteLine("[CustomGameSupport]Custom Game No:" + fileInfo.FullName);
                result = false;
            }
            return result;
        }

        private static void AppendCustomArguments(string[] args)
        {
            Console.WriteLine("[AppendCustomArguments]Number:" + args.Length);
            foreach (string value in args)
            {
                Program.strBuild.Append(value);
                Program.strBuild.Append(' ');
                Console.WriteLine("[AppendCustomArguments]:" + value);
            }
        }

        private static void AppendConfigArguments()
        {
            Program.strBuild.Append(ConfigurationArguments);
            Console.WriteLine("[ConfigurationArguments]:" + ConfigurationArguments);
        }

        private static void DebugInfo(Exception e)
        {
            MessageBox.Show(string.Format("'{0}\\n{1}'", Program.strBuild.ToString(), e.Message), "DEBUG", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private static bool ValidateGameLocation(string path)
        {;
            Console.WriteLine("[ValidateGameLocation]Vaild Game Path:" + path + "\\" + Program.TargetEntryName);
            bool flag = File.Exists(path + "\\" + Program.TargetEntryName);
            bool result;
            if (flag)
            {
                foreach (string text in Program.ValidateGameFileNames)
                {
                    Console.WriteLine("[ValidateGameLocation]Game Files Check:" + path + "\\Data\\" + text);
                    bool flag2 = !File.Exists(path + "\\Data\\" + text);
                    if (flag2)
                    {
                        Console.WriteLine("[ValidateGameLocation]Invaild Game File:{0}", path + "\\Data\\" + text);
                        return false;
                    }
                }
                Console.WriteLine("[ValidateGameLocation]Try StartupPath:" + Program.StartupPath);
                Program.EntryFolderLocation = Program.StartupPath;
                Program.GameLocationIsExists = true;
                Console.WriteLine("[ValidateGameLocation]Vaild Game Path:{0}", path);
                result = true;
            }
            else
            {
                Console.WriteLine("[ValidateGameLocation]Invaild Path:{0} ({1}?)",path,Program.TargetEntryName);
                result = false;
            }
            return result;
        }

        private static void GetRegistry()
        {
            string[] registryLocation = Program.RegistryLocation;
            int i = 0;
            while (i < registryLocation.Length)
            {
                string text = registryLocation[i];
                Console.WriteLine("[GetRegistry]Try Registry:" + text);
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(text))
                {
                    bool flag = registryKey == null;
                    if (!flag)
                    {
                        foreach (string name in Program.RegistryValue)
                        {
                            string text2 = registryKey.GetValue(name, RegistryValueKind.String).ToString();
                            Console.WriteLine("[GetRegistry]:Get ["+name+"] Key:" + text2);
                            bool flag2 = Program.ValidateGameLocation(text2);
                            if (flag2)
                            {
                                Program.EntryFolderLocation = text2;
                                return;
                            }
                        }
                    }
                }
                i++;
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002638 File Offset: 0x00000838
        private static void SaveLocation(string path)
        {
            Console.WriteLine("[SaveLocation]Save Location:" + Program.ApplicationDataLocation);
            bool flag = !Directory.Exists(Program.ApplicationDataLocation);
            if (flag)
            {
                Console.WriteLine("[SaveLocation]Create Location:" + Program.ApplicationDataLocation);
                Directory.CreateDirectory(Program.ApplicationDataLocation);
            }
            using (StreamWriter streamWriter = new StreamWriter(Program.ApplicationDataLocation + "\\" + Program.BootLoaderName + ".dat", false, Encoding.UTF8))
            {
                Console.WriteLine("[SaveLocation]Save:" + path);
                streamWriter.WriteLine(path);
            }
        }

        private static void LoadLocation()
        {
            Console.WriteLine(string.Concat(new string[]
            {
                "[LoadLocation]Load Location:",
                Program.ApplicationDataLocation,
                "\\",
                Program.BootLoaderName,
                ".dat"
            }));
            bool flag = !File.Exists(Program.ApplicationDataLocation + "\\" + Program.BootLoaderName + ".dat");
            if (flag)
            {
                Console.WriteLine("[LoadLocation]Null File.");
            }
            else
            {
                using (StreamReader streamReader = new StreamReader(Program.ApplicationDataLocation + "\\" + Program.BootLoaderName + ".dat", Encoding.UTF8))
                {
                    string text = streamReader.ReadLine();
                    Console.WriteLine("[LoadLocation]Read:" + text);
                    bool flag2 = Program.ValidateGameLocation(text);
                    if (flag2)
                    {
                        Program.EntryFolderLocation = text;
                    }
                }
            }
        }

        private static void GetGameLocation()
        {
            bool flag = !Program.GameLocationIsExists;
            if (flag)
            {
                Program.ValidateGameLocation(Program.StartupPath);
            }
            bool flag2 = !Program.GameLocationIsExists;
            if (flag2)
            {
                Program.LoadLocation();
            }
            bool flag3 = !Program.GameLocationIsExists;
            if (flag3)
            {
                Program.GetRegistry();
            }
            bool flag4 = !Program.GameLocationIsExists;
            if (flag4)
            {
                Program.FileDialog();
            }
            bool flag5 = !Program.GameLocationIsExists;
            if (flag5)
            {
                Program.AllDiskSearchController();
            }
        }

        private static void FileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = Program.OpenDialogFilter;
            openFileDialog.Title = string.Format("Please select {0} manually:", Program.TargetEntryName);
            bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
            if (flag)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                Console.WriteLine("[FileDialog]Choose:" + fileInfo.FullName);
                bool flag2 = Program.ValidateGameLocation(fileInfo.Directory.FullName);
                if (flag2)
                {
                    Program.EntryFolderLocation = fileInfo.Directory.FullName;
                }
                else
                {
                    MessageBox.Show(string.Format("{0} is wrong, please try other once.(Need {1})\n", fileInfo.Name, Program.TargetEntryName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Program.FileDialog();
                }
            }
        }

        private static void AllDiskSearch()
        {
            try
            {
                Program.AllDiskSearchBrowser();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Search error:\n{0}\nPlease try again.", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Program.AllDiskSearch();
            }
        }

        private static void AllDiskSearchBrowser()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please select the directory to search:";
            bool flag = DialogResult.OK == folderBrowserDialog.ShowDialog();
            if (flag)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                DirectoryInfo directoryInfo = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles(Program.TargetEntryName, SearchOption.AllDirectories))
                {
                    bool flag2 = Program.ValidateGameLocation(fileInfo.Directory.FullName);
                    if (flag2)
                    {
                        Program.EntryFolderLocation = fileInfo.Directory.FullName;
                        stopwatch.Stop();
                        MessageBox.Show(string.Format("Congratulations! You found the target program through a full search:\n{0}\nTotal time spent {1} seconds!", fileInfo.FullName, stopwatch.ElapsedMilliseconds / 1000L), "Congratulations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                stopwatch.Stop();
                MessageBox.Show(string.Format("Unfortunately, {0} was not found.\nTotal time spent {1} seconds.", Program.TargetEntryName, stopwatch.ElapsedMilliseconds / 1000L), "Congratulations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Program.AllDiskSearchBrowser();
            }
        }

        private static void AllDiskSearchController()
        {
            bool flag = DialogResult.Yes == MessageBox.Show("Do you want a search? This will be slow!!!\nI recommend that you install the program normally, or manually select the target program.", "?", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
            if (flag)
            {
                bool flag2 = DialogResult.Yes == MessageBox.Show("Ask again, do you really want a search?\nIf you want to stop, you can only end the program from the task manager.", "??", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (flag2)
                {
                    bool flag3 = DialogResult.Yes == MessageBox.Show("I still recommend you to think clearly, search is very complicated and slows down the system. \nAre you really going to do this? If something goes wrong, don't blame the program.", "???", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (flag3)
                    {
                        Program.AllDiskSearch();
                    }
                }
            }
        }

        private static bool GetModSkudefLocation()
        {
            FileInfo fileInfo = new FileInfo(string.Concat(new string[]
            {
                Program.StartupPath,
                "\\",
                Program.ModFolderName,
                "\\",
                Program.ModSkudefName
            }));
            Program.ModSkudefLocation = fileInfo.FullName;
            return fileInfo.Exists;
        }

        //private static bool GetModRuntimeLocation()
        //{
        //    FileInfo fileInfo = new FileInfo(string.Concat(new string[]
        //    {
        //        Program.StartupPath,
        //        "\\",
        //        Program.ModFolderName,
        //        "\\",
        //        Program.TargetGameName
        //    }));
        //    Program.TargetGameLocation = fileInfo.FullName;
        //    Console.WriteLine("[GetModRuntimeLocation]Mod Runtime Game:" + fileInfo.FullName);
        //    return fileInfo.Exists;
        //}

        private static bool GetGameSkudefLocation()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Program.EntryFolderLocation);
            FileInfo[] files = directoryInfo.GetFiles("RA3_"+GameSkudefLanguage+"_" + Program.GameRunver + ".SkuDef", SearchOption.TopDirectoryOnly);
            bool flag = files.Length != 0;
            bool result;
            if (flag)
            {
                Program.GameSkudefLocation = files[0].FullName;
                result = true;
            }
            else
            {
                Program.GameSkudefLocation = Program.EntryFolderLocation + "\\RA3_"+GameSkudefLanguage+"_" + Program.GameRunver + ".SkuDef";
                result = false;
            }
            return result;
        }

        private static string BootLoaderName = "RA3";

        private static string GameRunver = "1.12";

        private static string TargetGameNamePrefix = "ra3_";
        private static string TargetGameNameSuffix = ".game";

        private static string TargetGameName = Program.TargetGameNamePrefix + Program.GameRunver + Program.TargetGameNameSuffix;

        private static string TargetEntryName = "ra3.exe";

        private static string ModFolderName = "Science.Mod";

        private static bool LegcayModFolder = true;

        private static string GameSkudefLanguage = "*";

        private static string ConfigurationArguments = "";

        private static string ModSkudefName = "Science.Faster";

        private static string[] RegistryLocation = new string[]
        {
            "SOFTWARE\\Electronic Arts\\Electronic Arts\\Red Alert 3",
            "SOFTWARE\\WOW6432Node\\Electronic Arts\\Electronic Arts\\Red Alert 3"
        };

        private static string[] RegistryValue = new string[]
        {
            "Install Dir",
            "Folder"
        };

        private static string[] ValidateGameFileNames = new string[]
        {
            "Apt.big",
            "StaticStream.big",
            "WBData.big",
            "GlobalStream.big"
        };

        private static string ApplicationDataLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CNCLauncher3";

        private static string OpenDialogFilter = string.Format("({0}){1}|{2}", Program.BootLoaderName, Program.TargetEntryName, Program.TargetEntryName);

        private static string StartupPath = Application.StartupPath;

        private static StringBuilder strBuild = new StringBuilder();

        private static string ModSkudefLocation = "";

        private static string GameSkudefLocation = "";

        private static string EntryFolderLocation = "";

        private static string TargetGameLocation = "";

        private static bool GameLocationIsExists = false;

        //private static string ConfigurationFile = StartupPath + "\\FasterRunner.config";
    }
}
