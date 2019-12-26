using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using TIAGroupCopyCLI;

namespace TiaOpennessHelper.Utils
{

    public static class MyResolverClass
    {
        public static Assembly MyResolver(object sender, ResolveEventArgs args)
        {
            int index = args.Name.IndexOf(',');
            if (index == -1)
            {
                return null;
            }
            string name = args.Name.Substring(0, index) + ".dll";
            // Edit the following path according to your installation
            string path = Path.Combine(@"C:\Program Files\Siemens\Automation\Portal V15_1\PublicAPI\V15.1\", name);
            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                Program.Progress("loading Assembly: " + fullPath);
                return Assembly.LoadFrom(fullPath);
            }
            else if (name == "Siemens.Engineering.dll")
            {
                Program.Progress("The following DLL does not exits: " + fullPath);
            }

            return null;
        }

        public static void AddAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += MyResolver;
        }
    }


    public static class Heandlers
    {
        private const string BASE_PATH = "SOFTWARE\\Siemens\\Automation\\Openness\\";
        private static string AssemblyPath = "";


        public static bool SelectAssmebly(string projectVersion, string preferedTiaVersion, string preferedAssemblyVersion)
        {
            
            string selectedTiaVersion = "";
            string selectedAssemblyVersion = "";

            Version projectV = new Version(projectVersion);
            Version preferedAssemblyV = new Version(preferedAssemblyVersion);

            //check if TIA protal version for this project is isntalled
            List<string> tiaVersionsString = GetEngineeringVersions();
            if (tiaVersionsString.Count == 0)
            {
                Program.Progress($"No installed TIA version found.");
                return false;
            }
            foreach (string currentV in tiaVersionsString)
            {
                Version tiaVersion = new Version(currentV);
                if (tiaVersion == projectV)
                {
                    selectedTiaVersion = currentV;
                }
            }
            if (selectedTiaVersion == "")
            {
                Program.Progress($"TIAP version not found for project version {projectVersion}");
                Program.Progress($"Found this TIAP versions installed:");
                foreach (string currentV in tiaVersionsString)
                {
                    Program.Progress($"V{currentV}");
                }
                return false;
            }
            if (selectedTiaVersion != preferedTiaVersion)
            {
                Program.Progress($"Application was tested with TIAP version {preferedTiaVersion}");
                Program.Progress($"However, application will run with TIAP version {selectedTiaVersion}");
            }
            else
            {
                Program.Progress($"Aapplication will run with TIAP version {selectedTiaVersion}");
            }

            //select openness version closest to wha was used during development
            List<string> assmblyVersionsString =  GetOpennessAssmblyVersions(selectedTiaVersion);
            if (assmblyVersionsString.Count == 0)
            {
                Program.Progress($"No installed Openness version found.");
                return false;
            }
            foreach (string currentV in assmblyVersionsString)
            {
                selectedAssemblyVersion = currentV;
                Version assemblyVersion = new Version(currentV);
                if (assemblyVersion >= preferedAssemblyV)
                {
                    break;
                }
            }

            if (selectedAssemblyVersion == "")
            {
                Program.Progress($"No fitting Openness version found.");
                Program.Progress($"Found this Openness versions installed:");
                foreach (string currentV in assmblyVersionsString)
                {
                    Program.Progress($"V{currentV}");
                }
                return false;
            }

            if (selectedAssemblyVersion != preferedAssemblyVersion)
            {
                Program.Progress($"Application was tested with Openness version {preferedAssemblyVersion}");
                Program.Progress($"However, application will run with Openness version {selectedAssemblyVersion}");
            }
            else
            {
                Program.Progress($"Application will run with Openness version {selectedAssemblyVersion}");
            }

            string AssemblyPath = GetOpennessAssemblyPath(selectedTiaVersion, selectedAssemblyVersion);


            if (AssemblyPath == "")
            {
                Program.Progress("Could not find Openness DLL.");
                return false;
            }
            if (!File.Exists(AssemblyPath))
            {
                Program.Progress("The following DLL does not exits: " + AssemblyPath);
                return false;
            }


                return true;
        }

        public static List<string> GetEngineeringVersions()
        {
            RegistryKey key = GetRegistryKey(BASE_PATH);

            if (key != null)
            {
                var names = key.GetSubKeyNames().OrderBy(x => x).ToList();
                key.Dispose();

                return names;
            }

            return new List<string>();
        }

        public static List<string> GetOpennessAssmblyVersions(string tiaVersion)
        {
            RegistryKey key = GetRegistryKey(BASE_PATH + tiaVersion);

            if (key != null)
            {
                try
                {
                    var subKey = key.OpenSubKey("PublicAPI");

                    var result = subKey.GetSubKeyNames().OrderBy(x => x).ToList();

                    subKey.Dispose();

                    return result;
                }
                finally
                {
                    key.Dispose();
                }
            }

            return new List<string>();
        }

        public static string GetOpennessAssemblyPath(string tiaVersion, string opennessAssemblyVersion)
        {
            RegistryKey key = GetRegistryKey(BASE_PATH + tiaVersion + "\\PublicAPI\\" + opennessAssemblyVersion);

            if(key != null)
            {
                try
                {
                    AssemblyPath = key.GetValue("Siemens.Engineering").ToString();
                    
                    return AssemblyPath;
                }
                finally
                {
                    key.Dispose();
                }
            }

            return null;
        }

        private static RegistryKey GetRegistryKey(string keyname)
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key = baseKey.OpenSubKey(keyname);
            if (key == null)
            {
                baseKey.Dispose();
                baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                key = baseKey.OpenSubKey(keyname);
            }
            if (key == null)
            {
                baseKey.Dispose();
                baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                key = baseKey.OpenSubKey(keyname);
            }
            baseKey.Dispose();

            return key;
        }

        public static void AddAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolve;
        }

        public static Assembly OnResolve(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(args.Name);
            
            if (assemblyName.Name.EndsWith("Siemens.Engineering") 
                && string.IsNullOrEmpty(AssemblyPath) == false
                && File.Exists(AssemblyPath))
            {
                return Assembly.LoadFrom(AssemblyPath);
            }

            //if (name == "Siemens.Engineering.dll")
            //{
            //    Program.Progress("The following DLL does not exits: " + fullPath);
            //}


            return null;
        }

        public static void AddAppExceptionHaenlder()
        {
            if (!Debugger.IsAttached)
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
            }
        }
        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            string exceptionStr = args.ExceptionObject.ToString();
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Ups -> Runtime terminating: {0}", args.IsTerminating);

            // Get stack trace for the exception with source file information
            var st = new StackTrace(e, true);
            // Get the top stack frame
            var frame = st.GetFrame(0);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();

            //AppDomain.Unload(AppDomain.CurrentDomain);
        }
    }
}
