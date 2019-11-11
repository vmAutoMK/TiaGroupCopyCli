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
using Siemens.Engineering.Library.MasterCopies;

namespace TIAGroupCopyCLI.Models
{
    class ManageEthernetInterface : ParentInstance, TiaInstance
    {
        #region Fields
        public ParentInstance Parent { get; }

        private NetworkInterface _TiaNetworkInterface;
        public IEngineeringObject TiaEngineeringObject
        {
            get
            {
                return _TiaNetworkInterface;
            }
        }

        public List<int> Location { get; }

        


        private SaveAttributeGroup _allStartAddresses;

        #endregion Fields

        #region Constructor
        public ManageEthernetInterface(ManageEthernetInterfaceGroup parent, NetworkInterface tiaNetworkInterface, List<int> location)
        {
            Parent = parent;
            _TiaNetworkInterface = tiaNetworkInterface;
            Location = location;

            _allStartAddresses = new SaveAttributeGroup(this);

        }
        #endregion
    }


    class ManageEthernetInterfaceGroup : ParentInstance
    {
        public ParentInstance Parent { get; }
    }
}
