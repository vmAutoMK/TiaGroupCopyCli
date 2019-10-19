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
using TIAGroupCopyCLI.Models;

namespace TIAGroupCopyCLI.Drives
{

    public class TelegramAndAttributes
    {

        public Telegram Telegram;
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

        public TelegramAndAttributes(Telegram aTelegram)
        {
            Telegram = aTelegram;
            if (aTelegram != null)
            {
                FDestinationAddr = Service.GetAttribute(aTelegram, "Failsafe_FDestinationAddress");
                _startAddress = Service.GetAttributes(aTelegram.Addresses, "StartAddress");
            }
        }

        public void SaveFDestAndIoAddresses()
        {
            if (Telegram != null)
            {
                FDestinationAddr = Service.GetAttribute(Telegram, "Failsafe_FDestinationAddress");
                _startAddress = Service.GetAttributes(Telegram.Addresses, "StartAddress");
            }

        }

        public void RestoreFDestAndIoAddresses()
        {
            if (Telegram != null)
            {
                if (FDestinationAddr != null)
                {
                    Service.SetAttribute(Telegram, "Failsafe_FDestinationAddress", FDestinationAddr);
                }
                int i = 0;
                foreach (Address currentAddress in Telegram.Addresses)
                {
                    Service.SetAttribute(currentAddress, "StartAddress", _startAddress[i]);
                    i++;
                }
            }

        }

    }

    class ManageDrive : ManageDevice
    {

        private List<TelegramAndAttributes> _allTelegrams;
        public List<TelegramAndAttributes> allTelegrams
        {
            get
            {
                if (_allTelegrams == null)
                {
                    _allTelegrams = new List<TelegramAndAttributes>();
                }
                return _allTelegrams;
            }
            set
            {
                if (_allTelegrams == null)
                {
                    _allTelegrams = new List<TelegramAndAttributes>();
                }
                _allTelegrams = value;
            }
        }

        public ManageDrive(Device aDevice) : base(aDevice)
        {
            //AllDevices.Add(aDevice);
            Save();
        }
        public ManageDrive(IList<Device> aDevices) : base( aDevices)
        {
            //AllDevices.AddRange(aDevices);
            Save();
        }

        public ManageDrive(DeviceUserGroup aGroup) 
        {
            //AllDevices.AddRange(aDevices);
            IList<Device> devices = Service.GetG120DevicesInGroup(aGroup);
            AllDevices = (List<Device>)devices;
            Save();
        }

        public void Save()
        {
            SaveFDestAndIoAddresses();
        }

        public void Restore()
        {
            RestoreFDestAndIoAddresses();
        }

        public void SaveFDestAndIoAddresses()
        {
            foreach(Device currentDrive in AllDevices)
            {
                DriveObject tempDrive = currentDrive.DeviceItems[1].GetService<DriveObjectContainer>().DriveObjects[0];
                foreach (Telegram currentTelegram in tempDrive.Telegrams)
                {
                    TelegramAndAttributes newTelegram = new TelegramAndAttributes(currentTelegram);
                    if (newTelegram != null)
                    {
                        allTelegrams.Add(newTelegram);
                    }
                    
                }
            }
        }

        public void RestoreFDestAndIoAddresses()
        {
            foreach (TelegramAndAttributes currentTelegram in _allTelegrams)
            {

                currentTelegram.RestoreFDestAndIoAddresses();
            }
        }

        public void AdjustFDestinationAddress(ulong aOffset, ulong aLower, ulong aUpper)
        {
            foreach (TelegramAndAttributes currentTelegram in _allTelegrams)
            {
                if (currentTelegram.FDestinationAddr != null)
                {
                    if ( ( (uint)currentTelegram.FDestinationAddr.Value < aLower ) || ( (uint)currentTelegram.FDestinationAddr.Value > aUpper ) )
                    {
                        currentTelegram.FDestinationAddr.AddToValue(aOffset);
                    }
                    
                }
                
            }
        }

    }
}
