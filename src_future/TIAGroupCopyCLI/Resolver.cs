using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
                return Assembly.LoadFrom(fullPath);
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

        public static List<string> GetAssmblies(string version)
        {
            RegistryKey key = GetRegistryKey(BASE_PATH + version);

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

        public static string GetAssemblyPath(string version, string assembly)
        {
            RegistryKey key = GetRegistryKey(BASE_PATH + version + "\\PublicAPI\\" + assembly);

            if (key != null)
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
