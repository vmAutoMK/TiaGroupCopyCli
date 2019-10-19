using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


using TIAHelper.Services;
using TIAGroupCopyCLI.Models;

namespace TIAGroupCopyCLI.Models
{
    enum AttributeType
    {
        Undefined,
        Device,
        DeviceItem,
        Address
    }
    class AttributeLocation
    {
        #region Fileds
        public int EthernetInterfacecIdx;
        public List<int> DeviceItemIdx;
        public int TelegramIdx;
        public int AddressIdx;
        #endregion Fileds

        #region constructor
        public AttributeLocation()
        {
            EthernetInterfacecIdx = -1;
            DeviceItemIdx = new List<int>();
            TelegramIdx = -1;
            AddressIdx = -1;
        }
        public AttributeLocation(int ethernetInterfacecIdx,  List<int> deviceItemIdx, int telegramIdx, int addressIdx)
        {
            EthernetInterfacecIdx = ethernetInterfacecIdx;
            DeviceItemIdx = deviceItemIdx;
            TelegramIdx = telegramIdx;
            AddressIdx = addressIdx;
        }
        #endregion constructor


    }
    class SavedAttribute
    {
        #region private fields 
        private SaveAttributeGroup _Parent;
        #endregion

        #region Fileds
        public string Name;
        public object Value;
        public AttributeType Type;
        public AttributeLocation Location;
        #endregion

        #region constructor
        public SavedAttribute()
        {
            Location = new AttributeLocation();
            Type = AttributeType.Undefined;
        }
        public SavedAttribute(SaveAttributeGroup parent) : this()
        {
            _Parent = parent;
        }

        public SavedAttribute(SaveAttributeGroup parent, string name, object value, AttributeType type = AttributeType.Undefined, AttributeLocation location = null) : this(parent)
        {
            Name = name;
            Value = value;
            Type = type;
            if (location != null)
            {
                Location = location;
            }
        }

        #endregion constructor

        #region methods

        #region Access Value
        public object AddToValue(uint addToValue)
        {
            if (Value is ulong)
            {
                Value = (ulong)Value + (ulong)addToValue;
            }
            else if (Value is uint)
            {
                Value = (uint)Value + (uint)addToValue;
            }
            if (Value is int)
            {
                Value = (int)Value + (int)addToValue;
            }

            return Value;
        }

        public object AddToValue(ulong addToValue)
        {
            if (Value is ulong)
            {
                Value = (ulong)Value + (ulong)addToValue;
            }
            else if (Value is uint)
            {
                Value = (uint)Value + (uint)addToValue;
            }
            if (Value is int)
            {
                Value = (int)Value + (int)addToValue;
            }

            return Value;
        }

        public object AddToValue(int addToValue)
        {
            if (Value is ulong)
            {
                Value = (ulong)Value + (ulong)addToValue;
            }
            else if (Value is uint)
            {
                Value = (uint)Value + (uint)addToValue;
            }
            if (Value is int)
            {
                Value = (int)Value + (int)addToValue;
            }

            return Value;
        }

        public object AddToValue(object addToValue)
        {
            if (Value is ulong)
            {
                Value = (ulong)Value + (ulong)addToValue;
            }
            else if (Value is uint)
            {
                Value = (uint)Value + (uint)addToValue;
            }
            if (Value is int)
            {
                Value = (int)Value + (int)addToValue;
            }

            return Value;
        }

        public object AddToValueIfNameEquals(string name, object addToValue)
        {
            if (name == this.Name)
            {
                AddToValue(addToValue);
                return true;
            }
            return false;
        }

        public int ValueAsInt()
        {
            return (int)Value;
        }

        #endregion

        #region RestoreAttribute

        public bool RestoreAttribute(Device device)
        {
            if (Location.DeviceItemIdx.Count() > 0)
            {
                return RestoreAttribute(device.DeviceItems[Location.DeviceItemIdx[0]], 1);
            }
            else
            {
                return SetThisAttributeWrapper(device, device.Name);
            }
        }

        bool RestoreAttribute(DeviceItem deviceItem, int level = 0)
        {
            if (Location.DeviceItemIdx.Count() > level)
            {
                //recursive call
                return RestoreAttribute(deviceItem.DeviceItems[Location.DeviceItemIdx[level]], level+1);
            }
            else if (Location.AddressIdx != -1)
            {
                return SetThisAttributeWrapper(deviceItem.Addresses[Location.AddressIdx], deviceItem.Name);
            }
            else
            {
                return SetThisAttributeWrapper(deviceItem, deviceItem.Name);
            }
        }

        bool SetThisAttributeWrapper(IEngineeringObject engineeringObject, string engineeringObjectName)
        {
            try
            {
                engineeringObject.SetAttribute(Name, Value);
                return true;
            }
            catch (Exception ex)
            {
                Service.Exception("Could not set Attribute \"" + Name + "\" in engineeringObject \"" + (engineeringObjectName ?? "{null}") + " (Type: " + Type + ") " + "\" to value \"" + (Value ?? " {not_set}") + "\".", ex);
            }
            return false;
        }

        #endregion

        #region public static methods
        public static bool SetAttributeWrapper(IEngineeringObject engineeringObject, string engineeringObjectName, string name, object value)
        {
            try
            {
                engineeringObject.SetAttribute(name, value);
                return true;
            }
            catch (Exception ex)
            {
                Service.Exception("Could not set Attribute \"" + name + "\" in engineeringObject \"" + (engineeringObjectName ?? "{null}")  + "\" to value \"" + (value ?? " {not_set}") + "\".", ex);
            }
            return false;
        }

        public static object GetAttributeWrapper(IEngineeringObject engineeringObject, string engineeringObjectName, string name)
        {
            object value;
            try
            {

                value = engineeringObject.GetAttribute(name);
                return value;
            }
            catch (EngineeringNotSupportedException ex) //Attribute Name not found  = OK, move on
            {
                
            }
            catch (Exception ex)
            {
                Service.Exception("Could not get Attribute \"" + name + "\" in engineeringObject \"" + (engineeringObjectName ?? "{null}")  + "\".", ex);
            }
            return null;
        }
        #endregion

        #endregion
    }

    //=========================================================================================================
    class SaveAttributeGroup
    {
        #region private field
        IManageDevice _Parent;
        #endregion private Fields

        #region Fields
        public List<SavedAttribute> SavedAttributes;
        #endregion Fields

        #region indexer
        public SavedAttribute this[int index]
        {
            get
            {
                if ((index < SavedAttributes.Count) && (index >= 0))
                {
                    return SavedAttributes[index];
                }
                else
                {
                    int reverseIndex = SavedAttributes.Count + index;
                    if ((reverseIndex < SavedAttributes.Count) && (reverseIndex >= 0))
                    {
                        return SavedAttributes[reverseIndex];
                    }
                }
                return null;
            }
            private set
            {
                if ( (index < SavedAttributes.Count ) && (index >= 0) )
                {
                    SavedAttributes[index] = value;
                }
                else
                {
                    SavedAttributes.Add(value);
                }
            }
        }
        #endregion

        #region Constructor
        SaveAttributeGroup()
        {
            SavedAttributes = new List<SavedAttribute>();
        }

        public SaveAttributeGroup(IManageDevice parent) : this()
        {
            _Parent = parent;
        }
        #endregion Constructor

        #region methods

        #region Save
        public bool SaveDeviceAttribute(Device device, string attributeName)
        {
            object value = SavedAttribute.GetAttributeWrapper(device, device?.Name, attributeName);
            if (value != null)
            {
                SavedAttributes.Add
                    (
                        new SavedAttribute(this, attributeName, value)
                        {
                            Name = attributeName,
                            Value = value,
                            Type = AttributeType.Device
                        }
                    );
            }
            return false;
        }
        public bool FindAndSaveDeviceItemAtribute(Device device, string attributeName)
        {
            List<int>  deviceItemIdx = new List<int>{ 0 };
            int level = 0;
            foreach (DeviceItem currentDeviceItem in device.DeviceItems)
            {
                bool found = FindAndSaveDeviceItemAttribute(currentDeviceItem, attributeName, level+1, deviceItemIdx);
                if (found)
                    return true;

                deviceItemIdx[level]++;
            }

            return false;
        }

        bool FindAndSaveDeviceItemAttribute(DeviceItem deviceItem, string attributeName, int level = 0, List<int> deviceItemIdx = null)
        {
            if (deviceItemIdx == null)
            {
                deviceItemIdx = new List<int>();
            }

            object value = SavedAttribute.GetAttributeWrapper(deviceItem, deviceItem?.Name, attributeName);
            if (value != null)
            {
                SavedAttributes.Add
                    (
                        new SavedAttribute(this)
                        {
                            Name = attributeName,
                            Value = value,
                            Type = AttributeType.DeviceItem,
                            Location = new AttributeLocation()
                            {
                                DeviceItemIdx = deviceItemIdx
                            }
                        }
                    );
                return true;
            }

            List<int> newdeviceItemIdx = new List<int>(deviceItemIdx);
            newdeviceItemIdx.Add(0);
            foreach (DeviceItem currentDeviceItem in deviceItem.DeviceItems )
            {
                //call recursive
                bool found = FindAndSaveDeviceItemAttribute(currentDeviceItem, attributeName, level+1, newdeviceItemIdx);
                if (found)
                    return true;

                newdeviceItemIdx[level]++;
            }

            return false;
        }

        public int FindAndSaveAllDeviceItemAtributes(Device device, string attributeName)
        {
            int Counter = 0;
            List<int> deviceItemIdx = new List<int> { 0 };
            int level = 0;
            foreach (DeviceItem currentDeviceItem in device.DeviceItems)
            {
                Counter = Counter + FindAndSaveAllDeviceItemAtributes(currentDeviceItem, attributeName, level + 1, deviceItemIdx);
                deviceItemIdx[level]++;
            }

            return Counter;
        }

        int FindAndSaveAllDeviceItemAtributes(DeviceItem deviceItem, string attributeName, int level = 0, List<int> deviceItemIdx = null)
        {
            int Counter = 0;

            if (deviceItemIdx == null)
            {
                deviceItemIdx = new List<int>();
            }

            object value = SavedAttribute.GetAttributeWrapper(deviceItem, deviceItem?.Name, attributeName);
            if (value != null)
            {
                SavedAttributes.Add
                    (
                        new SavedAttribute(this)
                        {
                            Name = attributeName,
                            Value = value,
                            Type = AttributeType.DeviceItem,
                            Location = new AttributeLocation()
                            {
                                DeviceItemIdx = new List<int>(deviceItemIdx)
                            }
                        }
                    ); ;
                Counter++;
            }

            List<int> newDeviceItemIdx = new List<int>(deviceItemIdx);
            newDeviceItemIdx.Add(0);
            foreach (DeviceItem currentDeviceItem in deviceItem.DeviceItems)
            {
                //call recursive
                Counter = Counter + FindAndSaveAllDeviceItemAtributes(currentDeviceItem, attributeName, level + 1, newDeviceItemIdx);
                newDeviceItemIdx[level]++;
            }

            return Counter;
        }

        public int FindAndSaveAllAddressAtributes(Device device, string attributeName)
        {
            int Counter = 0;
            List<int> deviceItemIdx = new List<int> { 0 };
            int level = 0;
            foreach (DeviceItem currentDeviceItem in device.DeviceItems)
            {
                Counter = Counter + FindAndSaveAllAddressAtributes(currentDeviceItem, attributeName, level + 1, deviceItemIdx);
                deviceItemIdx[level]++;
            }

            return Counter;
        }

        int FindAndSaveAllAddressAtributes(DeviceItem deviceItem, string attributeName, int level = 0, List<int> deviceItemIdx = null)
        {
            int Counter = 0;

            if (deviceItemIdx == null)
            {
                deviceItemIdx = new List<int>();
            }

            int addressIdx = 0;
            foreach (Address currentAddress in deviceItem.Addresses)
            {
                object value = SavedAttribute.GetAttributeWrapper(currentAddress, deviceItem?.Name + "-Address", attributeName);
                if (value != null)
                {
                    SavedAttributes.Add
                        (
                            new SavedAttribute(this)
                            {
                                Name = attributeName,
                                Value = value,
                                Type = AttributeType.Address,
                                Location = new AttributeLocation()
                                {
                                    DeviceItemIdx = new List<int>(deviceItemIdx),
                                    AddressIdx = addressIdx
                                }
                            }
                        ); ;
                    Counter++;
                }
                addressIdx++;
            }

            List<int> newDeviceItemIdx = new List<int>(deviceItemIdx);
            newDeviceItemIdx.Add(0);
            foreach (DeviceItem currentDeviceItem in deviceItem.DeviceItems)
            {
                //call recursive
                Counter = Counter + FindAndSaveAllAddressAtributes(currentDeviceItem, attributeName, level + 1, newDeviceItemIdx);
                newDeviceItemIdx[level]++;
            }

            return Counter;
        }

        #endregion

        #region Save


        #endregion

        #endregion methods


    }
}
