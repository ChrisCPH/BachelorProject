using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RunningPlanner.Models
{
    public class Workout
    {
        [Key]
        public int WorkoutID { get; set; }

        public int TrainingPlanID { get; set; } // Foreign key to TrainingPlansModel

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // e.g., "Strength", "Cross-Training"
        
        public int WeekNumber { get; set; } // Week number in the training plan

        public DayOfWeek DayOfWeek { get; set; } // Day of the week for the workout. Sunday = 0, Monday = 1, ..., Saturday = 6

        public TimeSpan? TimeOfDay { get; set; } // Time of day for the workout. hh:mm:ss format

        public int? Duration { get; set; } // in seconds

        public string? Notes { get; set; }

        public bool Completed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<Exercise> Exercises { get; set; } = new List<Exercise>(); // List of exercises in the workout

        [JsonIgnore]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}