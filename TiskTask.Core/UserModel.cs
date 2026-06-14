using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.Core;

public class UserModel
{
    public long Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }

    public UserModel(int id, string login, string password)
    {
        Id = id;
        Login = login;
        Password = password;
    }
    public UserModel()
    {
        Id = -1;
        Login = string.Empty;
        Password = string.Empty;
    }
}
