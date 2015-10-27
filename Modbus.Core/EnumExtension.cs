using System;
using System.Reflection;

namespace EnumExtension
{
    public class EnumDescription : Attribute
    {

        public string Text;

        public EnumDescription(string text)
        {

            Text = text;

        }

    }

    public static class EnumExt
    {
        public static string GetDescription(this Enum en)
        {

            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {

                object[] attrs = memInfo[0].GetCustomAttributes(typeof(EnumDescription),
                                                                false);

                if (attrs != null && attrs.Length > 0)

                    return ((EnumDescription)attrs[0]).Text;
            }
            return en.ToString();
        }
    }

}