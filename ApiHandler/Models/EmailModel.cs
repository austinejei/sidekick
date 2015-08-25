using System.ComponentModel.DataAnnotations;

namespace ApiHandler.Models
{
    public class EmailModel
    {
        [Required]
        public string To { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
    }
}