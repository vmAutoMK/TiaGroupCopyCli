using System;
using System.Reflection;
//using System.IO;
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
using TIAGroupCopyCLI.Models.template;
using TIAGroupCopyCLI.MessagingFct;
using TIAGroupCopyCLI.AppExceptions;

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
            Messaging.Progress("TIA Group copy v" + fileVersion);
            Messaging.Progress("This beta version is a customized solution for now");


            Messaging.Progress("================================================================");






            Run(args);




            Console.WriteLine("");
            Console.WriteLine("Hit enter to exit.");
            Console.ReadLine();
        }

        //=================================================================================================
        private static void Run(string[] args)
        {
            try
            {
                Parameters = new Parameters(args);
            }
            catch (TIAGroupCopyCLI.AppExceptions.ParameterException)
            {
                return;
            }


            if (!Heandlers.SelectAssmebly(Parameters.ProjectVersion, TIAP_VERSION_USED_FOR_TESTING, OPENESS_VERSION_USED_FOR_TESTING))
            {
                return;
            }
            Heandlers.AddAssemblyResolver();


            try
            {
                GroupCopy();
            }
            catch (TIAGroupCopyCLI.AppExceptions.GroupCopyException e)
            {
                Messaging.FaultMessage(e.Message);
                return;
            }
            finally
            {
                tiaPortal.Dispose();
            }

        }


        //=================================================================================================
        private static void GroupCopy()
        {
            uint groupCounter = 1;

           #region tia and project

            Messaging.Progress("Check running TIA Portal");
            bool tiaStartedWithoutInterface = false;

            Service.OpenProject(Parameters.ProjectPath, ref tiaPortal, ref project, ref tiaStartedWithoutInterface);

            if ((tiaPortal == null) || (project == null))
            {
                throw new GroupCopyException("Could not open project.");
            }

            Messaging.Progress($"Project {project.Path.FullName} is open");

            #endregion


            #region get template group
            Messaging.Progress("Searching for template group.");

            ManageTemplateGroup xxx =  ManageTemplateGroup.CreateTemplate(project, Parameters.TemplateGroupName, Parameters.TemplateGroupNumber , Parameters.DevicePrefix);

            throw new GroupCopyException("TEMP END OF PROGRAM");

            DeviceUserGroup tiaTemplateGroup = project.DeviceGroups.Find(Parameters.TemplateGroupName);
            if (tiaTemplateGroup == null)
            {
                //CancelGeneration("Group not found.");
                return;
            }

            ManageGroup templateGroup = new ManageGroup(tiaTemplateGroup, Parameters.GroupNamePrefix, Parameters.DevicePrefix, groupCounter, Parameters.IndexFormat);
            if (templateGroup.Devices.Where(d => d.DeviceType == DeviceType.Plc).Count() != 1)
            {
                //CancelGeneration("No PLC or more than 1 PLC in group.");
                return;
            }
            //templateGroup.SavePlcConfigInTemplate();
            templateGroup.SaveConfig();





            //=======copy to master copy========
            //MasterCopyComposition masterCopies = project.ProjectLibrary.MasterCopyFolder.MasterCopies;
            MasterCopy templateCopy = null;
            try
            {

                templateCopy = project.ProjectLibrary.MasterCopyFolder.MasterCopies.Create(tiaTemplateGroup);
            }
            catch (Exception ex)
            {
                //CancelGeneration("Could not create master copy.",ex);
                return;
            }

            if (templateCopy == null)
            {
                //CancelGeneration("Could not create master copy.");
                return;
            }

            MasterCopy deleteMasterCopy = project.ProjectLibrary.MasterCopyFolder.MasterCopies.Find(templateCopy.Name);
            #endregion

            #region get basic info from template group



            //templateGroup.SavePlcConfigInTemplate();



            #endregion

            #region change name and IP of first group (template Group)


            Messaging.Progress("Adjusting template group.");
            templateGroup.ChangeNames();
           
            #endregion


            #region copy group loop
            DeviceUserGroupComposition tiaUserGroups = project.DeviceGroups;
 
            while (++groupCounter <= Parameters.NumOfGroups)
            {
                #region copy group
                Messaging.Progress("Creating Group " + groupCounter);
               // currentPrefix = Parameters.Prefix + groupCounter.ToString(indexFormat);

                DeviceUserGroup newTiaGroup;
                ManageGroup newGroup;
                try
                {
                    newTiaGroup = tiaUserGroups.CreateFrom(templateCopy);
                    if (newTiaGroup == null)
                    {
                        //CancelGeneration("Could not create new Group.");
                        return;
                    }
                    else
                    {
                        newGroup = new ManageGroup(newTiaGroup, Parameters.GroupNamePrefix, Parameters.DevicePrefix, groupCounter, Parameters.IndexFormat);
                    }
                }
                catch(Exception e)
                {
                    //CancelGeneration("Could not create new Group.", e);
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
                Messaging.FaultMessage("Could not delete Mastercopy.", ex);
            }

            Messaging.Progress("");

            Messaging.Progress("Copy complete.");
            if (tiaStartedWithoutInterface == true)
            {
                Messaging.Progress("Saving project.");
                project.Save();
                project.Close();
            }
            else
            {
                Messaging.Progress("Please save project within TIAP.");
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
        public static void xCancelGeneration(string message, Exception e = null)
        {
            //MessageBox.Show(message);
            //GenerateText = notInProgressText;
            //ProgressMessage = "";
            Messaging.Progress("");
            Messaging.Progress(message);
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
            Messaging.Progress("");
        }



        #endregion
    }
}
