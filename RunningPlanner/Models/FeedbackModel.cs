using System.ComponentModel.DataAnnotations;

namespace RunningPlanner.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        public int RunID { get; set; } // Foreign key to RunsModel

        public int EffortRating { get; set; } // Scale of 1-10

        public int FeelRating { get; set; } // Scale of 1-10

        public int? Pace { get; set; } // in seconds per kilometer

        public int? Duration { get; set; } // in seconds

        public int? Distance { get; set; } // in kilometers

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}