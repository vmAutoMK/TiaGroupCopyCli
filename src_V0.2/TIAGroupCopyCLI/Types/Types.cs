using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAGroupCopyCLI.Types
{
    public struct PnDeviceNumber
    {
        public object Value;
        public string Name;
    }

    public class AttributeValue
    {
        public object Value;
        public string Name;

        public AttributeValue()
        {
        }
        public AttributeValue(object aObject)
        {
            Value = aObject;
        }
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

        public int GetValueAsInt()
        {
            return (int)Value;
        }

    }

}
