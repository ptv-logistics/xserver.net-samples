using Ptv.XServer.Controls.Map.Localization;

namespace CustomLocalizer
{
    public class CustomMapLocalizer : MapLocalizer
    {
        public override string GetLocalizedString(MapStringId id)
        {
            var str = CustomStrings.ResourceManager.GetString(id.ToString());
            return string.IsNullOrEmpty(str) ? "_" + base.GetLocalizedString(id) : str;
        }
    }
}
