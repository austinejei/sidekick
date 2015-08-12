using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class UserApp
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }

        public DateTime DateInstalled { get; set; }
        public DateTime? DateUninstalled { get; set; }

        public bool IsInstalled { get; set; }

        public int AppId { get; set; }
        public virtual App App { get; set; }
    }
}