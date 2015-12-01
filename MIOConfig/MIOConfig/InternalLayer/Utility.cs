using System;
using System.Reflection;

namespace MIOConfig.InternalLayer
{
    public class Utility
    {

        public static bool IsIntersect(int Start1, int End1, int Start2, int End2)
        {
            if ((Start2 >= Start1) && (Start2 <= End1))
                return true;
            if ((Start1 >= Start2) && (Start1 <= End2))
                return true;
            if ((End2 >= Start1) && (End2 <= End1))
                return true;
            if ((End1 >= Start2) && (End1 <= End2))
                return true;
            return true;
        }

        public static object CloneObject(object objSource)
        {
            if (objSource == null)
                return null;
            //step : 1 Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);

            //Step2 : Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Step : 3 Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    //Step : 4 check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {
                            property.SetValue(objTarget, CloneObject(objPropertyValue), null);
                        }
                    }
                }
            }
            return objTarget;
        }

        public static void CloneObjectProperties(object objSource, ref object objTarget)
        {
            if(objSource == null)
                return;
            //step : 1 Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            if (objTarget.GetType() != typeSource)
                return;
            //object objTarget = Activator.CreateInstance(typeSource);

            //Step2 : Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Step : 3 Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    //Step : 4 check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }                   
                }
            }            
        }
    }
}
