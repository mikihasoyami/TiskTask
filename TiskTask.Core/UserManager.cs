using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.Core;

public class UserManager
{
    private UsersFileStorage _usersStorage = new UsersFileStorage("Users.csv");
    List<UserModel> _users;

    public UserManager()
    {
        _users = _usersStorage.Load();
    }

    public bool IsUser(string login, string password)
    {
        foreach (var user in _users)
            if (user.Login == login && user.Password == password) 
                return true;
        return false;
    }

    public UserModel GetUser(string login, string password)
    {
        foreach (var user in _users)
        {
            if (user.Login == login && user.Password == password)
                return user;
        }
        return new UserModel();
    }

    public void CreateNewUser(string login, string password)
    {

        _users.Add(new UserModel(_users.Count, login, password));
        _usersStorage.Save(_users);
    }
}
