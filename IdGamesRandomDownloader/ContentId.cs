namespace IdGamesRandomDownloader
{
    internal static class ContentId
    {
        private static readonly int[] _doom = [21, 22, 28, 29, 34, 38, 99, 104, 130, 167];
        private static readonly int[] _doom2 = [8, 18, 20, 30, 41, 57, 70, 72, 81, 82];
        private static readonly int[] _doomPorts = [15, 32, 33, 65, 86, 90, 116, 162, 196, 326];
        private static readonly int[] _doom2Ports = [3, 7, 12, 13, 14, 37, 46, 64, 66, 106];

        public static int[] Ids 
        { 
            get 
            {
                List<int> array = [.. _doom, .. _doom2, .. _doomPorts, .. _doom2Ports];
                return [.. array];
            } 
        }
    }
}