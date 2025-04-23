using System.ComponentModel.DataAnnotations;

namespace RunningPlanner.Models
{
    public class TrainingPlan
    {
        [Key]
        public int TrainingPlanID { get; set; }

        public int UserID { get; set; } // Foreign key to UsersModel

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; } // Null until the user starts the plan

        public int Duration { get; set; } // In weeks

        [MaxLength(50)]
        public string? Event { get; set; } // "Marathon", "Half Marathon", "5K" ...

        public List<Run> Runs { get; set; } = new List<Run>();

        public List<Workout> Workouts { get; set; } = new List<Workout>();
    }
}
