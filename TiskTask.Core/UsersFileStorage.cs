using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.Core;

public class UsersFileStorage
{
    private readonly string _filePath;

    public UsersFileStorage(string fileName)
    {
        _filePath = fileName;

        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Dispose();
        }
    }

    public List<UserModel> Load()
    {
        var result = new List<UserModel>();

        if (!File.Exists(_filePath))
        {
            return result;
        }

        var lines = File.ReadAllLines(_filePath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(';');
            foreach (var part in parts)
            {
                var user = part.Split(" ");

                result.Add(new UserModel
                {
                    Login = user[0],
                    Password = user[1]
                });
            }
        }

        return result;
    }

    public void Save(IEnumerable<UserModel> users)
    {
        var lines = users.Select(user =>
            string.Join(" ",
                user.Login,
                user.Password));

        File.WriteAllLines(_filePath, lines);
    }

}
