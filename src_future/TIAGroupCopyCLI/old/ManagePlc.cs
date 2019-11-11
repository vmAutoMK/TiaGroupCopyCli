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
//using TIAGroupCopyCLI.Devices;
using TIAGroupCopyCLI.Models;

namespace TIAGroupCopyCLI.Plcs
{

    public class TransferAreaAndAttributes
    {

        public TransferArea TransferArea;
        public AttributeValue PartnerStartAddress;


        public TransferAreaAndAttributes(TransferArea aTransferArea)
        {
            TransferArea = aTransferArea;
            if (aTransferArea != null)
            {
                PartnerStartAddress = Service.GetAttribute(aTransferArea.PartnerAddresses[0], "StartAddress");
            }
        }

        public void SavePartnerStartAddress()
        {
            if (TransferArea != null)
            {
                PartnerStartAddress = Service.GetAttribute(TransferArea.PartnerAddresses[0], "StartAddress");
            }

        }

        public void RestorePartnerStartAddress()
        {
            if (TransferArea != null)
            {
                if (PartnerStartAddress != null)
                {
                    Service.SetAttribute(TransferArea.PartnerAddresses[0], "StartAddress", PartnerStartAddress);
                }
            }

        }

    }

    class ManagePlc : ManageDevice, IManageDevice
    {

        private List<TransferAreaAndAttributes> _allIDevicePartnerAddrsses;
        public List<TransferAreaAndAttributes> AllIDevicePartnerAddrsses
        {
            get
            {
                if (_allIDevicePartnerAddrsses == null)
                {
                    _allIDevicePartnerAddrsses = new List<TransferAreaAndAttributes>();
                }
                return _allIDevicePartnerAddrsses;
            }
            set
            {
                if (_allIDevicePartnerAddrsses == null)
                {
                    _allIDevicePartnerAddrsses = new List<TransferAreaAndAttributes>();
                }
                _allIDevicePartnerAddrsses = value;
            }
        }


        public AttributeAndDeviceItem CentralFSourceAddress_attribue;
        public AttributeAndDeviceItem LowerBoundForFDestinationAddresses_attribues;
        public AttributeAndDeviceItem UpperBoundForFDestinationAddresses_attribues;
        public IList<AttributeAndDeviceItem> FDestinationAddress_attribues;
        public Subnet originalSubnet;
        public IoSystem originalIoSystem;
        public IoSystem newIoSystem;

        public ManagePlc(Device aDevice) : base(aDevice)
        {
            Save();
        }

        /*
        public ManagePlc(IList<Device> aDevices) : base(aDevices)
        {

        }
        */

        public ManagePlc(DeviceUserGroup aGroup)
        {
            IList<Device> devices = Service.GetPlcDevicesInGroup(aGroup);
            AllDevices = (List<Device>)devices;
            Save();
        }


        public void ChangeIoSystemName(string aPrefix)
        {
            //get PLCs in sub folders - recursive


            try
            {
                FirstPnNetworkInterfaces[0].IoControllers[0].IoSystem.Name = aPrefix + FirstPnNetworkInterfaces[0].IoControllers[0].IoSystem.Name;
            }
            catch
            {
            }
        }

        public void Save()
        {

            Device currentDevice = AllDevices[0];
            CentralFSourceAddress_attribue = Service.Get1ValueAndDeviceItemWithAttribute(currentDevice.DeviceItems, "Failsafe_CentralFSourceAddress");
            LowerBoundForFDestinationAddresses_attribues = Service.Get1ValueAndDeviceItemWithAttribute(currentDevice.DeviceItems, "Failsafe_LowerBoundForFDestinationAddresses");
            UpperBoundForFDestinationAddresses_attribues = Service.Get1ValueAndDeviceItemWithAttribute(currentDevice.DeviceItems, "Failsafe_UpperBoundForFDestinationAddresses");
            FDestinationAddress_attribues = Service.GetValueAndDeviceItemsWithAttribute(currentDevice.DeviceItems, "Failsafe_FDestinationAddress");

            originalSubnet = FirstPnNetworkInterfaces[0].Nodes[0].ConnectedSubnet;
            originalIoSystem = FirstPnNetworkInterfaces[0].IoConnectors[0].ConnectedToIoSystem;

        }

        public void Restore()
        {

            CentralFSourceAddress_attribue.Restore();
            LowerBoundForFDestinationAddresses_attribues.Restore();
            UpperBoundForFDestinationAddresses_attribues.Restore();

            ulong lower = (ulong)LowerBoundForFDestinationAddresses_attribues.Value;
            ulong upper = (ulong)UpperBoundForFDestinationAddresses_attribues.Value;

            foreach (AttributeAndDeviceItem item in FDestinationAddress_attribues)  //.Where(i => true)
            {
                if (((ulong)item.Value >= lower) && ((ulong)item.Value <= upper))
                {
                    item.Restore();
                }
            }
        }

        public void GetAllIDeviceParnerAdresses()
        {
            foreach (TransferArea currentTransferArea in FirstPnNetworkInterfaces[0].TransferAreas)
            {

                if (currentTransferArea.PartnerAddresses.Count >= 0)
                {
                    TransferAreaAndAttributes newTransferArea = new TransferAreaAndAttributes(currentTransferArea);
                    if (newTransferArea != null)
                    {
                        AllIDevicePartnerAddrsses.Add(newTransferArea);
                    }
                }
            }
        }

        public void SetAllIDeviceParnerAdresses()
        {
            foreach (TransferAreaAndAttributes item in AllIDevicePartnerAddrsses)
            {
                    item.RestorePartnerStartAddress();
            }
        }


        public void CopyFromTemplate(ManagePlc aTemplatePlc)
        {
            CentralFSourceAddress_attribue.Value = aTemplatePlc.CentralFSourceAddress_attribue.Value;
            LowerBoundForFDestinationAddresses_attribues.Value = aTemplatePlc.LowerBoundForFDestinationAddresses_attribues.Value;
            UpperBoundForFDestinationAddresses_attribues.Value = aTemplatePlc.UpperBoundForFDestinationAddresses_attribues.Value;

            for (int i = 0; i < FDestinationAddress_attribues.Count; i++)
            {
                FDestinationAddress_attribues[i].Value = aTemplatePlc.FDestinationAddress_attribues[i].Value;
            }

            for (int i = 0; i < AllIDevicePartnerAddrsses.Count; i++)
            {
                AllIDevicePartnerAddrsses[i].PartnerStartAddress.Value = aTemplatePlc.AllIDevicePartnerAddrsses[i].PartnerStartAddress.Value;
            }
        }
        public void AdjustFSettings(ulong FSourceOffset, ulong aFDestOffset)
        {
            ulong oldUower = (ulong)LowerBoundForFDestinationAddresses_attribues.Value;
            ulong oldLpper = (ulong)UpperBoundForFDestinationAddresses_attribues.Value;

            CentralFSourceAddress_attribue.AddToValue(FSourceOffset);
            LowerBoundForFDestinationAddresses_attribues.AddToValue(aFDestOffset);
            UpperBoundForFDestinationAddresses_attribues.AddToValue(aFDestOffset);

            foreach (AttributeAndDeviceItem item in FDestinationAddress_attribues)  //.Where(i => true)
            {
                if (((ulong)item.Value >= oldUower) && ((ulong)item.Value <= oldLpper))
                {
                    item.AddToValue(aFDestOffset);
                }
            }

        }

        public void AdjustPartnerAddresses(ulong aIDeviceOffsett)
        {

            foreach (TransferAreaAndAttributes item in AllIDevicePartnerAddrsses)  //.Where(i => true)
            {
                {
                    item.PartnerStartAddress.AddToValue(aIDeviceOffsett);
                }
            }

        }

        public void CreateNewIoSystem(Subnet aSubnet, string aPrefix)
        {
            string IoSystemName = FirstPnNetworkInterfaces[0].IoControllers[0].IoSystem.Name;
            FirstPnNetworkInterfaces[0].Nodes[0].DisconnectFromSubnet();
            FirstPnNetworkInterfaces[0].Nodes[0].ConnectToSubnet(aSubnet);
            newIoSystem = FirstPnNetworkInterfaces[0].IoControllers[0].CreateIoSystem(aPrefix + IoSystemName);
                       
        }

        public void ConnectToMasterIoSystem(IoSystem aIoSystem)
        {
            FirstPnNetworkInterfaces[0].IoConnectors[0].ConnectToIoSystem(aIoSystem);
        }

        public void DelecteOldSubnet()
        {
            originalSubnet.Delete();
        }

    }
}

