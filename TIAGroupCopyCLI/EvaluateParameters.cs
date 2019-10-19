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
        public string ProjectPath;
        public string TemplateGroupName;
        public string Prefix = "";
        public uint NumOfGroups = 0;
        public uint FBaseAddrOffset = 0;
        public uint FDestAddrOffset = 0;
        //public uint IDeviceDeviceNumberOffset = 0;
        public uint IDeviceIoAddressOffset = 0;
        public bool ParameterOK = false;


        public Parameters(string[] aArgs)
        {

            #region mandatory Argument 1 to 4
            if ( (aArgs.Count() == 0) || (aArgs.Count() < 4) )
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

            
            if (! File.Exists(aArgs[0]))
            {
                Program.Progress("File " + aArgs[0] + " does not exisits!");
                Description();
                return;
            }
            ProjectPath = aArgs[0];
            TemplateGroupName = aArgs[1];
            Prefix = aArgs[2];

            try
            {
                NumOfGroups = UInt32.Parse(aArgs[3]);
            }
            catch (Exception e)
            {
                Program.Progress("Parameters NumOfGroups = " + aArgs[3] + " could not be converted to a number. ");
                Program.Progress(e.Message);
                Description();
                return;
            }
            
            if (NumOfGroups < 1 )
            {
                Program.Progress("Parameters NumOfGroups = " + NumOfGroups + " too small ");
                Description();
                return;
            }else if (NumOfGroups > 1000)
            {
                Program.Progress("Parameters NumOfGroups = " + NumOfGroups + " too larg (max 999 ");
                Description();
                return;
            }

            #endregion
            #region Agument 5
            if (aArgs.Count() >= 5)
            {
                try
                {
                    FBaseAddrOffset = UInt32.Parse(aArgs[4]);
                }
                catch (Exception e)
                {
                    Program.Progress("Parameters FBaseAddrOffset = " + aArgs[4] + " could not be converted to a number. ");
                    Program.Progress(e.Message);
                    Description();
                    return;
                }
            }
            #endregion
            #region Agument 6
            if (aArgs.Count() >= 6)
            {
                try
                {
                    FDestAddrOffset = UInt32.Parse(aArgs[5]);
                }
                catch (Exception e)
                {
                    Program.Progress("Parameters FDestAddrOffset = " + aArgs[5] + " could not be converted to a number. ");
                    Program.Progress(e.Message);
                    Description();
                    return;
                }
            }
            #endregion
            /*
            #region Agument 7
            if (aArgs.Count() >= 7)
            {
                try
                {
                    IDeviceDeviceNumberOffset = UInt32.Parse(aArgs[6]);
                }
                catch (Exception e)
                {
                    Program.Progress("Parameters FDestAddrOffset = " + aArgs[6] + " could not be converted to a number. ");
                    Program.Progress(e.Message);
                    Description();
                    return;
                }
            }
            #endregion
            */
            #region Agument 7
            if (aArgs.Count() >= 7)
            {
                try
                {
                    IDeviceIoAddressOffset = UInt32.Parse(aArgs[7]);
                }
                catch (Exception e)
                {
                    Program.Progress("Parameters FDestAddrOffset = " + aArgs[7] + " could not be converted to a number. ");
                    Program.Progress(e.Message);
                    Description();
                    return;
                }
            }
            #endregion

            PrintSettings();
            ParameterOK = true;
        }

        private void Description()
        {
            
            Program.Progress("");
            Program.Progress("TIAGroupCopyCLI.exe ProjectPath GroupName Prefix NumberOfGroups FBaseAddrOffset FDestAddrOffset IDeviceDeviceNoOffset IDeviceIoAddrOffset");
            Program.Progress("");
            Program.Progress("Parameters:");
            Program.Progress("1. ProjectPath           = path and name of project (e.g. C:\\Projects\\MyProject\\MyProjects.ap15_1)");
            Program.Progress("2. GroupName             = name of exiting template group in project (e.g. Group_ ");
            Program.Progress("3. Prefix                = Text to be added in fron of device name (e.g. AGV, so _plc will become AGV01_plc");
            Program.Progress("4. NumberOfGroups        = how many groups do you want to end up with including the template group");
            Program.Progress("5. FBaseAddrOffset       = by what increment should the central FBaseAddr of the PLC be increamented");
            Program.Progress("6. FDestAddrOffset       = by what increment should the type 1 F-Dest Address of each module be increased");
            Program.Progress("                           as well as the lower and uper limit setting of type 1 F-DestAddresses)");
            //Program.Progress("7. IDeviceDeviceNoOffset = by what increment should the iDevice DeviceNumber for each PLC be increased");
            Program.Progress("7. IDeviceIoAddrOffset   = by what increment should the io address for the iDevice connection to the");
            Program.Progress("                           master PLC be incremented (on the master PLC side)");

            Program.Progress("Example:");
            Program.Progress("TIAGroupCopyCLI.exe  C:\\Projects\\MyProject\\MyProjects.ap15_1  Group_ AGV 60 1 50 1 100");
            Program.Progress("");

            ParameterOK = false;
        }

        private void PrintSettings()
        {

            Program.Progress("");
            Program.Progress("Tool Starts with the following settings:");
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
    }
}
