using System;
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


    class ManageDevice : ParentInstance , TiaInstance
    {

        #region Fields
        private Device _TiaDevice;
        public IEngineeringObject TiaEngineeringObject 
        {
            get
            {
                return _TiaDevice;
            }
        }

        public List<int> Location  { get; }

        public ParentInstance Parent { get; }

        public string OriginalName { get; }
        public string TemplatelName { get; }
        public string OriginalTiaName { get; }
        public string TemplatelTiaName { get; }

        private SaveAttributeGroup _allStartAddresses;

        #endregion Fields

        #region Constructor
        public ManageDevice(ManageGroup parent, Device tiaDevice, List<int> location)
        {
            Parent = parent;
            _TiaDevice = tiaDevice;
            Location = location;
            OriginalName = _TiaDevice.Name;
            OriginalTiaName = _TiaDevice?.DeviceItems[1]?.Name ?? "None";
            _allStartAddresses = new SaveAttributeGroup(this);

        }
        #endregion

        #region Methods
        

        #endregion Methods

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


    interface IManageDevice : ParentInstance, TiaInstance
    {

    }
}
