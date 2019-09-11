using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

using NoZ.Serialization;

namespace NoZ.Platform.Windows{
    public static class CLI {
        /// <summary>
        /// Run the NoZ command line interface with the given arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>0 if there was no error</returns>
        public static int Run (string args) {
            // Read NoZ install diretory from the registry.
            string nozInstallDir;
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                if (null == key)
                    return 1;

                using (var subkey = key.OpenSubKey("Software\\NoZ Games\\NoZ")) {
                    if (null == subkey)
                        return 1;

#if NOZ_RELEASE
                    nozInstallDir = (string)subkey.GetValue("InstallDirectory");
#else
                    nozInstallDir = (string)subkey.GetValue("DebugInstallDirectory");
#endif                
                    if (null == nozInstallDir)
                        return 1;
                }
            }

            var cli = Path.Combine(nozInstallDir, "NozCLI.exe");

            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    CreateNoWindow = true,
                    FileName = cli,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            using (var reader = process.StandardOutput) {
                while (!process.HasExited) {
                    Console.WriteLine(reader.ReadLine());
                }
            }
            process.WaitForExit();
            return process.ExitCode;
        }

        /// <summary>
        /// Runs an 'import' command 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public static int Import (string sourcePath, string targetPath) {
            // Find all of the libraries with Xaml namespaces
            string load = "";
            foreach(var a in SerializedType.GetAssemblies()) 
                load = load + $" -l \"{a.Location}\"";

            return Run($"import \"{sourcePath}\" {load} -o \"{targetPath}\"");
        }

        /// <summary>
        /// Runs a 'pack' command
        /// </summary>
        public static void Pack (string sourcePath, string targetPath) {
            Run($"pack \"{sourcePath}\" -o \"{targetPath}\"");
        }

    }
}
