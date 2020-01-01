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


namespace TIAGroupCopyCLI.Models
{

    public class ManageNetworkInterface
    {
        #region References to openness object and managed objcts
        public DeviceItem DeviceItem;
        public NetworkInterface NetworkInterface;
        public List<ManageNetworkPort> DevicePorts { get; set; } = new List<ManageNetworkPort>();
        #endregion

        #region Fields for Saved Information
        private SingleAttribute PnDeviceNames;
        private SingleAttribute PnDeviceNumbers;
        public List<TransferAreaAndAttributes> IDevicePartnerIoAddrsses = new List<TransferAreaAndAttributes>();
        bool isConnectedToIoSystem;
        bool isConnectedtoNetwork;
        #endregion Fileds

        #region Constructor
        //constructor will discover all the 
        public ManageNetworkInterface(DeviceItem deviceItem, NetworkInterface networkInterface = null)
        {
            DeviceItem = deviceItem;
            if (networkInterface != null)
            {
                NetworkInterface = networkInterface;
            }
            else
            {
                NetworkInterface = deviceItem.GetService<NetworkInterface>();
            }
            DevicePorts = ManageNetworkPort.GetAll_ManageNetworkPortObjects(NetworkInterface);
        }                
        #endregion Constuctor

        #region Methods

        public void SaveConfig()
        {
            if (NetworkInterface?.IoConnectors?.Count() > 0)
            {
                PnDeviceNumbers = SingleAttribute.GetSimpleAttributeObject(NetworkInterface.IoConnectors[0], "PnDeviceNumber");
                if (NetworkInterface.IoConnectors[0].ConnectedToIoSystem != null) isConnectedToIoSystem = true;
            }

            if (NetworkInterface?.Nodes.Count() > 0)
            {
                object attributeValue = SingleAttribute.GetAttribute_Wrapper(NetworkInterface.Nodes[0], "PnDeviceNameAutoGeneration");
                if (attributeValue is bool value)
                    if (value == false)
                    {
                        PnDeviceNames = SingleAttribute.GetSimpleAttributeObject(NetworkInterface.Nodes[0], "PnDeviceName");
                    }

                if (NetworkInterface.Nodes[0].ConnectedSubnet != null) isConnectedtoNetwork = true;
            }

            foreach (ManageNetworkPort currentPort in DevicePorts)
            {
                currentPort.SaveConfig();
            }

        }

        public void CopyFromTemplate(ManageNetworkInterface aTemplateManageNetworkInterface)
        {
            if (aTemplateManageNetworkInterface.PnDeviceNumbers != null)
            {
                if (PnDeviceNumbers == null)
                {
                    if (NetworkInterface?.IoConnectors?.Count() > 0)
                    {
                        PnDeviceNumbers = new SingleAttribute(NetworkInterface.IoConnectors[0], "PnDeviceNumber", aTemplateManageNetworkInterface.PnDeviceNumbers.Value);
                    }
                    
                }
                else
                {
                    PnDeviceNumbers.Value = aTemplateManageNetworkInterface.PnDeviceNumbers.Value;
                }
            }

            if (IDevicePartnerIoAddrsses.Count < aTemplateManageNetworkInterface.IDevicePartnerIoAddrsses.Count)
            {
                IDevicePartnerIoAddrsses.Clear();
                Save_iDeviceParnerIoAdresses();
            }
            for (int i = 0; i < aTemplateManageNetworkInterface.IDevicePartnerIoAddrsses.Count; i++)
            {
                IDevicePartnerIoAddrsses[i].PartnerStartAddress.Value = aTemplateManageNetworkInterface.IDevicePartnerIoAddrsses[i].PartnerStartAddress.Value;
            }
        }
        public void Restore()
        {
            //PnDeviceNumber.Restore();
            foreach (ManageNetworkPort currentItem in DevicePorts)
            {
                currentItem.RestoreConfig();
            }
        }

        public void RestoreConfig_WithAdjustments(string prefix, ulong pnDeviceNumberOffset)
        {
            foreach (ManageNetworkPort currentItem in DevicePorts)
            {
                currentItem.RestoreConfig();
            }
            
            PnDeviceNumbers?.RestoreWithOffset((int)pnDeviceNumberOffset);
            PnDeviceNames?.RestoreWithPrefix(prefix);


        }

        public void RestorePnDeviceNamesWithPrefix(string prefix)
        {
            PnDeviceNames?.RestoreWithPrefix(prefix);
        }

        public void RestorePnDeviceNumberWithOffset(ulong offset)
        {
            PnDeviceNumbers?.RestoreWithOffset(offset);
        }

        public void ChangeIpAddress(ulong ipAddressOffset, int nodeNumber = 0)
        {
            if (NetworkInterface?.Nodes?.Count() > nodeNumber)
            {
                string[] tempIPaddress = null;
                try
                {
                    tempIPaddress = ((string)NetworkInterface.Nodes[nodeNumber].GetAttribute("Address")).Split('.');
                }
                catch
                {  }

                if (tempIPaddress != null)
                {
                    try
                    {
                        NetworkInterface.Nodes[nodeNumber].SetAttribute("Address", tempIPaddress[0] + "." + tempIPaddress[1] + "." + (Convert.ToInt32(tempIPaddress[2]) + (uint)ipAddressOffset) + "." + tempIPaddress[3]);
                    }
                    catch (Exception ex)
                    {
                        Program.FaultMessage("Could not set IP address of " + DeviceItem?.Name ?? "{null}" + "." + NetworkInterface?.Nodes?[nodeNumber]?.Name ?? "{null}", ex, "ManageNetworkInterface.ChangeIpAddresses");
                    }
                }
            }
        }

        public void ChangePnDeviceName(string prefix, int nodeNumber = 0)
        {

            if (NetworkInterface?.Nodes?.Count() >  nodeNumber)
            {
                string tempPnDeviceName = null;

                object attributeValue = SingleAttribute.GetAttribute_Wrapper(NetworkInterface.Nodes[nodeNumber], "PnDeviceNameAutoGeneration");
                if (attributeValue is bool value)
                    if (value == false)
                    {
                        tempPnDeviceName = ((string)SingleAttribute.GetAttribute_Wrapper(NetworkInterface.Nodes[nodeNumber], "PnDeviceName"));
                    }


                if (tempPnDeviceName != null)
                {
                    SingleAttribute.SetAttribute_Wrapper(NetworkInterface.Nodes[nodeNumber], "PnDeviceName", prefix + tempPnDeviceName);
                }
            }
        }

        #region Networking 

        public Subnet Get_Subnet(int nodeNumber = 0)
        {

            if (NetworkInterface?.Nodes?.Count() >  nodeNumber)
            {
                try
                {
                    return NetworkInterface.Nodes[nodeNumber].ConnectedSubnet;
                }
                catch
                {
                }
            }
            return null;
        }

        public IoSystem Get_ioSystem(int ioConnectorNumber = 0)
        {

            if (NetworkInterface?.Nodes?.Count() >  ioConnectorNumber)
            {
                try
                {
                    return NetworkInterface.IoConnectors[ioConnectorNumber].ConnectedToIoSystem;
                }
                catch
                {
                }
            }
            return null;
        }

        public void Reconnect(Subnet subnet, IoSystem ioSystem,int nodeNumber = 0, int ioConnectorNumber = 0)
        {
            if (isConnectedtoNetwork && subnet != null)
            {

                DisconnectFromSubnet();
                ConnectToSubnet(subnet, nodeNumber);

                if (isConnectedToIoSystem && ioSystem != null)
                {
                    ConnectToIoSystem(ioSystem, ioConnectorNumber);
                }
            }

        }
        public void DisconnectFromSubnet(int nodeNumber = 0)
        {
            if (NetworkInterface?.Nodes?.Count() >  nodeNumber)
            {
                try
                {
                    NetworkInterface.Nodes[nodeNumber].DisconnectFromSubnet();
                }
                catch
                {
                }
            }
        }

        public void ConnectToSubnet(Subnet aSubnet, int nodeNumber = 0)
        {
            if ((NetworkInterface?.Nodes?.Count() >  nodeNumber) && (aSubnet != null) )
            {
                NetworkInterface.Nodes[nodeNumber].ConnectToSubnet(aSubnet);
            }
        }
         
        public void ConnectToIoSystem(IoSystem aIoSystem, int ioConnectorNumber = 0)
        {
            if ((NetworkInterface?.IoConnectors?.Count() >  ioConnectorNumber) && (aIoSystem != null))
            {
                NetworkInterface.IoConnectors[ioConnectorNumber].ConnectToIoSystem(aIoSystem);
            }
        }

        public IoSystem CreateNewIoSystem(Subnet aSubnet, string aPrefix, int ioConnectorNumber = 0, int nodeNumber = 0)
        {
            try
            {
                if ((NetworkInterface?.IoConnectors?.Count() >  ioConnectorNumber) && (NetworkInterface?.Nodes?.Count() >  nodeNumber) && (aSubnet != null))
                {
                    string IoSystemName = NetworkInterface.IoControllers[0].IoSystem.Name;
                    NetworkInterface.Nodes[nodeNumber].DisconnectFromSubnet();
                    NetworkInterface.Nodes[nodeNumber].ConnectToSubnet(aSubnet);
                    IoSystem newIoSystem = NetworkInterface.IoControllers[ioConnectorNumber].CreateIoSystem(aPrefix + IoSystemName);
                    return newIoSystem;
                }

            }
            catch (NullReferenceException)
            { }

            return null;

        }

        #endregion Networking

        #region i-device
        public void Save_iDeviceParnerIoAdresses()
        {

            foreach (TransferArea currentTransferArea in NetworkInterface.TransferAreas)
            {

                if (currentTransferArea.PartnerAddresses.Count > 0)
                {
                    TransferAreaAndAttributes newTransferArea = new TransferAreaAndAttributes(currentTransferArea);
                    if (newTransferArea != null)
                    {
                        IDevicePartnerIoAddrsses.Add(newTransferArea);
                    }
                }
            }
        }

        public void Restore_iDeviceParnerAdressesWithOffest(ulong aIDeviceOffsett)
        {
            foreach (TransferAreaAndAttributes item in IDevicePartnerIoAddrsses)
            {
                item.RestorePartnerStartAddressWitOffset(aIDeviceOffsett);
            }
        }

        #endregion i-device


        #endregion Methods

        #region Static Methods

        public static List<ManageNetworkInterface> GetAll_ManageNetworkInterfaceObjects(Device device)
        {
            List<ManageNetworkInterface> returnManageNetworkInterfaceObjects = new List<ManageNetworkInterface>();

            foreach (DeviceItem currentDeviceItem in device.DeviceItems)
            {
                List<ManageNetworkInterface> newManageNetworkInterfaceObjects = GetAll_ManageNetworkInterfaceObjects(currentDeviceItem);
                if (newManageNetworkInterfaceObjects.Count > 0)
                {
                    returnManageNetworkInterfaceObjects.AddRange(newManageNetworkInterfaceObjects);
                }
            }

            return returnManageNetworkInterfaceObjects;
        }

        private static List<ManageNetworkInterface> GetAll_ManageNetworkInterfaceObjects(DeviceItem deviceItem)
        {
            List<ManageNetworkInterface> returnManageNetworkInterfaceObjects = new List<ManageNetworkInterface>();

            NetworkInterface newNetworkInterface = deviceItem.GetService<NetworkInterface>();
            if (newNetworkInterface != null)
            {
                returnManageNetworkInterfaceObjects.Add(new ManageNetworkInterface(deviceItem, newNetworkInterface));
            }

            foreach (DeviceItem currentDeviceItem in deviceItem.DeviceItems)
            {
                //call recursive
                List<ManageNetworkInterface> newManageNetworkInterfaceObjects = GetAll_ManageNetworkInterfaceObjects(currentDeviceItem);
                if (newManageNetworkInterfaceObjects.Count > 0)
                {
                    returnManageNetworkInterfaceObjects.AddRange(newManageNetworkInterfaceObjects);
                }
            }

            return returnManageNetworkInterfaceObjects;
        }


        #endregion
    }

}
