// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using Ptv.XServer.Controls.Map;


namespace Ptv.XServer.Demo.UseCases.MapProfile
{
    public class MapProfileUseCase
    {
        public void ChangeMapProfile(WpfMap wpfMap, string mapProfile)
        {
            if (mapProfile.Equals("default"))
                Reset(wpfMap);
            else
                wpfMap.XMapStyle = mapProfile;
        }

        public void Reset(WpfMap wpfMap)
        {
            wpfMap.XMapStyle = null;
        }
    }
}
