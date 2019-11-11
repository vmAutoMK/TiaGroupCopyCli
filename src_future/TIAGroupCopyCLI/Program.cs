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
using Siemens.Engineering.Library.MasterCopies;

using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

using Siemens.Engineering.Library.MasterCopies;

using TiaOpennessHelper.Utils;
using TIAHelper.Services;
using TIAGroupCopyCLI.Models;
//using TIAGroupCopyCLI.Devices;
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


            Progress("==============");

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
            Parameters.ProjectPath = "D:\\KnesMX\\source\\TIA\\Groups\\Groups.ap15_1";
            Parameters.TemplateGroupName = "Group_";


            if (!Parameters.ParameterOK)
            {
                CancelGeneration("");
                Console.ReadLine();
                return;
            }

            RunTiaPortal();

            Console.WriteLine("");
            Console.WriteLine("Hit enter to exit");
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

            #region ******testing********
            DeviceUserGroup testGroup = project.DeviceGroups.Find(Parameters.TemplateGroupName);
            IList<Device> testPlcs= Service.GetPlcDevicesInGroup(testGroup);
            ManagePlc testManagePlcs = new ManagePlc(testPlcs[0]);

            SaveAttributeGroup x = new SaveAttributeGroup(testManagePlcs);
            int foundCounter = 0;
            //x.SaveDeviceAttribute(testPlcs[0], "TypeName");
            //bool found = x.FindAndSaveDeviceItemAtribute(testPlcs[0], "InterfaceType");
            // foundCounter = x.FindAndSaveAllDeviceItemAtributes(testPlcs[0], "Name");
            //foundCounter = x.FindAndSaveAllDeviceItemAtributes(testPlcs[0], "Failsafe_FDestinationAddress");
            foundCounter = x.FindAndSaveAllAddressAtributes(testPlcs[0], "StartAddress");
            x[2].RestoreAttribute(testPlcs[0]);

            #endregion testing


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
            templateGroup.Name = templateGroup.Name + groupCounter.ToString(indexformat);
            //templateNetworkInterface.IoControllers[0].IoSystem.Name = currentPrefix + temlateIoSystemName;

            ChangeDeviceNames(templateGroup, currentPrefix);
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
                    CancelGeneration("Could not create new Group", e);
                    return;
                }

                #endregion

                newGroup.Name = newGroup.Name + groupCounter.ToString(indexformat); ;
                ChangeDeviceNames(newGroup, currentPrefix);


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
                plcs.SetAllIDeviceParnerAdresses();

                ioDevices.ChangeIpAddresses(groupCounter - 1);
                ioDevices.SwitchIoSystem(templatePlcs.originalSubnet, plcs.newIoSystem);
                //ioDevices.AdjustFDestinationAddress(Parameters.FDestAddrOffset, (ulong)templatePlcs.LowerBoundForFDestinationAddresses_attribues.Value, (ulong)templatePlcs.UpperBoundForFDestinationAddresses_attribues.Value);
                ioDevices.Restore();

                hmis.ChangeIpAddresses(groupCounter - 1);
                hmis.DisconnectFromSubnet();
                hmis.ConnectToSubnet(templatePlcs.originalSubnet);
                hmis.Restore();

                drives.ChangeIpAddresses(groupCounter - 1);
                drives.SwitchIoSystem(templatePlcs.originalSubnet, plcs.newIoSystem);
                drives.AdjustFDestinationAddress(Parameters.FDestAddrOffset * (groupCounter - 1), (ulong)templatePlcs.LowerBoundForFDestinationAddresses_attribues.Value, (ulong)templatePlcs.UpperBoundForFDestinationAddresses_attribues.Value);
                drives.Restore();


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

        public static void ChangeDeviceNames(DeviceUserGroup aDeviceUserGroup, string aPrefix)
        {

            if (aDeviceUserGroup != null)
            {
                //get PLCs in sub folders - recursive
                foreach (Device device in aDeviceUserGroup.Devices)
                {
                    try
                    {
                        //change device name 
                        device.DeviceItems[1].Name = aPrefix + device.DeviceItems[1].Name;

                    }
                    catch
                    {

                    }
                }


                //get PLCs in sub folders - recursive
                foreach (DeviceUserGroup deviceUserGroup in aDeviceUserGroup.Groups)
                {
                    ChangeDeviceNames(deviceUserGroup, aPrefix);
                }

            }



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
