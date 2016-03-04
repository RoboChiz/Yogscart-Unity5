using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
Gamemode Base Class v1.0
Created by Robert (Robo_Chiz)
FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
If you wanna add something add it as part of your child class.
Thanks
*/



public class GameMode
{
    protected float maxPlayers = 12f;
}

public class TeamGameMode : GameMode
{

    protected List<Team> teams;

    //Used to handle Team Names & Max number of players per team
    public class Team
    {
        /// <summary>
        /// Create a new Team for your GameMode
        /// </summary>
        /// <param name="tn">The name of the new Team.</param>
        /// <param name="mp">The maximum number of players allowed in this Team.</param>
        public Team(string tn, int mp)
        {
            teamName = tn;
            maxPlayers = mp;
        }

        //Team name is set during declaration and cannot be changed directely.
        private string teamName;
        public string TeamName
        {
            get { return teamName; }
            set { }
        }

        //Maximum number of players in team is set during declaration and cannot be changed directely.
        private int maxPlayers;
        public int MaxPlayers
        {
            get { return maxPlayers; }
            set { }
        }

    }

}

public class TestClass : TeamGameMode
{
        
}

