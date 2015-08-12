using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataLayer
{
    public class OAuthScope
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
    }
}
