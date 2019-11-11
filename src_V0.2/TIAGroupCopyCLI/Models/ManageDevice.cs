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


    class ManageDevice : IDevice
    {
        public List<Device> AllDevices  = new List<Device>();
        public List<NetworkInterface> FirstPnNetworkInterfaces;
        public List<AttributeInfo> PnDeviceNumberOfFirstPnNetworkInterfaces;

        #region References to openness object and managed objcts

        public Device Device { get; }
        public List<ManageNetworkInterface> NetworkInterfaces { get; set; }  = new List<ManageNetworkInterface>();

        #endregion

        #region Fields
        protected readonly ManageAttributeGroup FDestinationAddress_attribues = new ManageAttributeGroup();

        #endregion Fields

        #region constructors

        public ManageDevice(Device device)
        {
            Device = device;
            NetworkInterfaces = ManageNetworkInterface.GetAll_ManageNetworkInterfaceObjects(Device);
            
        }
        #endregion

        #region Methods
        public virtual void Save()
        {
            FDestinationAddress_attribues.FindAndSaveDeviceItemAtributes(Device, "Failsafe_FDestinationAddress");
            foreach(ManageNetworkInterface currentItem in NetworkInterfaces)
            {
                currentItem.Save();
            }

        }

        public virtual  void Restore()
        {
            FDestinationAddress_attribues.Restore();
            foreach (ManageNetworkInterface currentItem in NetworkInterfaces)
            {
                currentItem.Restore();
            }
        }

        public virtual void AdjustFDestinationAddress(ulong aFDestOffset, ulong aLower, ulong aUpper)
        {

            foreach (SimpleAttribute item in FDestinationAddress_attribues)  //.Where(i => true)
            {
                if (((ulong)item.Value >= aLower) && ((ulong)item.Value <= aUpper))
                {
                    item.AddToValue(aFDestOffset);
                }
            }

        }

         public void ChangeTiaName(string aPrefix)
        {
            try
            {
                Device.DeviceItems[1].Name = aPrefix + Device.DeviceItems[1].Name;
            }
            catch
            {
            }
        }

        public void ChangePnDeviceName(string aPrefix)
        {
            if (NetworkInterfaces.Count > 0)
                NetworkInterfaces[0].RestorePnDeviceNamesWithPrefix(aPrefix);
        }

        public void ChangeIpAddresse(ulong aIpOffset)
        {
            if (NetworkInterfaces.Count > 0)
            {
                    NetworkInterfaces[0].ChangeIpAddresses(aIpOffset);
            }
        }

         public void SwitchIoSystem(Subnet aSubnet, IoSystem aIoSystem)
        {
            DisconnectFromSubnet();
            ConnectToSubnet(aSubnet);
            ConnectToIoSystem(aIoSystem);
        }

        public void DisconnectFromSubnet()
        {
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaces[0].DisconnectFromSubnet();
            }
        }

        public void ConnectToSubnet(Subnet aSubnet)
        {
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaces[0].ConnectToSubnet(aSubnet);
            }
        }

 
        public void ConnectToIoSystem(IoSystem aIoSystem)
        {
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaces[0].ConnectToIoSystem(aIoSystem);
            }
        }

        #endregion Methods

    }
}
