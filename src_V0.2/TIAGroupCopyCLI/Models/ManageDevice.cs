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


    class ManageDevice
    {

        #region References to openness object and managed objcts

        public Device Device { get; set; }
        public List<ManageNetworkInterface> NetworkInterfaces { get; set; }  = new List<ManageNetworkInterface>();

        #endregion

        #region Fields
        
        protected readonly ManageAttributeGroup FDestinationAddress_attribues = new ManageAttributeGroup();
        protected readonly string OriginalName;

        #endregion Fields

        #region constructors

        public ManageDevice(Device device)
        {
            Device = device;
            NetworkInterfaces = ManageNetworkInterface.GetAll_ManageNetworkInterfaceObjects(Device);
            try
            {
                OriginalName = Device.DeviceItems[1].Name;
            }
            catch { }
        }
        #endregion

        #region Methods


        public virtual void SaveConfig()
        {
            FDestinationAddress_attribues.FindAndSaveDeviceItemAtributes(Device, "Failsafe_FDestinationAddress");
            foreach(ManageNetworkInterface currentItem in NetworkInterfaces)
            {
                currentItem.SaveConfig();
            }
        }

        public virtual void Restore()
        {
            FDestinationAddress_attribues.Restore();
            foreach (ManageNetworkInterface currentItem in NetworkInterfaces)
            {
                currentItem.Restore();
            }
        }

        public void CopyFromTemplate(ManageDevice atemplateDevice)
        {

            for (int i = 0; i < atemplateDevice.FDestinationAddress_attribues.Count; i++)
            {
                FDestinationAddress_attribues[i].Value = atemplateDevice.FDestinationAddress_attribues[i].Value;
            }

            for (int i = 0; i < atemplateDevice.NetworkInterfaces.Count; i++)
            {
                NetworkInterfaces[i]?.CopyFromTemplate(atemplateDevice.NetworkInterfaces[i]);

                /*
                if (PnDeviceNumberOfFirstPnNetworkInterfaces.Count < i)
                {
                    PnDeviceNumberOfFirstPnNetworkInterfaces.Add(new AttributeInfo());
                }
                if (PnDeviceNumberOfFirstPnNetworkInterfaces[i] == null)
                {
                    PnDeviceNumberOfFirstPnNetworkInterfaces[i] = new AttributeInfo()
                    {
                        Name = "PnDeviceNumber"
                    };
                }

                PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value = aTemplatePlc.PnDeviceNumberOfFirstPnNetworkInterfaces[i]?.Value;
                */
            }
        }

        public virtual void RestoreConfig_WithAdjustments(string prefix, ulong pnDeviceNumberOffset, ulong fSourceOffset, ulong fDestOffset, ulong lowerFDest, ulong upperFDest)
        {
            foreach (SingleAttribute currentItem in FDestinationAddress_attribues) 
            {
                if (((ulong)currentItem.Value >= lowerFDest) && ((ulong)currentItem.Value <= upperFDest))
                {
                    currentItem.RestoreWithOffset(fDestOffset);
                }
            }
            foreach (ManageNetworkInterface currentItem in NetworkInterfaces)
            {
                currentItem.RestoreConfig_WithAdjustments(prefix, pnDeviceNumberOffset);
            }
        }

        public virtual void AdjustFDestinationAddress(ulong aFDestOffset, ulong aLower, ulong aUpper)
        {

            foreach (SingleAttribute item in FDestinationAddress_attribues)  //.Where(i => true)
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
                NetworkInterfaces[0].ChangePnDeviceName(aPrefix);
        }

        public void ChangeIpAddresse(ulong aIpOffset)
        {
            if (NetworkInterfaces.Count > 0)
            {
                    NetworkInterfaces[0].ChangeIpAddress(aIpOffset);
            }
        }

        public Subnet Get_Subnet()
        {

            if (NetworkInterfaces.Count > 0)
            {
                return  NetworkInterfaces[0].Get_Subnet();
            }
            return null;
        }

        public IoSystem Get_ioSystem()
        {
            if (NetworkInterfaces.Count > 0)
            {
                return NetworkInterfaces[0].Get_ioSystem();
            }
            return null;
        }

        public void Reconnect(Subnet subnet, IoSystem ioSystem)
        {
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaces[0].Reconnect(subnet, ioSystem);
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
