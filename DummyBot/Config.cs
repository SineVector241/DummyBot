using Newtonsoft.Json;

namespace DummyBot
{
    public class Config
    {
        private const string _configFolder = "Resources";
        private const string _configFile = "config.json";
        public const string _MapPicturesFolder = "Resources/MapPictures";
        private const string _AmmoPicturesFolder = "Resources/AmmunitionPictures";
        private const string _VehiclePicturesFolder = "Resources/VehiclePictures";
        public static BotConfig BotConfiguration;

        static Config()
        {
            try
            {
                if (!Directory.Exists(_configFolder))
                {
                    Directory.CreateDirectory(_configFolder);
                }

                if(!Directory.Exists(_MapPicturesFolder))
                {
                    Directory.CreateDirectory(_MapPicturesFolder);
                }

                if(!Directory.Exists(_AmmoPicturesFolder))
                {
                    Directory.CreateDirectory(_AmmoPicturesFolder);
                }

                if(!Directory.Exists(_VehiclePicturesFolder))
                {
                    Directory.CreateDirectory(_VehiclePicturesFolder);
                }



                if (!File.Exists(_configFolder + "/" + _configFile))
                {
                    BotConfiguration = new BotConfig();
                    string botConfigJson = JsonConvert.SerializeObject(BotConfiguration, Formatting.Indented);
                    File.WriteAllText(_configFolder + "/" + _configFile, botConfigJson);
                }
                else
                {
                    string botConfigJson = File.ReadAllText(_configFolder + "/" + _configFile);
                    BotConfiguration = JsonConvert.DeserializeObject<BotConfig>(botConfigJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in InteractiveServiceManager.cs \nError Info:\n{ex}");
            }
        }

        public struct BotConfig
        {
            public string Token { get; set; }
        }
    }
}
