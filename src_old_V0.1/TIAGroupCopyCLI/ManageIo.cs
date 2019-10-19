using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

using TiaOpennessHelper.Utils;
using TIAHelper.Services;
using TIAGroupCopyCLI.Devices;

namespace TIAGroupCopyCLI.Io
{

    #region temp ??? public class DeviceItemAndAttributes
    /*
    public class DeviceItemAndAttributes
    {
        public DeviceItem DeviceItem;
        public AttributeValue FDestinationAddr;
        private List<AttributeValue> _startAddress;
        public List<AttributeValue> StartAddress
        {
            get
            {
                if (_startAddress == null)
                {
                    _startAddress = new List<AttributeValue>();
                }
                return _startAddress;
            }
            set
            {
                if (_startAddress == null)
                {
                    _startAddress = new List<AttributeValue>();
                }
                _startAddress = value;
            }
        }

        public DeviceItemAndAttributes(DeviceItem aDeviceItem)
        {
            DeviceItem = aDeviceItem;
            if (aDeviceItem != null)
            {
                FDestinationAddr = Service.GetAttribute(aDeviceItem, "Failsafe_FDestinationAddress");
                _startAddress = Service.GetAttributes(aDeviceItem.Addresses, "StartAddress");
            }

        }

        public void SaveFDestAndIoAddresses()
        {
            if (DeviceItem != null)
            {
                FDestinationAddr = Service.GetAttribute(DeviceItem, "Failsafe_FDestinationAddress");
                _startAddress = Service.GetAttributes(DeviceItem.Addresses, "StartAddress");
            }

        }

        public void RestoreFDestAndIoAddresses()
        {
            if (DeviceItem != null)
            {
                if (FDestinationAddr != null)
                {
                    Service.SetAttribute(DeviceItem, "Failsafe_FDestinationAddress", FDestinationAddr);
                }
                int i = 0;
                foreach (Address currentAddress in DeviceItem.Addresses)
                {
                    Service.SetAttribute(currentAddress, "StartAddress", _startAddress[i]);
                    i++;
                }
            }

        }

    }
    */
    #endregion

    class ManageIo : ManageDevice
    {

        private List<AttributeAndAddress> _allStartAddresses;
        public List<AttributeAndAddress> AllStartAddresses
        {
            get
            {
                if (_allStartAddresses == null)
                {
                    _allStartAddresses = new List<AttributeAndAddress>();
                }
                return _allStartAddresses;
            }
            set
            {
                if (_allStartAddresses == null)
                {
                    _allStartAddresses = new List<AttributeAndAddress>();
                }
                _allStartAddresses = value;
            }
        }


        #region  Constructor

        public ManageIo(Device aDevice) : base(aDevice)
        {
            //AllDevices.Add(aDevice);
            Save();
        }
        public ManageIo(IList<Device> aDevices) : base(aDevices)
        {
            //AllDevices.AddRange(aDevices);
            Save();
        }

        public ManageIo(DeviceUserGroup aGroup)
        {
            //AllDevices.AddRange(aDevices);
            IList<Device> devices = Service.GetAllDevicesInGroup(aGroup);
            AllDevices = (List<Device>)devices;
            Save();
        }
        #endregion

        #region Methodes
        public void Save()
        {
            List<AttributeAndAddress> returnStartAddressAndAddressObjects = new List<AttributeAndAddress>();
            IList<AttributeAndAddress> addStartAddressAndAddressObjects;

            foreach (Device currentDevice in AllDevices)
            {
                addStartAddressAndAddressObjects =  (List<AttributeAndAddress>)Service.GetValueAndAddressWithAttribute(currentDevice.DeviceItems, "StartAddress");
                returnStartAddressAndAddressObjects.AddRange(addStartAddressAndAddressObjects);
            }
            AllStartAddresses = returnStartAddressAndAddressObjects;
        }

        
        public void Restore()
        {
            foreach (AttributeAndAddress currentAddress in AllStartAddresses)
            {

                currentAddress.RestoreValue();
            }

            /*
            int i = 0;
            foreach (NetworkInterface currentNetworkInterface in FirstPnNetworkInterfaces)
            {
                if ( currentNetworkInterface.IoConnectors.Count > 0 ) 
                {
                    PnDeviceNumberOfFirstPnNetworkInterfaces[i].AddToValue(10);
                    Service.SetAttribute(currentNetworkInterface.IoConnectors[0], PnDeviceNumberOfFirstPnNetworkInterfaces[i]);
                    i++;
                }
            }
            */
        }

        #endregion

    }
}
