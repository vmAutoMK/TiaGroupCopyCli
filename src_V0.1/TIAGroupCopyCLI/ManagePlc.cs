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
using Siemens.Engineering.SW.TechnologicalObjects;
using Siemens.Engineering.SW.TechnologicalObjects.Motion;
using System.IO;

using TiaOpennessHelper.Utils;
using TIAHelper.Services;
using TIAGroupCopyCLI.Devices;

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

    public class ConnectionProviderAndAttributes
    {

        public AxisHardwareConnectionProvider AxisHardwareConnection;
        public Int32 addressIn;
        public Int32 addressOut;
        public ConnectOption connectOption;
        public bool isConnected;


        public ConnectionProviderAndAttributes(AxisHardwareConnectionProvider connectionProvider)
        {
            AxisHardwareConnection = connectionProvider;
            if (AxisHardwareConnection != null)
            {
                addressIn = AxisHardwareConnection.ActorInterface.InputAddress;
                addressOut = AxisHardwareConnection.ActorInterface.OutputAddress;
                connectOption = AxisHardwareConnection.ActorInterface.ConnectOption;
                isConnected = AxisHardwareConnection.ActorInterface.IsConnected;
            }
        }

        public void Save()
        {
            if (AxisHardwareConnection != null)
            {
                addressIn = AxisHardwareConnection.ActorInterface.InputAddress;
                addressOut = AxisHardwareConnection.ActorInterface.OutputAddress;
                connectOption = AxisHardwareConnection.ActorInterface.ConnectOption;
                isConnected = AxisHardwareConnection.ActorInterface.IsConnected;
            }

        }

        public void Restore()
        {
            if (AxisHardwareConnection != null)
            {

                    AxisHardwareConnection.ActorInterface.Disconnect();
                    AxisHardwareConnection.ActorInterface.Connect(addressIn, addressOut, connectOption);

            }

        }

    }

    class ManagePlc : ManageDevice
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

        public List<ConnectionProviderAndAttributes> AllToConnections = new List<ConnectionProviderAndAttributes>();

        public AttributeAndDeviceItem CentralFSourceAddress_attribue;
        public AttributeAndDeviceItem LowerBoundForFDestinationAddresses_attribues;
        public AttributeAndDeviceItem UpperBoundForFDestinationAddresses_attribues;
        //public IList<AttributeAndDeviceItem> xFDestinationAddress_attribues;
        public Subnet originalSubnet;
        public IoSystem originalIoSystem;
        public IoSystem newIoSystem;

        PlcSoftware plcSoftware;

        public ManagePlc(Device aDevice) : base(aDevice)
        {
            Save();
        }


        public ManagePlc(IList<Device> aDevices) : base(aDevices)
        {
            Save();
        }

        /*
        public ManagePlc(DeviceUserGroup aGroup)
        {
            IList<Device> devices = Service.GetPlcDevicesInGroup(aGroup);
            AllDevices = (List<Device>)devices;
            Save();
        }
        */

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

        public new void Save()
        {

            Device currentDevice = AllDevices[0];
            CentralFSourceAddress_attribue = Service.Get1ValueAndDeviceItemWithAttribute(currentDevice.DeviceItems, "Failsafe_CentralFSourceAddress");
            LowerBoundForFDestinationAddresses_attribues = Service.Get1ValueAndDeviceItemWithAttribute(currentDevice.DeviceItems, "Failsafe_LowerBoundForFDestinationAddresses");
            UpperBoundForFDestinationAddresses_attribues = Service.Get1ValueAndDeviceItemWithAttribute(currentDevice.DeviceItems, "Failsafe_UpperBoundForFDestinationAddresses");
            //xFDestinationAddress_attribues = Service.GetValueAndDeviceItemsWithAttribute(currentDevice.DeviceItems, "Failsafe_FDestinationAddress");

            originalSubnet = FirstPnNetworkInterfaces[0].Nodes[0].ConnectedSubnet;
            originalIoSystem = FirstPnNetworkInterfaces[0].IoConnectors[0].ConnectedToIoSystem;

            //GetAll_I_DeviceParnerAdresses();

            plcSoftware = Service.GetPlcSoftware(currentDevice);
            GetAllToConnections();

        }

        public new  void  Restore()
        {

            CentralFSourceAddress_attribue.Restore();
            LowerBoundForFDestinationAddresses_attribues.Restore();
            UpperBoundForFDestinationAddresses_attribues.Restore();

            ulong lower = (ulong)LowerBoundForFDestinationAddresses_attribues.Value;
            ulong upper = (ulong)UpperBoundForFDestinationAddresses_attribues.Value;

            base.Restore();

            //foreach (AttributeAndDeviceItem item in xFDestinationAddress_attribues)  //.Where(i => true)
            //{
            //    if (((ulong)item.Value >= lower) && ((ulong)item.Value <= upper))
            //    {
             //       item.Restore();
            //    }
           // }

            RestorePnDeviceNumber();
            SetAllIDeviceParnerAdresses();
        }

        public void GetAll_I_DeviceParnerAdresses()
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

        public void GetAllToConnections()
        {
            foreach (TechnologicalInstanceDB currentTechnologicalInstanceDB in plcSoftware.TechnologicalObjectGroup.TechnologicalObjects)
            {
                AxisHardwareConnectionProvider connectionProvider = currentTechnologicalInstanceDB.GetService<AxisHardwareConnectionProvider>();


                if (connectionProvider != null)
                {
                    ConnectionProviderAndAttributes newItem = new ConnectionProviderAndAttributes(connectionProvider);
                    if (newItem != null)
                    {
                        AllToConnections.Add(newItem);
                    }
                }
            }
        }

        public void SetAllToConnections()
        {
            foreach (ConnectionProviderAndAttributes item in AllToConnections)
            {
                item.Restore();
            }
        }

        public void CopyFromTemplate(ManagePlc aTemplatePlc)
        {
            CentralFSourceAddress_attribue.Value = aTemplatePlc.CentralFSourceAddress_attribue.Value;
            LowerBoundForFDestinationAddresses_attribues.Value = aTemplatePlc.LowerBoundForFDestinationAddresses_attribues.Value;
            UpperBoundForFDestinationAddresses_attribues.Value = aTemplatePlc.UpperBoundForFDestinationAddresses_attribues.Value;


            for (int i = 0; i < aTemplatePlc.FDestinationAddress_attribues.Count; i++)
            {
                FDestinationAddress_attribues[i].Value = aTemplatePlc.FDestinationAddress_attribues[i].Value;
            }

            //AllIDevicePartnerAddrsses = aTemplatePlc.AllIDevicePartnerAddrsses.CopyTo;
            for (int i = 0; i < aTemplatePlc.AllIDevicePartnerAddrsses.Count; i++)
            {
                AllIDevicePartnerAddrsses[i].PartnerStartAddress.Value = aTemplatePlc.AllIDevicePartnerAddrsses[i].PartnerStartAddress.Value;
            }

            PnDeviceNumberOfFirstPnNetworkInterfaces = aTemplatePlc.PnDeviceNumberOfFirstPnNetworkInterfaces;
        }
        public new  void  AdjustFSettings(ulong FSourceOffset, ulong aFDestOffset)
        {
            ulong oldLower = (ulong)LowerBoundForFDestinationAddresses_attribues.Value;
            ulong oldUpper = (ulong)UpperBoundForFDestinationAddresses_attribues.Value;

            CentralFSourceAddress_attribue.AddToValue(FSourceOffset);
            LowerBoundForFDestinationAddresses_attribues.AddToValue(aFDestOffset);
            UpperBoundForFDestinationAddresses_attribues.AddToValue(aFDestOffset);

            base.AdjustFDestinationAddress(aFDestOffset, oldLower, oldUpper);
            //foreach (AttributeAndDeviceItem item in xFDestinationAddress_attribues)  //.Where(i => true)
            //{
            //   if (((ulong)item.Value >= oldUower) && ((ulong)item.Value <= oldLpper))
            //    {
            //        item.AddToValue(aFDestOffset);
            //    }
            //}

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
            if (aIoSystem != null )
            {
                FirstPnNetworkInterfaces[0].IoConnectors[0].ConnectToIoSystem(aIoSystem);
            }
        }


        public void RestorePnDeviceNumber()
        {
            if ((FirstPnNetworkInterfaces[0].IoConnectors.Count > 0))
            {
                if ((PnDeviceNumberOfFirstPnNetworkInterfaces[0]?.Value ?? null) != null)
                {
                    FirstPnNetworkInterfaces[0].IoConnectors[0].SetAttribute(PnDeviceNumberOfFirstPnNetworkInterfaces[0].Name, PnDeviceNumberOfFirstPnNetworkInterfaces[0].Value);
                }
            }
        }

        public void AdjustPnDeviceNumberWithOffset(uint aOffset)
        {
            if ((FirstPnNetworkInterfaces[0].IoConnectors.Count > 0))
            {
                if ((PnDeviceNumberOfFirstPnNetworkInterfaces[0]?.Value ?? null) != null)
                {
                    PnDeviceNumberOfFirstPnNetworkInterfaces[0].AddToValue(aOffset);
                }
            }
        }
        public void DelecteOldSubnet()
        {
            originalSubnet.Delete();
        }

    }

}

