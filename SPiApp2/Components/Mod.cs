using SPiApp2.Components.Common;
using SPiApp2.Components.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace SPiApp2.Components
{
    public static class Mod
    {
        public static bool CreateMod()
        {
            WinCreateMod dialog = new WinCreateMod();

            bool? result = dialog.ShowDialog();
            if (result != null && result.HasValue && result.Value == true)
            {
                RefreshList(true, dialog.SelectedMod);
                return true;
            }

            return false;
        }

        public static void BrowseFolder()
        {
            // C:\Users\UserName\AppData\Local\Activision\CoDWaW\mods
            UserData.Instance.Save();

            string selectedMod = UserData.SelectedMod;
            // string installPath = Preferences.InstallPath;
            char sep = System.IO.Path.DirectorySeparatorChar;

            if (!string.IsNullOrWhiteSpace(selectedMod))
            {
                //string path = string.Format("{0}{1}mods{1}{2}", installPath, sep, selectedMod);
                string path = string.Format("C:\\Users\\{0}\\AppData\\Local\\Activision\\CoDWaW\\mods{1}{2}" , Environment.UserName , sep , selectedMod );
                SPiApp2.Components.Application.Browse(path);
            }
        }
        
        // Since its appdata , its hard for bat files to do job 
        // because of this many exceptions hit and crashes the application
        // so its better i copy mod.csv in zone_source , compile mod.ff
        // place the mod.ff in mods folder
        // remove mod.csv and mod.ff from zone_source and zone_respectively
        public static void BuildFastFile()
        {
            UserData.Instance.Save();           

            string AppdataPath = string.Format("C:\\Users\\{0}\\AppData\\Local\\Activision\\CoDWaW\\mods" , Environment.UserName );

            if (File.Exists(string.Format("{0}\\{1}\\mod.csv", AppdataPath, UserData.SelectedMod ) ) )
            {                
                File.Copy(string.Format("{0}\\{1}\\mod.csv", AppdataPath, UserData.SelectedMod ) , string.Format( "{0}\\zone_source\\mod.csv" , Preferences.InstallPath ) , true);
                SPiApp2.Components.Application.LaunchAndWaitUntilFinished( string.Format("{0}{1}WaWSPiApp2{1}bin{1}mod_build.bat", Preferences.InstallPath , System.IO.Path.DirectorySeparatorChar),Preferences.InstallPath,
                string.Format("\"{0}\" {1} {2}", Preferences.InstallPath, Preferences.Language.ToLower(), UserData.SelectedMap));

                if(File.Exists(string.Format("{0}\\{1}\\mod.ff", AppdataPath, UserData.SelectedMod))) 
                {
                    File.Delete(string.Format("{0}\\{1}\\mod.ff", AppdataPath, UserData.SelectedMod));
                }
             
                if (File.Exists(string.Format("{0}{1}zone{1}{2}{1}mod.ff", Preferences.InstallPath, Path.DirectorySeparatorChar, Preferences.Language.ToLower() ) ) )
                {
                    File.Move(string.Format("{0}{1}zone{1}{2}{1}mod.ff", Preferences.InstallPath, Path.DirectorySeparatorChar, Preferences.Language.ToLower()), string.Format("{0}\\{1}\\mod.ff", AppdataPath, UserData.SelectedMod));
                }                

                if (File.Exists(string.Format("{0}\\zone_source\\mod.csv", Preferences.InstallPath)))
                {                    
                    File.Delete(string.Format("{0}\\zone_source\\mod.csv", Preferences.InstallPath));                    
                }
            }
            else
            {
                MessageResult result = AppDialogMessage.Show("mod.csv could not be located. Do you want to create a blank mod.csv?", "Missing File", MessageButtons.YesNo, MessageIcon.Question);
                if (result == MessageResult.Yes)
                {                                    
                    File.Create(string.Format("{0}\\{1}\\mod.csv", AppdataPath, UserData.SelectedMod));
                }
                return;
            }
        }

        // Plan is to move all the folders in game folder mods 
        // then make iwd there and sent them back to appdata
        // and deleting the folders fro m game folder mods
        public static void BuildIWD()
        {
            UserData.Instance.Save();

            string AppdataPath = string.Format("C:\\Users\\{0}\\AppData\\Local\\Activision\\CoDWaW\\mods", Environment.UserName);

            char sep = System.IO.Path.DirectorySeparatorChar;
            string installPath = Preferences.InstallPath;
            string zipper = Preferences.Zipper;            

            if ( !Directory.Exists( string.Format("{0}\\mods\\{1}", Preferences.InstallPath, UserData.SelectedMod) ) ) 
            {
                Directory.CreateDirectory(string.Format("{0}\\mods\\{1}", Preferences.InstallPath, UserData.SelectedMod));
            }

            string[] AppdataFolderNames = Directory.GetDirectories(string.Format("{0}\\{1}", AppdataPath, UserData.SelectedMod));

            foreach ( var FolderPath in AppdataFolderNames ) 
            {
                string FolderName = FolderPath.Substring((AppdataPath+"\\"+UserData.SelectedMod+"\\").Length);
                // move them to game dir to make iwds                                
                Directory.Move(FolderPath, string.Format("{0}\\mods\\{1}\\{2}", Preferences.InstallPath, UserData.SelectedMod ,  FolderName));                                                                        
            }

            SPiApp2.Components.Application.LaunchAndWaitUntilFinished(
                string.Format("{0}{1}WaWSPiApp2{1}bin{1}mod_iwd.bat", Environment.CurrentDirectory, sep),
                installPath,
                string.Format("\"{0}\" \"{1}\" {2}", installPath, zipper, UserData.SelectedMod)                
            );

            string[] MODSFolderNames = Directory.GetDirectories(string.Format("{0}\\mods\\{1}", Preferences.InstallPath , UserData.SelectedMod));

            foreach (var FolderPath in MODSFolderNames)
            {
                string FolderName = FolderPath.Substring((Preferences.InstallPath + "\\mods\\" + UserData.SelectedMod + "\\").Length);                
                Directory.Delete(string.Format("{0}\\mods\\{1}\\{2}", Preferences.InstallPath, UserData.SelectedMod, FolderName) ,true );
            }
            
                File.Move(string.Format("{0}\\mods\\{1}\\{1}_assets.iwd", Preferences.InstallPath, UserData.SelectedMod) ,
                    string.Format( "{0}\\{1}\\{1}_assets.iwd" , AppdataPath , UserData.SelectedMod));            

            //SPiApp2.Controls.Console.WriteLine("exists " + FolderNames.Length);
            // Directory.Move();

            /*SPiApp2.Components.Application.LaunchAndWaitUntilFinished(
                string.Format("{0}{1}WaWSPiApp2{1}bin{1}mod_iwd.bat", Environment.CurrentDirectory, sep),
                installPath,
                //string.Format("\"{0}\" \"{1}\" {2}", installPath, zipper, UserData.SelectedMod)
                string.Format("\"{0}\" \"{1}\" {2}", AppdataPath+"\\"+UserData.SelectedMod, zipper, UserData.SelectedMod)
            );*/
        }

        public static void UpdateZoneFile()
        {
            UserData.Instance.Save();
            (new WinZoneMod()).ShowDialog();
        }

        public static void RunSelectedMod()
        {
            // First save the settings
            UserData.Instance.Save();

            ModSettings settings = ModSettings.Instance;
            settings.Save();

            string installPath = Preferences.InstallPath;
            string optional = string.Empty;

            // Enable developer
            if (UserData.ModDeveloper)
                optional = string.Format("{0} +set developer 1", optional);

            // Enable developer_scipt
            if (UserData.ModDeveloperScript)
                optional = string.Format("{0} +set developer_script 1", optional);

            // Enable cheats
            if (UserData.ModCheats)
                optional = string.Format("{0} +set thereisacow 1337", optional);

            // Use selected map
            if (UserData.ModUseMap)
                optional = string.Format("{0} +devmap {1}", optional, UserData.SelectedMap);

            // Enable custom command line options
            if (UserData.ModCustom)
                optional = string.Format("{0} {1}", optional, UserData.ModOptions);

            SPiApp2.Components.Application.Launch(
                string.Format("{0}{1}WaWSPiApp2{1}bin{1}mod_run.bat", Environment.CurrentDirectory, System.IO.Path.DirectorySeparatorChar),
                installPath,
                string.Format("\"{0}\" {1} \"{2}\" {3} \"{4}\"", installPath, UserData.SelectedMod, Preferences.Executable, GetMultiplayerSign(), optional)
            );
        }

        public static void RefreshList(bool select, string target = null)
        {
            Utility.GetWindow(out MainWindow window);
            ListBox list = window.ctrlModList;

            list.SelectionChanged -= ChangedSelectedMod;
            list.Items.Clear();
            // C:\Users\UserName\AppData\Local\Activision\CoDWaW\mods
            //string directory = string.Format("{0}{1}mods", Preferences.InstallPath, System.IO.Path.DirectorySeparatorChar);
            string directory = string.Format("C:\\Users\\{0}\\AppData\\Local\\Activision\\CoDWaW\\mods", Environment.UserName);
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    int start = directory.Length + 1;
                    int length = file.Length - start;
                    string name = file.Substring(start, length);

                    if (!string.IsNullOrWhiteSpace(name) && Utility.IsValid(name))
                    {
                        list.Items.Add(name);
                    }
                }

                if (select)
                {
                    if (target == null)
                    {
                        target = UserData.SelectedMod;
                    }

                    Utility.SelectValue(ref list, target);

                    if (list.SelectedItem is string selected)
                    {
                        UserData.SelectedMod = selected;
                    }
                    else
                    {
                        UserData.SelectedMod = string.Empty;
                    }

                    ModSettings.Instance.Load();
                }
            }

            list.SelectionChanged += ChangedSelectedMod;
        }

        private static void ChangedSelectedMod(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                object item = e.AddedItems[0];

                if (item != null)
                {
                    string name = item as string;
                    Debug.Assert(name != null);

                    UserData.SelectedMod = name;
                    ModSettings.Instance.Load();
                }
            }
        }

        private static string GetMultiplayerSign()
        { 
            if (UserData.ModIsSingleplayer)
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
    }
}
