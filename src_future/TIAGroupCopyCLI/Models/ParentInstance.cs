using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Siemens.Engineering;

namespace TIAGroupCopyCLI.Models
{
    interface ParentInstance
    {
        //
        // Summary:
        //     Gets the parent of the instance.
        ParentInstance Parent { get; }
        //IEngineeringObject TiaEngineeringObject { get; }
    }

    interface TiaInstance
    {
        //
        // Summary:
        //     Gets the parent of the instance.
        IEngineeringObject TiaEngineeringObject { get; }
    }
}
