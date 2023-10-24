using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCLRRRRR.Runner
{
    public class RA3
    {

        private string gameVersion = "1.12";

        private string gameLanguage ="en";

        private string gameProgram = "";

        private string gameEnvironment = "";

        private string gameRootFolder = "";

        private string gameModSkudef = "";

        public string GameVersion { get => gameVersion; set => gameVersion = value; }
        public string GameLanguage { get => gameLanguage; set => gameLanguage = value; }
        public string GameProgram { get => gameProgram; set => gameProgram = value; }
        public string GameEnvironment { get => gameEnvironment; set => gameEnvironment = value; }
        public string GameModSkudef { get => gameModSkudef; set => gameModSkudef = value; }
        public string GameRootFolder { get => gameRootFolder; set => gameRootFolder = value; }

        public string GetBestGameSkudefFromList()
        {
            return null;
        }



        

    }
}
