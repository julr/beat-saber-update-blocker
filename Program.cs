using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BeatSaberUpdateBlocker
{
    internal class Program
    {
        const string AcfFileName = "appmanifest_620980.acf";

        static void Main(string[] args)
        {
            string steamappsPath = null;
            string manifestId = null;

            Console.WriteLine("=== Beat Saber Update Blocker ===");
            Console.WriteLine();

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var currentArg = args[i].ToLower();
                    if (currentArg == "-help")
                    {
                        Console.WriteLine("Available options:");
                        Console.WriteLine("-help\t\t\tDisplay this help text");
                        Console.WriteLine("-steamapps [dir]\tManually specify the path to the steamapps directory");
                        Console.WriteLine("-manifest [id]\t\tManually specify the manifest id to write");
                        Exit();
                    }
                    else if ((currentArg == "-steamapps") && (args.Length > i + 1))
                    {
                        steamappsPath = args[i + 1];
                    }
                    else if ((currentArg == "-manifest") && (args.Length > i + 1))
                    {
                        manifestId = args[i + 1];
                    }
                }
            }

            if (IsSteamRunning())
            {
                Console.WriteLine("Steam is currently running. Please shut it down and try again.");
                Exit(true);
            }

            Console.Write("Searching for ACF File... ");
            var acfPath = SearchAcfPath(steamappsPath);
            if (acfPath == null)
            {
                Console.WriteLine("not found, use the -steamapps option to specify the steamapps folder by hand");
                Exit(true);
            }
            Console.WriteLine(" found");
            if (manifestId == null)
            {
                Console.Write("Fetching latest manifest id... ");
                manifestId = GetLastestManifestId();
                if (manifestId == null)
                {
                    Console.WriteLine("failed, use the -manifest option to specify the manifest id by hand");
                    Exit(true);
                }
                Console.WriteLine("ok");
            }
            else
            {
                Console.WriteLine("Using manually specified manifest id");
            }

            Console.Write("Patching ACF file... ");
            try
            {
                ModifyAcfFile(acfPath, manifestId);
                Console.WriteLine("done");
                Exit();

            }
            catch (Exception)
            {
                Console.WriteLine("failed");
                Exit(true);
            }
        }

        private static void Exit(bool Error = false)
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            Environment.Exit((Error) ? 1 : 0);
        }

        private static bool IsSteamRunning()
        {
            return (Process.GetProcessesByName("Steam").Length > 0) ? true : false;
        }

        private static string SearchAcfPath(string SteamappsPath = null)
        {
            if (SteamappsPath == null)
            {
                string steamPath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", string.Empty);
                SteamappsPath = Path.Combine(steamPath, "steamapps");
            }
            string path = Path.Combine(SteamappsPath, AcfFileName);
            return (File.Exists(path)) ? path : null;
        }

        private static string GetLastestManifestId()
        {
            try
            {
                using WebClient webClient = new();
                var jsonObject = JsonDocument.Parse(webClient.DownloadString("https://api.steamcmd.net/v1/info/620980"));
                return jsonObject.RootElement.GetProperty("data").GetProperty("620980").GetProperty("depots").GetProperty("620981").GetProperty("manifests").GetProperty("public").GetString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void ModifyAcfFile(string FilePath, string ManifestId)
        {
            //very crude implementation but it works
            string statePattern = @"(""StateFlags""\s+"")(?<state>\d+)";
            string manifestPattern = @"(""620981""\s*{\s*""manifest""\s+"")(\d+)";

            string acfContent = File.ReadAllText(FilePath);
            acfContent = Regex.Replace(acfContent, statePattern, m => m.Groups[1].Value + "4");
            acfContent = Regex.Replace(acfContent, manifestPattern, m => m.Groups[1].Value + ManifestId);

            File.WriteAllText(FilePath, acfContent);
        }
    }
}
