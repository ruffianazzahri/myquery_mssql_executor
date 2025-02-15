using System.ComponentModel.DataAnnotations;

namespace MyQuery.Models
{
    public class SaveQueryModel
    {
        public int No { get; set; }
        public string SavedQuery { get; set; }
        public string SavedName { get; set; }
        public string Username { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
    }

}
