using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiskTask.Core
{
    public class UserTask
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime Created { get; private set; }
        public int TimeSpent {  get; private set; }
    }
}
