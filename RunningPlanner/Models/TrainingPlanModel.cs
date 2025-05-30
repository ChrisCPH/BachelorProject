using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RunningPlanner.Models
{
    public class TrainingPlan
    {
        [Key]
        public int TrainingPlanID { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; } // Null until the user starts the plan

        public int Duration { get; set; } // In weeks

        [MaxLength(50)]
        public string? Event { get; set; } // "Marathon", "Half Marathon", "5K" ...

        [MaxLength(50)]
        public string? GoalTime { get; set; } // e.g. "3:30:00" for a marathon

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<UserTrainingPlan> UserTrainingPlans { get; set; } = new List<UserTrainingPlan>();

        [JsonIgnore]
        public List<Run> Runs { get; set; } = new List<Run>();

        [JsonIgnore]
        public List<Workout> Workouts { get; set; } = new List<Workout>();
    }
}
