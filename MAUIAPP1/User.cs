﻿namespace MAUIAPP1
{
    
	public class User
    {
        string _name;
        int _id;

        public User(string name, int id)
		{
            _name = name;
            _id = id;
		}

        public string GetName()
        {
            return _name;
        }

        public int GetId()
        {
            return _id;
        }
    }

    
}

