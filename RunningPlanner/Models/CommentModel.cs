using System.ComponentModel.DataAnnotations;

namespace RunningPlanner.Models
{
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        public int UserID { get; set; } // Foreign key to the user who made the comment

        public int? RunID { get; set; } // Foreign key to RunsModel

        public int? WorkoutID { get; set; } // Foreign key to WorkoutModel

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = new User();

        public Run? Run { get; set; }

        public Workout? Workout { get; set; }
    }
}