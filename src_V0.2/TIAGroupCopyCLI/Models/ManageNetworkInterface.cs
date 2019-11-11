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
        private DeviceItem DeviceItem;
        private NetworkInterface NetworkInterface;
        public List<ManageNetworkPort> DevicePorts { get; set; } = new List<ManageNetworkPort>();
        #endregion

        #region Fields for Saved Information
        private ManageAttributeGroup PnDeviceNames = new ManageAttributeGroup();
        private ManageAttributeGroup PnDeviceNumber = new ManageAttributeGroup();
        private bool isConnected;
        #endregion Fileds

        #region Constructor
        //constructor will discover all the 
        public ManageNetworkInterface(DeviceItem deviceItem, NetworkInterface networkInterface = null)
        {
            UpdateRefernces(deviceItem, networkInterface);
        }


        public void UpdateRefernces(DeviceItem deviceItem, NetworkInterface networkInterface = null)
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
        public void Save()
        {
            if (NetworkInterface != null)
            {
                foreach (IoConnector currentIoConnector in NetworkInterface.IoConnectors)
                {
                    PnDeviceNumber.GetAndAddAttribute(currentIoConnector, "PnDeviceNumber");
                }

                foreach(Node currentNode in NetworkInterface.Nodes)
                {
                    object attributeValue = SimpleAttribute.GetAttribute_Wrapper(currentNode, "PnDeviceNameAutoGeneration");
                    if (attributeValue is bool value)
                        if (value == false)
                        {
                            PnDeviceNames.GetAndAddAttribute(currentNode, "PnDeviceName");
                        }
                }
                foreach (ManageNetworkPort currentItem in DevicePorts)
                {
                    currentItem.Save();
                }
            }
        }

        public void Restore()
        {
            //PnDeviceNumber.Restore();
            foreach (ManageNetworkPort currentItem in DevicePorts)
            {
                currentItem.Restore();
            }
        }

        public void RestorePnDeviceNamesWithPrefix(string prefix)
        {
            PnDeviceNames.RestoreWithPrefix(prefix);
        }

        public void RestorePnDeviceNumberWithOffset(ulong offset)
        {
            PnDeviceNumber.RestoreWithOffset(offset);
        }

        public void ChangeIpAddresses(ulong ipAddressOffset, int nodeNumber = 0)
        {
            if (NetworkInterface?.Nodes?[nodeNumber] != null)
            {
                string[] tempIPaddress = null;
                try
                {
                    tempIPaddress = ((string)NetworkInterface.Nodes[nodeNumber].GetAttribute("Address")).Split('.');
                }
                catch
                {

                }
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

        #region Networking 




        public void DisconnectFromSubnet(int nodeNumber = 0)
        {
            if (NetworkInterface?.Nodes?[nodeNumber] != null)
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
            if ((NetworkInterface?.Nodes?[nodeNumber] != null) && (aSubnet != null) )
            {
                NetworkInterface.Nodes[nodeNumber].ConnectToSubnet(aSubnet);
            }
        }

 
        public void ConnectToIoSystem(IoSystem aIoSystem, int ioConnectorNumber = 0)
        {
            if ((NetworkInterface?.IoConnectors?[ioConnectorNumber] != null) && (aIoSystem != null))
            {
                NetworkInterface.IoConnectors[ioConnectorNumber].ConnectToIoSystem(aIoSystem);
            }
        }

        #endregion

        #endregion Methods


        #region Static Methods
        public static List<ManageNetworkInterface> xGetAll_ManageNetworkInterfaceObjects(Device device)
        {
            List<ManageNetworkInterface> returnManageNetworkInterfaceObjects = new List<ManageNetworkInterface>();

            if (device != null)
            {
                foreach (DeviceItem currentDeviceItem in device.DeviceItems)
                {
                    foreach (DeviceItem currentSubDeviceItems in currentDeviceItem.DeviceItems)
                    {
                        try
                        {
                            if (currentSubDeviceItems.GetAttribute("InterfaceType").ToString() == "Ethernet")
                            {
                                ManageNetworkInterface addManageNetworkInterfaceObject = new ManageNetworkInterface(currentSubDeviceItems);

                                returnManageNetworkInterfaceObjects.Add(addManageNetworkInterfaceObject);
                            }

                        }
                        catch (EngineeringNotSupportedException)
                        {
                            //not the Device item we are looking for ---> move on
                        }
                        catch (Exception ex)
                        {
                            Program.FaultMessage("Could not get Attribute", ex, "Service.GetAllPnInterfaces");
                        }

                    }
                }

            }

            return returnManageNetworkInterfaceObjects;
        }

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
