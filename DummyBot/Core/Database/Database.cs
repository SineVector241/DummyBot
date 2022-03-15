using System.Data.SQLite;

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
            string query2 = "CREATE TABLE IF NOT EXISTS Squads (Name string, DeathMatch string, BattleRoyale string, MissileLaunch string, PackageDrop string, VehicleEscort string, ZombieBR string, CapturePoint string, " +
                "Member1 string, " +
                "Member2 string, " +
                "Member3 string, " +
                "Member4 string, " +
                "Member5 string, " +
                "Member6 string, " +
                "Member7 string, " +
                "Member8 string, " +
                "Member9 string, " +
                "Member10 string, " +
                "Member11 string, " +
                "Member12 string, " +
                "Member13 string, " +
                "Member14 string, " +
                "Member15 string, " +
                "Member16 string, " +
                "Member17 string, " +
                "Member18 string, " +
                "Member19 string, " +
                "Member20 string, " +
                "Member21 string, " +
                "Member22 string, " +
                "Member23 string, " +
                "Member24 string, " +
                "Member25 string, " +
                "Member26 string, " +
                "Member27 string, " +
                "Member28 string, " +
                "Member29 string, " +
                "Member30 string, " +
                "Member31 string, " +
                "Member32 string)";
            SQLiteCommand cmd = new SQLiteCommand(query, db.MyConnection);
            SQLiteCommand cmd2 = new SQLiteCommand(query2, db.MyConnection);
            cmd.Prepare();
            cmd2.Prepare(); 
            db.OpenConnection();
            cmd.ExecuteNonQuery();
            cmd2.ExecuteNonQuery();
            db.CloseConnection();
            await cmd.DisposeAsync();
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
    }
}
