﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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


using TiaOpennessHelper.Utils;
using TIAHelper.Services;

namespace TIAGroupCopyCLI.Models
{


    class ManageDevice
    {
        Device Device;

        int[] Location;

        ManageGroup Parent;

        //temporarly************************************************************************************

            

        public ManageDevice() { }
        public ManageDevice(Device device)
        {
            AllDevices = new List<Device>();
            AllDevices.Add(device);
            FirstPnNetworkInterfaces = (List<NetworkInterface>)Service.GetPnInterfaces(device);
        }
        public ManageDevice(IList<Device> aDevices) { }

        public List<Device> AllDevices;
        public List<NetworkInterface> FirstPnNetworkInterfaces;
        public void ChangeIpAddresses(uint offset)   {  }
        public void SwitchIoSystem(Subnet x, IoSystem io) {  }
        public void DisconnectFromSubnet()  { }
        public void ConnectToSubnet(Subnet x) {  }

        //**************************************************************************************************

    }


    interface IManageDevice
    {

    }
}
