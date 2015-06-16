using Ptv.XServer.Controls.Map.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomLocalizer
{
    public class CustomMapLocalizer : MapLocalizer
    {
        public override string GetLocalizedString(MapStringId id)
        {
            string str = CustomLocalizer.CustomStrings.ResourceManager.GetString(id.ToString());

            if (string.IsNullOrEmpty(str))
                return "_" + base.GetLocalizedString(id);
            else
                return str;
        }
    }
}
