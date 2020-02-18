using System;
using System.Reflection;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.Library.MasterCopies;

using TiaOpennessHelper.Utils;
using TIAHelper.Services;
using TIAGroupCopyCLI.Models;
using TIAGroupCopyCLI.Para;

namespace TIAGroupCopyCLI //TIAGroupCopyCLI
{
    class Program
    {
        #region Fileds
        const string TIAP_VERSION_USED_FOR_TESTING = "15.1";
        const string OPENESS_VERSION_USED_FOR_TESTING = "15.1.0.0";

        static Parameters Parameters;

        private static TiaPortal tiaPortal;
        private static Project project;
        #endregion


        static void Main(string[] args)
        {
            Heandlers.AddAppExceptionHaenlder();


            //string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            //string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            Progress("TIA Group copy v" + fileVersion);
            Progress("This beta version is a customized solution for now");


            Progress("================================================================");

            Parameters = new Parameters(args);
            if (!Parameters.ParameterOK)
            {
                Console.WriteLine("");
                Console.WriteLine("Hit enter to exit.");
                Console.ReadLine();
                return;
            }

            if (!Heandlers.SelectAssmebly(Parameters.ProjectVersion, TIAP_VERSION_USED_FOR_TESTING, OPENESS_VERSION_USED_FOR_TESTING))
            {
                Console.WriteLine("");
                Console.WriteLine("Hit enter to exit.");
                Console.ReadLine();
                return;
            }
            Heandlers.AddAssemblyResolver();




            RunTiaPortal();




            Console.WriteLine("");
            Console.WriteLine("Hit enter to exit.");
            Console.ReadLine();
        }


        //=================================================================================================
        private static void RunTiaPortal()
        {
            string indexFormat = "D2";
            uint groupCounter = 1;


            #region test: hardcode path
            /*
            Console.WriteLine("!!!with hardcoded project path for testing!!!");

            Parameters.ProjectPath = @"D:\Source\TiaGroupCopyCli\TIA_samples\EMS\EMS.ap15_1";
             Parameters.TemplateGroupName = "EMS_Controller";
             Parameters.NewGroupNamePrefix = "EMS_Controller ";
             Parameters.Prefix = "ems";
            
           Parameters.ProjectPath = "D:\\KnesMX\\source\\TIA\\Groups\\Groups.ap15_1";
           Parameters.TemplateGroupName = "Group_";
           Parameters.Prefix = "sk";
           Parameters.NumOfGroups = 3;
           Parameters.FBaseAddrOffset = 1;
           Parameters.FDestAddrOffset = 100;
           Parameters.IDeviceDeviceNumberOffset = 1;
           Parameters.IDeviceIoAddressOffset = 100;
           */
            #endregion

            #region tia and project

            Progress("Check running TIA Portal");
            bool tiaStartedWithoutInterface = false;

            Service.OpenProject(Parameters.ProjectPath, ref tiaPortal, ref project, ref tiaStartedWithoutInterface);

            if ((tiaPortal == null) || (project == null))
            {
                CancelGeneration("Could not open project.");
                return;
            }

            Progress(String.Format("Project {0} is open", project.Path.FullName));
            
            #endregion


            #region master copy
            Progress("Creating master copy.");

            DeviceUserGroup tiaTemplateGroup = project.DeviceGroups.Find(Parameters.TemplateGroupName);
            if (tiaTemplateGroup == null)
            {
                CancelGeneration("Group not found.");
                return;
            }

            //=======copy to master copy========
            //MasterCopyComposition masterCopies = project.ProjectLibrary.MasterCopyFolder.MasterCopies;
            MasterCopy templateCopy = null;
            try
            {

                templateCopy = project.ProjectLibrary.MasterCopyFolder.MasterCopies.Create(tiaTemplateGroup);
            }
            catch (Exception ex)
            {
                CancelGeneration("Could not create master copy.",ex);
                return;
            }

            if (templateCopy == null)
            {
                CancelGeneration("Could not create master copy.");
                return;
            }

            MasterCopy deleteMasterCopy = project.ProjectLibrary.MasterCopyFolder.MasterCopies.Find(templateCopy.Name);
            #endregion

            #region get basic info from template group

            ManageGroup templateGroup = new ManageGroup(tiaTemplateGroup, Parameters.NewGroupNamePrefix, Parameters.Prefix, groupCounter, indexFormat);
            if (templateGroup.Devices.Where(d => d.DeviceType == DeviceType.Plc).Count() != 1)
            {
                CancelGeneration("No PLC or more than 1 PLC in group.");
                return;
            }

            templateGroup.SavePlcConfigInTemplate();
            
            

            #endregion

            #region change name and IP of first group (template Group)


            Progress("Adjusting template group.");
            templateGroup.ChangeNames();
           
            #endregion


            #region copy group loop
            DeviceUserGroupComposition tiaUserGroups = project.DeviceGroups;
 
            while (++groupCounter <= Parameters.NumOfGroups)
            {
                #region copy group
                Progress("Creating Group " + groupCounter);
               // currentPrefix = Parameters.Prefix + groupCounter.ToString(indexFormat);

                DeviceUserGroup newTiaGroup;
                ManageGroup newGroup;
                try
                {
                    newTiaGroup = tiaUserGroups.CreateFrom(templateCopy);
                    if (newTiaGroup == null)
                    {
                        CancelGeneration("Could not create new Group.");
                        return;
                    }
                    else
                    {
                        newGroup = new ManageGroup(newTiaGroup, Parameters.NewGroupNamePrefix, Parameters.Prefix, groupCounter, indexFormat);
                    }
                }
                catch(Exception e)
                {
                    CancelGeneration("Could not create new Group.", e);
                    return;
                }

                #endregion




                #region change settigns 
                newGroup.SaveConfig();
                newGroup.ChangeNames();
                newGroup.ChangeIpAddresses(groupCounter - 1);
                newGroup.CreateNewIoSystem(templateGroup.originalSubnet);
                newGroup.ConnectPlcToMasterIoSystem(templateGroup.masterIoSystem);
                newGroup.CopyFromTemplate(templateGroup);
                newGroup.ReconnectAndRestore_WithAdjustments((groupCounter - 1), Parameters.FBaseAddrOffset * (groupCounter - 1), Parameters.FDestAddrOffset * (groupCounter - 1), (Parameters.IDeviceIoAddressOffset * (groupCounter - 1)));
                newGroup.DelecteOldSubnet();
                #endregion change settigns

                
            }

            #endregion

            try
            {
                deleteMasterCopy.Delete();
            }
            catch(Exception ex)
            {
                Program.FaultMessage("Could not delete Mastercopy.", ex);
            }

            Progress("");

            Console.WriteLine("Copy complete.");
            if (tiaStartedWithoutInterface == true)
            {
                Console.WriteLine("Saving project.");
                project.Save();
                project.Close();
            }
            else
            {
                Console.WriteLine("Please save project within TIAP.");
            }

            try
            {
                tiaPortal.Dispose();
            }
            catch
            {

            }


        }

        #region messaging
        public static void CancelGeneration(string message, Exception e = null)
        {
            //MessageBox.Show(message);
            //GenerateText = notInProgressText;
            //ProgressMessage = "";
            Console.WriteLine("");
            Console.WriteLine(message);
            if (e != null)
            {
                Console.WriteLine(e.Message);
            }
            //Console.ReadLine();
            try
            {
                tiaPortal.Dispose();
            }
            catch
            {

            }
            Console.WriteLine("");
        }

        public static void Progress(string message)
        {
            //MessageBox.Show(message);
            //GenerateText = notInProgressText;
            //ProgressMessage = "";
            Console.WriteLine(message);
        }
        public static void FaultMessage(string message, Exception ex = null, string functionName = "")
        {
            //MessageBox.Show(message);
            //GenerateText = notInProgressText;
            //ProgressMessage = "";
            Console.WriteLine("");
            Console.WriteLine(message);
            if (functionName!="")
            {
                Console.WriteLine("Exception in " + functionName + " : ");
            }
            if (ex != null)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("");
        }

        #endregion
    }
}
