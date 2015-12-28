using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Core
{
    public class DeclarationOrderComparator : IComparer
    {
        int IComparer.Compare(Object x, Object y)
        {
            PropertyInfo first = x as PropertyInfo;
            PropertyInfo second = y as PropertyInfo;
            if (first.MetadataToken < second.MetadataToken)
                return -1;
            else if (first.MetadataToken > second.MetadataToken)
                return 1;

            return 0;
        }
    }
    class OrderedPropertiesGetter
    {
        public static PropertyInfo[] GetObjectPropertiesInDeclarationOrder(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            Array.Sort(properties, new DeclarationOrderComparator());
            return properties;
        }
    }
}
