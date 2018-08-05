using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking.Match;
using System;


namespace Assets.Scripts
{
    public static class AvailableMatchesList
    {
        public static event Action<List<LanConnectionInfo>> OnAvailableMatchesChanged = delegate { };

        private static List<LanConnectionInfo> matches = new List<LanConnectionInfo>();
        public static void HandleNewMatchList(List<LanConnectionInfo> matchList)
        {
            matches = matchList;
            OnAvailableMatchesChanged(matches);
        }

    }
}