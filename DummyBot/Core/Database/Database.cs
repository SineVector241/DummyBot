using System.Data.SQLite;
using Newtonsoft.Json.Linq;

namespace DummyBot.Core.Database
{
    public struct User
    {
        public ulong ID { get; set; }
        public bool IsSteam { get; set; }
        public string WBID { get; set; }
        public string WBName { get; set; }
        public string Level { get; set; }
        public string Kills { get; set; }
        public string Deaths { get; set; }
        public string XP { get; set; }
        public string KD { get; set; }
        public string KPM { get; set; }
        public string WeaponKills { get; set; }
        public string VehicleKills { get; set; }
        public string DamageDealt { get; set; }
        public string HeadShots { get; set; }

    }
    public struct Squad
    {
        public string Name { get; set; }
        public string DeathMatch { get; set; }
        public string BattleRoyale { get; set; }
        public string MissileLaunch { get; set; }
        public string PackageDrop { get; set; }
        public string VehicleEscort { get; set; }
        public string ZombieBR { get; set; }
        public string CapturePoint { get; set; }
        public JObject Members { get; set; }
        public DateTime LastUpdate { get; set; }
    }
    public struct SquadServer
    {
        public ulong ID { get; set; }
        public ulong NotifyChannel { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
        public bool WarStatus { get; set; }
        public string Tag { get; set; }
    }
    public class Database
    {
        SQLiteDBContext db = new SQLiteDBContext();

        public Database()
        {
            CreateTable().GetAwaiter().GetResult();
        }

        private async Task CreateTable()
        {
            string query = "CREATE TABLE IF NOT EXISTS Users (ID varchar(18), IsSteam boolean, WBID string, WBName string, Level string, Kills string, Deaths string, XP string, KD string, KPM string, WeaponKills string, VehicleKills string, DamageDealt string, HeadShots string)";
            string query2 = "CREATE TABLE IF NOT EXISTS Squads (Name string, DeathMatch string, BattleRoyale string, MissileLaunch string, PackageDrop string, VehicleEscort string, ZombieBR string, CapturePoint string, Members string, LastUpdate string)";
            string query3 = "CREATE TABLE IF NOT EXISTS SquadServers (ID varchar(18), NotifyChannel varchar(18), TimeZone string, WarStatus boolean, Tag string)";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            SQLiteCommand cmd2 = new SQLiteCommand(query2, db.MyConnection);
            SQLiteCommand cmd3 = new SQLiteCommand(query3, db.MyConnection);
            cmd.Prepare();
            cmd2.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            cmd2.ExecuteNonQuery();
            cmd3.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
            await cmd2.DisposeAsync();
            await cmd3.DisposeAsync();
        }
        public async Task CreateUserAsync(User user)
        {
            string query = $"INSERT INTO Users (ID, IsSteam, WBID, WBName, Level, Kills, Deaths, XP, KD, KPM, WeaponKills, VehicleKills, DamageDealt, HeadShots) VALUES (@ID, @IsSteam, @WBID, @WBName, @Level, @Kills, @Deaths, @XP, @KD, @KPM, @WeaponKills, @VehicleKills, @DamageDealt, @HeadShots)";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Parameters.AddWithValue("@ID", user.ID);
            cmd.Parameters.AddWithValue("@IsSteam", user.IsSteam);
            cmd.Parameters.AddWithValue("@WBID", user.WBID);
            cmd.Parameters.AddWithValue("@WBName", user.WBName);
            cmd.Parameters.AddWithValue("@Level", user.Level);
            cmd.Parameters.AddWithValue("@Kills", user.Kills);
            cmd.Parameters.AddWithValue("@KD", user.KD);
            cmd.Parameters.AddWithValue("@Deaths", user.Deaths);
            cmd.Parameters.AddWithValue("@XP", user.XP);
            cmd.Parameters.AddWithValue("@KPM", user.KPM);
            cmd.Parameters.AddWithValue("@WeaponKills", user.WeaponKills);
            cmd.Parameters.AddWithValue("@VehicleKills", user.VehicleKills);
            cmd.Parameters.AddWithValue("@DamageDealt", user.DamageDealt);
            cmd.Parameters.AddWithValue("@HeadShots", user.HeadShots);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }
        public async Task<bool> HasProfileAsync(ulong id)
        {
            string query = $"SELECT * FROM Users WHERE ID = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            bool hasActivity = false;
            if (result.HasRows) hasActivity = true;
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return hasActivity;
        }
        public async Task UpdateUserAsync(User user)
        {
            string query = $"UPDATE Users SET IsSteam = {user.IsSteam}, WBID = \"{user.WBID}\", WBName = \"{user.WBName}\", Level = \"{user.Level}\", Kills = \"{user.Kills}\", Deaths = \"{user.Deaths}\", XP = \"{user.XP}\", KD = \"{user.KD}\", KPM = \"{user.KPM}\", WeaponKills = \"{user.WeaponKills}\", VehicleKills = \"{user.VehicleKills}\", DamageDealt = \"{user.DamageDealt}\", HeadShots = \"{user.HeadShots}\" WHERE ID = {user.ID}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }
        public async Task<User> GetUserByIdAsync(ulong id)
        {
            string query = $"SELECT * FROM Users WHERE ID = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            User profile = new User();
            if (result.HasRows) while (result.Read())
                {
                    profile.ID = id;
                    profile.IsSteam = Convert.ToBoolean(result["IsSteam"]);
                    profile.WBID = result["WBID"].ToString();
                    profile.WBName = result["WBName"].ToString();
                    profile.Level = result["Level"].ToString();
                    profile.Kills = result["Kills"].ToString();
                    profile.Deaths = result["Deaths"].ToString();
                    profile.XP = result["XP"].ToString();
                    profile.KD = result["KD"].ToString();
                    profile.KPM = result["KPM"].ToString();
                    profile.WeaponKills = result["WeaponKills"].ToString();
                    profile.VehicleKills = result["VehicleKills"].ToString();
                    profile.DamageDealt = result["DamageDealt"].ToString();
                    profile.HeadShots = result["HeadShots"].ToString();
                }
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return profile;
        }
        public async Task DeleteUserAsync(ulong id)
        {
            string query = $"DELETE FROM Users WHERE ID = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }

        public async Task CreateSquadAsync(Squad squad)
        {
            string query = "INSERT INTO Squads (Name, DeathMatch, BattleRoyale, MissileLaunch, PackageDrop, VehicleEscort, ZombieBR, CapturePoint, Members, LastUpdate) VALUES (@Name, @DeathMatch, @BattleRoyale, @MissileLaunch, @PackageDrop, @VehicleEscort, @ZombieBR, @CapturePoint, @Members, @LastUpdate)";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Parameters.AddWithValue("@Name", squad.Name);
            cmd.Parameters.AddWithValue("@DeathMatch", squad.DeathMatch);
            cmd.Parameters.AddWithValue("@BattleRoyale", squad.BattleRoyale);
            cmd.Parameters.AddWithValue("@MissileLaunch", squad.MissileLaunch);
            cmd.Parameters.AddWithValue("@PackageDrop", squad.PackageDrop);
            cmd.Parameters.AddWithValue("@VehicleEscort", squad.VehicleEscort);
            cmd.Parameters.AddWithValue("@ZombieBR", squad.ZombieBR);
            cmd.Parameters.AddWithValue("@CapturePoint", squad.CapturePoint);
            cmd.Parameters.AddWithValue("@Members", squad.Members.ToString().Replace(" ", "").Replace(@"\n", ""));
            cmd.Parameters.AddWithValue("@LastUpdate", squad.LastUpdate.ToString());
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }
        public async Task<bool> HasSquadAsync(string name)
        {
            string query = $"SELECT * FROM Squads WHERE Name = \"{name}\"";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            bool hasActivity = false;
            if (result.HasRows) hasActivity = true;
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return hasActivity;
        }
        public async Task UpdateSquadAsync(Squad squad)
        {
            string query = @$"UPDATE Squads SET DeathMatch = '{squad.DeathMatch}', MissileLaunch = '{squad.MissileLaunch}', PackageDrop = '{squad.PackageDrop}', VehicleEscort = '{squad.VehicleEscort}', ZombieBR = '{squad.ZombieBR}', CapturePoint = '{squad.CapturePoint}', Members = '{squad.Members.ToString().Replace(" ", "").Replace(@"\n", "")}', LastUpdate = '{squad.LastUpdate}' WHERE Name = '{squad.Name}'";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }
        public async Task<Squad> GetSquadByNameAsync(string name)
        {
            string query = $"SELECT * FROM Squads WHERE Name = \"{name}\"";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            Squad squad = new Squad();
            if (result.HasRows) while (result.Read())
                {
                    squad.Name = name;
                    squad.DeathMatch = result["DeathMatch"].ToString();
                    squad.BattleRoyale = result["BattleRoyale"].ToString();
                    squad.MissileLaunch = result["MissileLaunch"].ToString();
                    squad.PackageDrop = result["PackageDrop"].ToString();
                    squad.VehicleEscort = result["VehicleEscort"].ToString();
                    squad.ZombieBR = result["ZombieBR"].ToString();
                    squad.CapturePoint = result["CapturePoint"].ToString();
                    squad.Members = JObject.Parse(result["Members"].ToString());
                    squad.LastUpdate = DateTime.Parse(result["LastUpdate"].ToString());
                }
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return squad;
        }
        public async Task DeleteSquadAsync(string name)
        {
            string query = $"DELETE FROM Squads WHERE Name = \"{name}\"";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }

        public async Task CreateSquadServerAsync(SquadServer server)
        {
            string query = "INSERT INTO SquadServers (ID, NotifyChannel, TimeZone, WarStatus, Tag) VALUES (@ID, @NotifyChannel, @TimeZone, @WarStatus, @Tag)";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Parameters.AddWithValue("@ID", server.ID);
            cmd.Parameters.AddWithValue("@NotifyChannel", server.NotifyChannel);
            cmd.Parameters.AddWithValue("@TimeZone", server.TimeZone.StandardName);
            cmd.Parameters.AddWithValue("@WarStatus", server.WarStatus);
            cmd.Parameters.AddWithValue("@Tag", server.Tag);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }
        public async Task<bool> HasSquadServerAsync(ulong id)
        {
            string query = $"SELECT * FROM SquadServers WHERE ID = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            bool Exists = false;
            if (result.HasRows) Exists = true;
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return Exists;
        }

        public async Task<bool> HasSquadServerByTagAsync(string tag)
        {
            string query = $"SELECT * FROM SquadServers WHERE Tag = \"{tag}\"";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            bool Exists = false;
            if (result.HasRows) Exists = true;
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return Exists;
        }
        public async Task UpdateSquadServerAsync(SquadServer server)
        {
            string query = $"UPDATE SquadServers SET NotifyChannel = {server.NotifyChannel}, TimeZone = \"{server.TimeZone.StandardName}\", WarStatus = {server.WarStatus}, Tag = \"{server.Tag}\" WHERE ID = {server.ID}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
        }
        public async Task<SquadServer> GetSquadServerByIdAsync(ulong id)
        {
            string query = $"SELECT * FROM SquadServers WHERE ID = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            SquadServer squadServer = new SquadServer();
            if(result.HasRows) while(result.Read())
                {
                    squadServer.ID = id;
                    squadServer.NotifyChannel = (ulong)Convert.ToInt64(result["NotifyChannel"]);
                    squadServer.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(result["TimeZone"].ToString());
                    squadServer.WarStatus = (bool)result["WarStatus"];
                    squadServer.Tag = result["Tag"].ToString();
                }
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return squadServer;
        }

        public async Task<SquadServer> GetSquadServerByTagAsync(string tag)
        {
            string query = $"SELECT * FROM SquadServers WHERE Tag = \"{tag}\"";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            cmd.Prepare();
            db.OpenConnection();
            SQLiteDataReader result = cmd.ExecuteReader();
            SquadServer squadServer = new SquadServer();
            if (result.HasRows) while (result.Read())
                {
                    squadServer.ID = (ulong)Convert.ToInt64(result["ID"]);
                    squadServer.NotifyChannel = (ulong)Convert.ToInt64(result["NotifyChannel"]);
                    squadServer.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(result["TimeZone"].ToString());
                    squadServer.WarStatus = (bool)result["WarStatus"];
                    squadServer.Tag = result["Tag"].ToString();
                }
            db.CloseConnection();
            await cmd.DisposeAsync();
            await result.DisposeAsync();
            return squadServer;
        }
    }
}
