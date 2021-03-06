﻿namespace Logic.Classes
{
    public class Authentication
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public Lid Lid { get; private set; }
        public int KassaId { get; private set; }
        public Authentication(string username, string password, Lid lid)
        {
            Username = username;
            Password = password;
            Lid = lid;
        }

        public Authentication(int id, string username, string password, Lid lid, int kassaId) : this(username, password, lid)
        {
            Id = id;
            KassaId = kassaId;
        }

        public bool SetUsername(string username)
        {
            Username = username;
            return true;
        }

        public bool SetPassword(string password)
        {
            if (Password != password)
            {
                Password = password;
                return true;
            }
            return false;
        }

        public void SetLid(Lid lid)
        {
            Lid = lid;
        }
        public string GetFullName()
        {
            // Lid is alleen null als er geen database connectie is.
            if (Lid != null)
            {
                if (Lid.Tussenvoegsel != "")
                {
                    return Lid.Voornaam + " " + Lid.Tussenvoegsel + " " + Lid.Achternaam;
                }
                return Lid.Voornaam + " " + Lid.Achternaam;
            }
            return "Admin";
        }
    }
}
