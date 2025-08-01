using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

namespace Homo.Bet.Api
{
    public class ConvertHelper
    {
        public static List<EnumList> EnumToList(Type aType)
        {
            List<string> names = Enum.GetNames(aType).ToList<string>();
            Array values = Enum.GetValues(aType);
            List<EnumList> list = new List<EnumList>();
            for (int i = 0; i < names.Count(); i++)
            {
                var source = values.GetValue(i);
                FieldInfo fi = source.GetType().GetField(source.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                string label = source.ToString();
                if (attributes.Length > 0)
                {
                    label = attributes[0].Description;
                }
                list.Add(new EnumList()
                {
                    Key = names[i],
                    Label = label,
                    Value = values.GetValue(i)
                });
            }
            return list;
        }

        public static long GetUserIdFromEmail(string username)
        {
            if (username == "miterfrants@gmail.com")
            {
                return 4;
            }
            else if (username == "vickychou99@gmail.com" || username == "Vickychou99")
            {
                return 5;
            }
            return 0;
        }

        public class EnumList
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public object Value { get; set; }
        }
    }
}