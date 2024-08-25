using System.ComponentModel.DataAnnotations;

namespace Synergy_Test.Models
{
    public class RequestedQueryModel
    {
        public int No { get; set; }
        public string Requester { get; set; }
        public string Status { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ExecutedDate { get; set; }
        public string SQLQuery { get; set; }
    }

}
