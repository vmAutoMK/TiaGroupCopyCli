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

namespace TIAGroupCopyCLI.Hmis
{

 
    class ManageHmi : ManageDevice
    {

        public ManageHmi(Device aDevice) : base(aDevice)
        {
            Save();
        }
        public ManageHmi(IList<Device> aDevices) : base(aDevices)
        {
            Save();
        }

        public ManageHmi(DeviceUserGroup aGroup)
        {
            IList<Device> devices = Service.GetHmiDevicesInGroup(aGroup);
            AllDevices = (List<Device>)devices;
            Save();
        }

        public void Save()
        {
            ;
        }

        public void Restore()
        {
            ;
        }


    }
}
