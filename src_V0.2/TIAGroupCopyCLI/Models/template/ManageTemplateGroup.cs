using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.MC.Drives;
using Siemens.Engineering.Hmi;

using TIAGroupCopyCLI.MessagingFct;
using TIAGroupCopyCLI.AppExceptions;

namespace TIAGroupCopyCLI.Models.template
{
    class ManageTemplateGroup
    {


        private DeviceUserGroup tiaTemplateGroup;

        #region Constructor

        public static ManageTemplateGroup CreateTemplate(Project project, string templateGroupName, uint templateGroupNumber, string devicePrefix)
        {
            DeviceUserGroup tiaTemplateGroup;
            //string groupNamePrefix;
            //ManageTemplateGroup newManageTemplateGroup;

            tiaTemplateGroup = project.DeviceGroups.Find(templateGroupName);
            if (tiaTemplateGroup == null)
            {
                throw new GroupCopyException("Group not found.");
            }

            ManageGroup templateGroup = new ManageGroup(tiaTemplateGroup);
            if (templateGroup.Devices.Where(d => d.DeviceType == DeviceType.Plc).Count() != 1)
            {
                throw new GroupCopyException("No PLC or more than 1 PLC in group.");
            }

            
            templateGroup.StripGroupNumAndPrefix(devicePrefix);


            templateGroup.SaveConfig();

            return null; // newManageTemplateGroup;
        }

        ManageTemplateGroup(DeviceUserGroup tiaTemplateGroup, string groupNamePrefix, string devicePrefix)
        {

            this.tiaTemplateGroup = tiaTemplateGroup;


        }

        #endregion Constructor


    }
}
