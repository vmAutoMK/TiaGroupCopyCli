using System;
using System.Reflection;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Hmi;
using HmiTarget = Siemens.Engineering.Hmi.HmiTarget;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Cycle;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.Hmi.Globalization;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.Hmi.RuntimeScripting;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Library;
using Siemens.Engineering.MC.Drives;

using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

using Siemens.Engineering.Library.MasterCopies;

using TiaOpennessHelper.Utils;
using TIAHelper.Services;
using TIAGroupCopyCLI.Devices;
using TIAGroupCopyCLI.Plcs;
using TIAGroupCopyCLI.Drives;
using TIAGroupCopyCLI.Hmis;
using TIAGroupCopyCLI.Io;
using TIAGroupCopyCLI.Para;



//using System.Windows.Forms;
//string pructverion2 = Application.ProductVersion;

namespace TIAGroupCopyCLI //TIAGroupCopyCLI
{
    class Program
    {


        static Parameters Parameters;

        private static TiaPortal tiaPortal;
        private static Project project;

        static void Main(string[] args)
        {
            MyResolverClass.AddAssemblyResolver();
            //AppDomain.CurrentDomain.AssemblyResolve += MyResolverClass.MyResolver;
            //AppDomain.CurrentDomain.AssemblyResolve += Resolver.OnResolve;
            Heandlers.AddAppExceptionHaenlder();

            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            Progress("TIA Group copy v" + fileVersion);
            Progress("This beta version is a customized solution for now");



            //var x = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            //var y = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Progress("================================================================");

            Parameters = new Parameters(args);
           
            /*
            Parameters.ProjectPath = "D:\\KnesMX\\source\\TIA\\Groups\\Groups.ap15_1";
            Parameters.TemplateGroupName = "Group_";
            Parameters.Prefix = "sk";
            Parameters.NumOfGroups = 3;
            Parameters.FBaseAddrOffset = 1;
            Parameters.FDestAddrOffset = 100;
            Parameters.IDeviceDeviceNumberOffset = 1;
            Parameters.IDeviceIoAddressOffset = 100;
            */



            if (!Parameters.ParameterOK)
            {
                Console.ReadLine();
                return;
            }



            RunTiaPortal();




            Console.WriteLine("");
            Console.WriteLine("Hit enter to exit.");
            Console.ReadLine();
        }

        private static void RunTiaPortal()
        {

            #region tia and project
            Progress("Check running TIA Portal");
            bool tiaStartedWithoutInterface = false;

            Service.AttachToTIA(Parameters.ProjectPath, ref tiaPortal, ref project);
                        
            if ((tiaPortal == null) || (project == null))
            {
                Service.OpenProject(Parameters.ProjectPath, ref tiaPortal, ref project);
                tiaStartedWithoutInterface = true;
            }
            if ((tiaPortal == null) || (project == null))
            {
                CancelGeneration("Could not open project.");
                return;
            }

            Progress(String.Format("Project {0} is open", project.Path.FullName));
            #endregion

            #region master copy
            Progress("Creating master copy.");

            DeviceUserGroup templateGroup = project.DeviceGroups.Find(Parameters.TemplateGroupName);
            if (templateGroup == null)
            {
                CancelGeneration("Group not found.");
                return;
            }

            //=======copy to master copy========
            MasterCopyComposition masterCopies = project.ProjectLibrary.MasterCopyFolder.MasterCopies;
            MasterCopy templateCopy = masterCopies.Create(templateGroup);
            if (templateCopy == null)
            {
                CancelGeneration("Could not create master copy.");
                return;
            }
            MasterCopy deleteCopy = project.ProjectLibrary.MasterCopyFolder.MasterCopies.Find(templateCopy.Name);
            #endregion

            #region get basic info from template group
            ManagePlc templatePlcs = new ManagePlc(templateGroup);
            templatePlcs.GetAllIDeviceParnerAdresses();

            if (templatePlcs.AllDevices.Count != 1)
            {
                CancelGeneration("No PLC or more than 1 PLC in group.");
                return;
            }

            #endregion

            #region change name and IP of first group (template Group)
            string indexformat = "D2";
            uint groupCounter = 1;

            Progress("Adjusting template group.");
            string currentPrefix = Parameters.Prefix + groupCounter.ToString(indexformat);
            //templateGroup.Name = templateGroup.Name + groupCounter.ToString(indexformat);
            templateGroup.Name = Parameters.NewGroupNamePrefix + groupCounter.ToString(indexformat);
            //templateNetworkInterface.IoControllers[0].IoSystem.Name = currentPrefix + temlateIoSystemName;

            Service.ChangeDeviceNames(templateGroup, currentPrefix);
            templatePlcs.ChangeIoSystemName(currentPrefix);

            #endregion

            #region copy group loop
            DeviceUserGroupComposition userGroups = project.DeviceGroups;
            //groupCounter++;
            while (++groupCounter <= Parameters.NumOfGroups)
            {
                #region copy group
                Progress("Creating Group " + groupCounter);
                currentPrefix = Parameters.Prefix + groupCounter.ToString(indexformat);

                DeviceUserGroup newGroup;
                try
                {
                    newGroup = userGroups.CreateFrom(templateCopy);

                }
                catch(Exception e)
                {
                    CancelGeneration("Could not create new Group");
                    return;
                }

                #endregion

                //newGroup.Name = newGroup.Name + groupCounter.ToString(indexformat); ;
                newGroup.Name = Parameters.NewGroupNamePrefix + groupCounter.ToString(indexformat); ;
                Service.ChangeDeviceNames(newGroup, currentPrefix);


                ManagePlc plcs = new ManagePlc(newGroup);
                ManageHmi hmis = new ManageHmi(newGroup);
                ManageDrive drives = new ManageDrive(newGroup);
                IList<Device> allDevices = Service.GetAllDevicesInGroup(newGroup);
                IList<Device> tempIoDevices = allDevices.Except(hmis.AllDevices).Except(drives.AllDevices).ToList();
                tempIoDevices.Remove(plcs.AllDevices[0]);
                ManageIo ioDevices = new ManageIo(tempIoDevices);


                plcs.ChangeIpAddresses(groupCounter - 1);
                plcs.CreateNewIoSystem(templatePlcs.originalSubnet, currentPrefix);
                plcs.ConnectToMasterIoSystem(templatePlcs.originalIoSystem);
                plcs.GetAllIDeviceParnerAdresses();
                plcs.CopyFromTemplate(templatePlcs);
                plcs.AdjustFSettings(Parameters.FBaseAddrOffset * (groupCounter - 1), Parameters.FDestAddrOffset * (groupCounter - 1));
                plcs.AdjustPartnerAddresses(Parameters.IDeviceIoAddressOffset * (groupCounter - 1));
                plcs.Restore();
                plcs.ChangePnDeviceNames(currentPrefix);
                plcs.SetAllIDeviceParnerAdresses();

                ioDevices.ChangeIpAddresses(groupCounter - 1);
                ioDevices.SwitchIoSystem(templatePlcs.originalSubnet, plcs.newIoSystem);
                //ioDevices.AdjustFDestinationAddress(Parameters.FDestAddrOffset, (ulong)templatePlcs.LowerBoundForFDestinationAddresses_attribues.Value, (ulong)templatePlcs.UpperBoundForFDestinationAddresses_attribues.Value);
                ioDevices.Restore();
                ioDevices.ChangePnDeviceNames(currentPrefix);

                hmis.ChangeIpAddresses(groupCounter - 1);
                hmis.DisconnectFromSubnet();
                hmis.ConnectToSubnet(templatePlcs.originalSubnet);
                hmis.Restore();
                hmis.ChangePnDeviceNames(currentPrefix);

                drives.ChangeIpAddresses(groupCounter - 1);
                drives.SwitchIoSystem(templatePlcs.originalSubnet, plcs.newIoSystem);
                drives.AdjustFDestinationAddress(Parameters.FDestAddrOffset * (groupCounter - 1), (ulong)templatePlcs.LowerBoundForFDestinationAddresses_attribues.Value, (ulong)templatePlcs.UpperBoundForFDestinationAddresses_attribues.Value);
                drives.Restore();
                drives.ChangePnDeviceNames(currentPrefix);

                plcs.SetAllToConnections();


                plcs.DelecteOldSubnet();
                //deleteNetworkSubnet.Delete();

            }

            #endregion


            deleteCopy.Delete();

            Console.WriteLine("Copy complete.");
            if (tiaStartedWithoutInterface == true)
            {
                Console.WriteLine("Saving project.");
                project.Save();
                project.Close();
            }
            else
            {
                Console.WriteLine("Please save project with TIAP");
            }

            
            tiaPortal.Dispose();



        }


        public static void CancelGeneration(string message, Exception e = null)
        {
            //MessageBox.Show(message);
            //GenerateText = notInProgressText;
            //ProgressMessage = "";
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
        }

        public static void Progress(string message)
        {
            //MessageBox.Show(message);
            //GenerateText = notInProgressText;
            //ProgressMessage = "";
            Console.WriteLine(message);

        }

    }
}
