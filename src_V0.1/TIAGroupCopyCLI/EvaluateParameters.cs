using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TIAGroupCopyCLI.Para
{

    class Parameters
    {
        #region Filed
        public string ProjectPath;
        public string TemplateGroupName;
        public string NewGroupNamePrefix;
        public string Prefix = "";
        public uint NumOfGroups = 0;
        public uint FBaseAddrOffset = 0;
        public uint FDestAddrOffset = 0;
        //public uint IDeviceDeviceNumberOffset = 0;
        public uint IDeviceIoAddressOffset = 0;
        public bool ParameterOK = false;
        #endregion Filed

        #region Constructor
        public Parameters(string[] aArgs)
        {

            int currentArgIdx = 0;
            #region mandatory Argument 1 to 4
            if ( (aArgs == null) || (aArgs.Count() == 0) || (aArgs.Count() < 4) )
            {
                Program.Progress("Not enough parameters.");
                Description();
                return;
            }
            


            if  ( (aArgs[0] == @"\?") || (aArgs[0] == @"/?") || (aArgs[0] == "?") )
            {
                Description();
                return;
            }
            

            if (! File.Exists(aArgs[currentArgIdx]))
            {
                Program.Progress("File " + aArgs[currentArgIdx] + " does not exisits!");
                Description();
                return;
            }
            ProjectPath = aArgs[currentArgIdx];

            NewGroupNamePrefix = aArgs[++currentArgIdx];
            TemplateGroupName = NewGroupNamePrefix;

            //char idx = TemplateGroupName[TemplateGroupName.Length - 1];// TemplateGroupName.LastIndexOf(" ");
            while (TemplateGroupName[TemplateGroupName.Length - 1].Equals(' '))
            {
                TemplateGroupName = TemplateGroupName.Substring(0, TemplateGroupName.Length - 1);
                //idx = TemplateGroupName[TemplateGroupName.Length - 1];
            }
            


            //TemplateGroupName = aArgs[++currentArgIdx];



            Prefix = aArgs[++currentArgIdx];

            try
            {
                NumOfGroups = UInt32.Parse(aArgs[++currentArgIdx]);
            }
            catch (Exception e)
            {
                Program.FaultMessage("Parameters NumOfGroups = " + aArgs[currentArgIdx] + " could not be converted to a number. ", e);
                Description();
                return;
            }

            
            if (NumOfGroups < 1 )
            {
                Program.FaultMessage("Parameters NumOfGroups = " + NumOfGroups + " too small .");
                Description();
                return;
            }else if (NumOfGroups > 1000)
            {
                Program.FaultMessage("Parameters NumOfGroups = " + NumOfGroups + " too larg (max 999 ");
                Description();
                return;
            }

            #endregion

            #region Agument FBaseAddrOffset
            currentArgIdx++;
            if (aArgs.Count() > currentArgIdx)
            {
                try
                {
                    FBaseAddrOffset = UInt32.Parse(aArgs[currentArgIdx]);
                }
                catch (Exception e)
                {
                    Program.FaultMessage("Parameters FBaseAddrOffset = " + aArgs[currentArgIdx] + " could not be converted to a number. ",e );
                    Description();
                    return;
                }
            }
            #endregion

            #region Agument FDestAddrOffset
            currentArgIdx++;
            if (aArgs.Count() > currentArgIdx)
            {
                try
                {
                    FDestAddrOffset = UInt32.Parse(aArgs[currentArgIdx]);
                }
                catch (Exception e)
                {
                    Program.FaultMessage("Parameters FDestAddrOffset = " + aArgs[currentArgIdx] + " could not be converted to a number. ",e);
                    Description();
                    return;
                }
            }
            #endregion

            #region Agument FDestAddrOffset
            if (aArgs.Count() > currentArgIdx)
            {
                //currentArgIdx = 10;  //test exeception
                try
                {
                    IDeviceIoAddressOffset = UInt32.Parse(aArgs[currentArgIdx]);
                }
                catch (Exception e)
                {
                    Program.FaultMessage("Parameters FDestAddrOffset = " + aArgs[currentArgIdx] + " could not be converted to a number. ",e);
                    Description();
                    return;
                }
            }
            #endregion
           
            PrintSettings();
            ParameterOK = true;
        }

        #endregion Constructor

        #region Methods
        private void Description()
        {
            
            Program.Progress("");
            Program.Progress("TIAGroupCopyCLI.exe ProjectPath GroupName Prefix NumberOfGroups FBaseAddrOffset FDestAddrOffset IDeviceDeviceNoOffset IDeviceIoAddrOffset");
            Program.Progress("");
            Program.Progress("Parameters:");
            Program.Progress("1. ProjectPath           = path and name of project");
            Program.Progress("                           (e.g. C:\\Projects\\MyProject\\MyProjects.ap15_1)");
            Program.Progress("2. GroupName             = name of exiting template group in project");
            Program.Progress("                           (e.g. Group_ ");
            Program.Progress("3. Prefix                = Text to be added in fron of device name");
            Program.Progress("                           (e.g. AGV, so _plc will become AGV01_plc");
            Program.Progress("4. NumberOfGroups        = how many groups do you want to end up with");
            Program.Progress("                           including the template group");
            Program.Progress("5. FBaseAddrOffset       = by what increment should the central FBaseAddr");
            Program.Progress("                           of the PLC be increamented");
            Program.Progress("6. FDestAddrOffset       = by what increment should the type 1 F-Dest Address");
            Program.Progress("                           of each module be increased as well as the lower");
            Program.Progress("                           and uper limit setting of type 1 F-DestAddresses)");
            //Program.Progress("7. IDeviceDeviceNoOffset = by what increment should the iDevice DeviceNumber for each PLC be increased");
            Program.Progress("7. IDeviceIoAddrOffset   = by what increment should the io address for the");
            Program.Progress("                           iDevice connection to the master PLC be incremented");
            Program.Progress("                           (on the master PLC side)");
            Program.Progress("");
            Program.Progress("Example:");
            Program.Progress("TIAGroupCopyCLI.exe  C:\\Projects\\MyProject\\MyProjects.ap15_1  Group_ AGV 60 1 50 1 100");
            Program.Progress("");

            ParameterOK = false;
        }

        private void PrintSettings()
        {

            Program.Progress("");
            Program.Progress("The tool Starts with the following settings:");
            Program.Progress("");
            Program.Progress("ProjectPath           = " + ProjectPath);
            Program.Progress("GroupName             = " + TemplateGroupName);
            Program.Progress("Prefix                = " + Prefix);
            Program.Progress("NumberOfGroups        = " + NumOfGroups);
            Program.Progress("FBaseAddrOffset       = " + FBaseAddrOffset);
            Program.Progress("FDestAddrOffset       = " + FDestAddrOffset);
            //Program.Progress("IDeviceDeviceNoOffset = " + IDeviceDeviceNumberOffset);
            Program.Progress("IDeviceIoAddrOffset   = " + IDeviceIoAddressOffset);
            Program.Progress("");

            ParameterOK = false;
        }

        #endregion Methods

    }
}
