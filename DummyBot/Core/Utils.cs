using System.Net;
using System.Text.RegularExpressions;
using Discord;
using HtmlAgilityPack;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace DummyBot.Core
{
    public struct UserCooldown
    {
        public ulong UserID { get; set; }
        public int CooldownSeconds { get; set; }
        public DateTime DateTime { get; set; }
    }

    public struct CooldownResponse
    {
        public int Seconds { get; set; }
        public bool CooledDown { get; set; }
    }

    public class Utils
    {
        Database.Database db = new Database.Database();
        private static List<UserCooldown> SearchPlayer = new List<UserCooldown>();
        private static List<UserCooldown> Sync = new List<UserCooldown>();
        private static List<UserCooldown> Link = new List<UserCooldown>();
        private static List<UserCooldown> ApplySteam = new List<UserCooldown>();
        private static List<UserCooldown> ApplySquad = new List<UserCooldown>();

        public string GetRequest(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            using WebResponse webResponse = request.GetResponse();
            using Stream webStream = webResponse.GetResponseStream();

            using StreamReader reader = new StreamReader(webStream);
            string data = reader.ReadToEnd();
            return data;
        }
        public EmbedBuilder GetEmbedWBStats(string playerID, bool isSteam = false)
        {
            var embed = new EmbedBuilder()
                .WithFooter("To get the full stats. Buy the steam version of WarBrokers!")
                .WithColor(Color.Green);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(GetRequest($"https://stats.warbrokers.io/players/i/{playerID}"));
            var playerName = document.DocumentNode.SelectSingleNode("//div[@class='page-header']");
            var playerData = document.DocumentNode.SelectNodes("//div[@class='player-details-number-box-grid']");
            var rx = new Regex(@"Lvl([\s,\S]*)$");

            embed.WithTitle($"Player stat: { rx.Replace(playerName.InnerText.Replace("\n", "").Replace(" ", ""), "") }");
            embed.AddField("Level", playerData[0].ChildNodes[3].InnerText);
            embed.AddField("Kills", playerData[1].ChildNodes[3].InnerText);

            if(isSteam)
                embed.AddField("Deaths", playerData[2].ChildNodes[3].InnerText);

            embed.AddField("XP", playerData[5].ChildNodes[3].InnerText);
            if (isSteam)
            {
                embed.AddField("K/D", playerData[3].ChildNodes[3].InnerText);
                embed.AddField("KPM", playerData[4].ChildNodes[3].InnerText);
                embed.AddField("Weapon Kills", playerData[6].ChildNodes[3].InnerText);
                embed.AddField("Vehicle Kills", playerData[7].ChildNodes[3].InnerText);
                embed.AddField("Damage Dealt", playerData[8].ChildNodes[3].InnerText);
                embed.AddField("Head Shots", playerData[9].ChildNodes[3].InnerText);
            }
            return embed;
        }
        public async Task<EmbedBuilder> GetSavedEmbedWBStats(ulong userID)
        {
            var embed = new EmbedBuilder()
                .WithFooter("To get the full stats. Buy the steam version of WarBrokers!")
                .WithColor(Color.Green);
            var data = await db.GetUserByIdAsync(userID);
            embed.WithTitle($"Player stat: {data.WBName}");
            embed.AddField("Level", data.Level);
            embed.AddField("Kills", data.Kills);

            if (data.IsSteam)
                embed.AddField("Deaths", data.Deaths);

            embed.AddField("XP", data.XP);
            if (data.IsSteam)
            {
                embed.AddField("K/D", data.KD);
                embed.AddField("KPM", data.KPM);
                embed.AddField("Weapon Kills", data.WeaponKills);
                embed.AddField("Vehicle Kills", data.VehicleKills);
                embed.AddField("Damage Dealt", data.DamageDealt);
                embed.AddField("Head Shots", data.HeadShots);
            }
            return embed;
        }
        public Database.User WBStatsData(string playerID, ulong userID)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(GetRequest($"https://stats.warbrokers.io/players/i/{playerID}"));
                var playerName = document.DocumentNode.SelectSingleNode("//div[@class='page-header']");
                var playerData = document.DocumentNode.SelectNodes("//div[@class='player-details-number-box-grid']");
                var rx = new Regex(@"Lvl([\s,\S]*)$");

                var userData = new Database.User()
                {
                    ID = userID,
                    IsSteam = false,
                    WBID = playerID,
                    WBName = rx.Replace(playerName.InnerText.Replace("\n", "").Replace(" ", ""), ""),
                    Level = playerData[0].ChildNodes[3].InnerText,
                    Kills = playerData[1].ChildNodes[3].InnerText,
                    Deaths = playerData[2].ChildNodes[3].InnerText,
                    XP = playerData[5].ChildNodes[3].InnerText,
                    KD = playerData[3].ChildNodes[3].InnerText,
                    KPM = playerData[4].ChildNodes[3].InnerText,
                    WeaponKills = playerData[6].ChildNodes[3].InnerText,
                    VehicleKills = playerData[7].ChildNodes[3].InnerText,
                    DamageDealt = playerData[8].ChildNodes[3].InnerText,
                    HeadShots = playerData[9].ChildNodes[3].InnerText
                };
                return userData;
            }
            catch
            {
                return new Database.User();
            }
        }

        public Database.Squad SquadStatsData(string squadname)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(GetRequest($"https://stats.warbrokers.io/squads/{squadname}"));
            var TopTenData = document.DocumentNode.SelectNodes("//div[@class='squad-top-ten-wrapper']");
            var SquadMembers = document.DocumentNode.SelectNodes("//div[@class='squad-player-grid']");
            JObject json = new JObject();

            string DeathMatch = "";
            string BattleRoyale = "";
            string MissileLaunch = "";
            string PackageDrop = "";
            string VehicleEscort = "";
            string ZombieBR = "";
            string CapturePoint = "";

            foreach (var data in TopTenData)
            {
                if (data.ChildNodes[1].InnerText == "Death Match")
                    DeathMatch = data.ChildNodes[3].InnerText;
                if (data.ChildNodes[1].InnerText == "Battle Royale")
                    BattleRoyale = data.ChildNodes[3].InnerText;
                if (data.ChildNodes[1].InnerText == "Missile Launch")
                    MissileLaunch = data.ChildNodes[3].InnerText;
                if (data.ChildNodes[1].InnerText == "Package Drop")
                    PackageDrop = data.ChildNodes[3].InnerText;
                if (data.ChildNodes[1].InnerText == "Vehicle Escort")
                    VehicleEscort = data.ChildNodes[3].InnerText;
                if (data.ChildNodes[1].InnerText == "Zombie BR")
                    ZombieBR = data.ChildNodes[3].InnerText;
                if (data.ChildNodes[1].InnerText == "Capture Point")
                    CapturePoint = data.ChildNodes[3].InnerText;
            }

            foreach(var SquadMember in SquadMembers)
            {
                json[SquadMember.ChildNodes[1].InnerText] = new JObject();
                json[SquadMember.ChildNodes[1].InnerText]["Level"] = SquadMember.ChildNodes[5].ChildNodes[1].ChildNodes[3].InnerText;
                json[SquadMember.ChildNodes[1].InnerText]["XP"] = SquadMember.ChildNodes[5].ChildNodes[11].ChildNodes[3].InnerText;
                json[SquadMember.ChildNodes[1].InnerText]["ClassicWins"] = SquadMember.ChildNodes[5].ChildNodes[15].ChildNodes[3].InnerText;
            }
            var Squad = new Database.Squad()
            {
                Name = squadname,
                DeathMatch = DeathMatch,
                BattleRoyale = BattleRoyale,
                MissileLaunch = MissileLaunch,
                PackageDrop = PackageDrop,
                VehicleEscort = VehicleEscort,
                ZombieBR = ZombieBR,
                CapturePoint = CapturePoint,
                Members = json,
                LastUpdate = DateTime.Now
            };
            return Squad;
        }
        public bool CheckSquadName(string name)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(GetRequest($"https://stats.warbrokers.io"));
            var squads = document.DocumentNode.SelectNodes("//a[@class='squadLink']");
            string list = "";
            foreach(var squadname in squads)
            {
                list += $"|{squadname.InnerText}";
            }
            if(list.Contains(name))
                return true;
            return false;
        }
        public CooldownResponse Cooldown(SocketUser user, string type, int Seconds = 0)
        {
            var list = new List<UserCooldown>();
            switch (type)
            {
                case "SearchPlayer":
                    list = SearchPlayer;
                    break;
                case "Sync":
                    list = Sync;
                    break;
                case "Link":
                    list = Link;
                    break;
                case "ApplySteam":
                    list = ApplySteam;
                    break;
                case "ApplySquad":
                    list = ApplySquad;
                    break;
                default:
                    throw new Exception($"Invalid cooldown type: {type}");
            }
            UserCooldown tempUser = list.FirstOrDefault(x => x.UserID == user.Id);
            if (tempUser.UserID != 0)
            {
                if ((DateTime.Now - tempUser.DateTime).TotalSeconds >= tempUser.CooldownSeconds)
                {
                    var value = list.Find(x => x.UserID == tempUser.UserID);
                    value.DateTime = DateTime.Now;
                    list.Remove(tempUser);
                    list.Add(value);
                    return new CooldownResponse() { CooledDown = true, Seconds = Seconds - (int)(DateTime.Now - tempUser.DateTime).TotalSeconds };
                }
                else
                {
                    return new CooldownResponse() { CooledDown = false, Seconds = Seconds - (int)(DateTime.Now - tempUser.DateTime).TotalSeconds };
                }
            }
            else
            {
                UserCooldown NewUser = new UserCooldown();
                NewUser.UserID = user.Id;
                NewUser.CooldownSeconds = Seconds;
                NewUser.DateTime = DateTime.Now;
                list.Add(NewUser);
                return new CooldownResponse() { CooledDown = true, Seconds = Seconds - (int)(DateTime.Now - tempUser.DateTime).TotalSeconds };
            }
        }
        public CooldownResponse CheckCooldown(SocketUser user, string type)
        {
            var list = new List<UserCooldown>();
            switch (type)
            {
                case "SearchPlayer":
                    list = SearchPlayer;
                    break;
                case "Sync":
                    list = Sync;
                    break;
                case "Link":
                    list = Link;
                    break;
                case "ApplySteam":
                    list = ApplySteam;
                    break;
                case "ApplySquad":
                    list = ApplySquad;
                    break;
                default:
                    throw new Exception($"Invalid cooldown type: {type}");
            }
            var tempUser = list.FirstOrDefault(x => x.UserID == user.Id);
            if (tempUser.UserID != 0)
            {
                return new CooldownResponse() { Seconds = tempUser.CooldownSeconds - (int)(DateTime.Now - tempUser.DateTime).TotalSeconds, CooledDown = tempUser.CooldownSeconds - (int)(DateTime.Now - tempUser.DateTime).TotalSeconds <= 0 ? true : false };
            }
            return new CooldownResponse() { Seconds = 0, CooledDown = true };
        }
    }
}
