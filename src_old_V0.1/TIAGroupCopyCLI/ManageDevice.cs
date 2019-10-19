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

namespace TIAGroupCopyCLI.Devices
{

    class ManageDevice
    {


        private List<Device> _allDevices;

        private List<NetworkInterface> _firstPnNetworkInterfaces;
        //IList<DeviceItem> tempPnInterfacesDeviceItems = Service.GetDeviceItemsWithAttribute(currentDevice.DeviceItems, "InterfaceType", "Ethernet");
        //NetworkInterface tempPnInterface = tempPnInterfacesDeviceItems[0].GetService<NetworkInterface>();
        //string[] tempIPaddress = ((string)tempPnInterface.Nodes[0].GetAttribute("Address")).Split('.');

        private List<AttributeInfo> _PnDeviceNumberOfFirstPnNetworkInterfaces;

        public List<Device> AllDevices
        {
            get
            {
                if (_allDevices == null)
                {
                    _allDevices = new List<Device>();
                }
                return _allDevices;
            }
            set
            {
                if (_allDevices == null)
                {
                    _allDevices = new List<Device>();
                }
                _allDevices = value;
                Get1PnInterfaces();
            }
        }

        public List<NetworkInterface> FirstPnNetworkInterfaces
        {
            get
            {
                if (_firstPnNetworkInterfaces == null)
                {
                    _firstPnNetworkInterfaces = new List<NetworkInterface>();
                }
                return _firstPnNetworkInterfaces;
            }
            set
            {
                if (_firstPnNetworkInterfaces == null)
                {
                    _firstPnNetworkInterfaces = new List<NetworkInterface>();
                }
                _firstPnNetworkInterfaces = value;
            }
        }

        public List<AttributeInfo> PnDeviceNumberOfFirstPnNetworkInterfaces
        {
            get
            {
                if (_PnDeviceNumberOfFirstPnNetworkInterfaces == null)
                {
                    _PnDeviceNumberOfFirstPnNetworkInterfaces = new List<AttributeInfo>();
                }
                return _PnDeviceNumberOfFirstPnNetworkInterfaces;
            }
            set
            {
                if (_PnDeviceNumberOfFirstPnNetworkInterfaces == null)
                {
                    _PnDeviceNumberOfFirstPnNetworkInterfaces = new List<AttributeInfo>();
                }
                _PnDeviceNumberOfFirstPnNetworkInterfaces = value;
            }
        }

        public List<AttributeInfo> PnDeviceNameOfFirstPnNetworkInterfaces = new List<AttributeInfo>();
        public List<AttributeInfo> PnDeviceNameAutoGenOfFirstPnNetworkInterfaces = new List<AttributeInfo>();

        #region constructors
        public ManageDevice()
        {
            //AllDevices = new List<Device>();
        }
        public ManageDevice(Device aDevice)
        {
            AllDevices.Add(aDevice);
            Get1PnInterfaces();
        }
        public ManageDevice(IList<Device> aDevices)
        {
            AllDevices.AddRange(aDevices);
            Get1PnInterfaces();
        }


        #endregion

        public void Get1PnInterfaces()
        {

            foreach (Device currentDevice in _allDevices)
            {
                IList<DeviceItem> tempPnInterfacesDeviceItems = Service.GetDeviceItemsWithAttribute(currentDevice.DeviceItems, "InterfaceType", "Ethernet");
                if (tempPnInterfacesDeviceItems != null)
                {
                    NetworkInterface tempPnInterface = tempPnInterfacesDeviceItems[0].GetService<NetworkInterface>();
                    FirstPnNetworkInterfaces.Add(tempPnInterface);

                    AttributeInfo tempPnDeviceNumber = null;
                    AttributeInfo tempPnDeviceName = null;
                    AttributeInfo tempPnDeviceNameAutoGen = null;

                    if (tempPnInterface != null)
                    {
                        if (tempPnInterface.IoConnectors.Count > 0)
                        {
                            AttributeValue tempAttributeValue;

                            tempAttributeValue = Service.GetAttribute(tempPnInterface.IoConnectors[0], "PnDeviceNumber");
                            if (tempAttributeValue != null)
                                tempPnDeviceNumber = new AttributeInfo()
                                {
                                    Name = "PnDeviceNumber",
                                    Value = tempAttributeValue.Value
                                };
                            else tempPnDeviceNumber = null;
                        }

                        if (tempPnInterface.Nodes.Count > 0)
                        {
                            AttributeValue tempAttributeValue;

                            tempAttributeValue = Service.GetAttribute(tempPnInterface.Nodes[0], "PnDeviceName");
                            if (tempAttributeValue != null)
                                tempPnDeviceName = new AttributeInfo()
                                {
                                    Name = "PnDeviceName",
                                    Value = tempAttributeValue.Value
                                };
                            else tempPnDeviceName = null;

                            tempAttributeValue = Service.GetAttribute(tempPnInterface.Nodes[0], "PnDeviceNameAutoGeneration");
                            if (tempAttributeValue != null)
                                tempPnDeviceNameAutoGen = new AttributeInfo()
                                {
                                    Name = "PnDeviceNameAutoGeneration",
                                    Value = tempAttributeValue.Value
                                };
                            else tempPnDeviceNameAutoGen = null;

                        }
                    }

                    PnDeviceNumberOfFirstPnNetworkInterfaces.Add(tempPnDeviceNumber);
                    PnDeviceNameOfFirstPnNetworkInterfaces.Add(tempPnDeviceName);
                    PnDeviceNameAutoGenOfFirstPnNetworkInterfaces.Add(tempPnDeviceNameAutoGen);
                }

            }
        }

        public void ChangeNames(string aPrefix)
        {

            if (_allDevices != null)
            {
                //get PLCs in sub folders - recursive
                foreach (Device device in AllDevices)
                {
                    try
                    {
                        device.DeviceItems[1].Name = aPrefix + device.DeviceItems[1].Name;
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void ChangePnDeviceNames(string aPrefix)
        {

            if (_allDevices != null)
            {
                int i = 0;
                foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
                {
                    
                    if ((networkInterface.Nodes.Count > 0) && (PnDeviceNameAutoGenOfFirstPnNetworkInterfaces[i].Value != null))
                    {
                        if (PnDeviceNameAutoGenOfFirstPnNetworkInterfaces[i].Value is bool value)
                            if (value == false)
                            {
                                //Service.SetAttribute(networkInterface.IoConnectors[0], PnDeviceNumberOfFirstPnNetworkInterfaces[i]);
                                networkInterface.Nodes[0].SetAttribute(PnDeviceNameOfFirstPnNetworkInterfaces[i].Name, aPrefix + PnDeviceNameOfFirstPnNetworkInterfaces[i].Value);
                            }
                    }
                    i++;
                }
                
            }
        }

        public void ChangeIpAddresses(ulong aIpOffset)
        {
            foreach (NetworkInterface currentInterface in FirstPnNetworkInterfaces)
            {
                string[] tempIPaddress = ((string)currentInterface.Nodes[0].GetAttribute("Address")).Split('.');

                currentInterface.Nodes[0].SetAttribute("Address", tempIPaddress[0] + "." + tempIPaddress[1] + "." + (Convert.ToInt32(tempIPaddress[2]) + (uint)aIpOffset) + "." + tempIPaddress[3]); //ip of PLC
            }
        }

        public void SwitchIoSystem(Subnet aSubnet, IoSystem aIoSystem, ulong aIpOffset)
        {

            int i = 0;
            foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
            {
                try
                {
                    networkInterface.Nodes[0].DisconnectFromSubnet();
                }
                catch
                {
                }
                if (aSubnet != null)
                {
                    string[] tempIPaddress = ((string)networkInterface.Nodes[0].GetAttribute("Address")).Split('.');
                    networkInterface.Nodes[0].SetAttribute("Address", tempIPaddress[0] + "." + tempIPaddress[1] + "." + (Convert.ToInt32(tempIPaddress[2]) + (uint)aIpOffset) + "." + tempIPaddress[3]); //ip of PLC
                    networkInterface.Nodes[0].ConnectToSubnet(aSubnet);
                    if (aIoSystem != null)
                    {

                        networkInterface.IoConnectors[0].ConnectToIoSystem(aIoSystem);
                        if ( (networkInterface.IoConnectors.Count > 0) && (PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value != null)  )
                        {
                            //PnDeviceNumberOfFirstPnNetworkInterfaces[i].AddToValue(10);
                            //Service.SetAttribute(networkInterface.IoConnectors[0], PnDeviceNumberOfFirstPnNetworkInterfaces[i]);
                            //var x = networkInterface.IoConnectors[0].GetAttribute("PnDeviceNumber");
                            //networkInterface.IoConnectors[0].SetAttribute("PnDeviceNumber", 2);
                            //networkInterface.IoConnectors[0].SetAttribute(PnDeviceNumberOfFirstPnNetworkInterfaces[i].Name, 4);

                            //int tempDeviceNumber = PnDeviceNumberOfFirstPnNetworkInterfaces[i].GetValueAsInt();
                            //networkInterface.IoConnectors[0].SetAttribute(PnDeviceNumberOfFirstPnNetworkInterfaces[i].Name, tempDeviceNumber);
                            networkInterface.IoConnectors[0].SetAttribute(PnDeviceNumberOfFirstPnNetworkInterfaces[i].Name, PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value);
                            
                        }
                    }
                }
                i++;
            }
        }


        public void SwitchIoSystem(Subnet aSubnet, IoSystem aIoSystem)
        {

            int i = 0;
            foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
            {
                try
                {
                    networkInterface.Nodes[0].DisconnectFromSubnet();
                }
                catch
                {
                }
                if (aSubnet != null)
                {
                    networkInterface.Nodes[0].ConnectToSubnet(aSubnet);
                    if (aIoSystem != null)
                    {
                        
                        networkInterface.IoConnectors[0].ConnectToIoSystem(aIoSystem);
                        if ((networkInterface.IoConnectors.Count > 0) && (PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value != null))
                        {
                            //Service.SetAttribute(networkInterface.IoConnectors[0], PnDeviceNumberOfFirstPnNetworkInterfaces[i]);
                            networkInterface.IoConnectors[0].SetAttribute(PnDeviceNumberOfFirstPnNetworkInterfaces[i].Name, PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value);
                        }
                    }
                }
                i++;
            }
        }

        public void DisconnectFromSubnet()
        {
            foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
            {
                networkInterface.Nodes[0].DisconnectFromSubnet();
            }
        }

        public void ConnectToSubnet(Subnet aSubnet)
        {
            if (aSubnet != null)
            {
                foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
                {
                    networkInterface.Nodes[0].ConnectToSubnet(aSubnet);
                }
            }
        }

        public void ConnectToSubnet(Subnet aSubnet, ulong aIpOffset)
        {
            if (aSubnet != null)
            {
                foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
                {
                    string[] tempIPaddress = ((string)networkInterface.Nodes[0].GetAttribute("Address")).Split('.');
                    networkInterface.Nodes[0].SetAttribute("Address", tempIPaddress[0] + "." + tempIPaddress[1] + "." + (Convert.ToInt32(tempIPaddress[2]) + (uint)aIpOffset) + "." + tempIPaddress[3]); //ip of PLC

                    networkInterface.Nodes[0].ConnectToSubnet(aSubnet);
                }
            }
        }

        public void ConnectToIoSystem(IoSystem aIoSystem)
        {
            if (aIoSystem != null)
            {
                int i = 0;
                foreach (NetworkInterface networkInterface in FirstPnNetworkInterfaces)
                {
                    networkInterface.IoConnectors[0].ConnectToIoSystem(aIoSystem);
                    if ((networkInterface.IoConnectors.Count > 0) && (PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value != null))
                    {
                        //Service.SetAttribute(networkInterface.IoConnectors[0], PnDeviceNumberOfFirstPnNetworkInterfaces[i]);
                        networkInterface.IoConnectors[0].SetAttribute(PnDeviceNumberOfFirstPnNetworkInterfaces[i].Name, PnDeviceNumberOfFirstPnNetworkInterfaces[i].Value);
                    }
                }
                i++;
            }
        }


    }
}
