using System.ComponentModel.DataAnnotations;

namespace RunningPlanner.Models
{
    public class Run
    {
        [Key]
        public int RunID { get; set; }
        
        public int TrainingPlanID { get; set; } // Foreign key to TrainingPlansModel

        public int WeekNumber { get; set; } // Week number in the training plan

        public DayOfWeek DayOfWeek { get; set; } // Day of the week for the workout. Sunday = 0, Monday = 1, ..., Saturday = 6

        public TimeSpan? TimeOfDay { get; set; } // Time of day for the workout. hh:mm:ss format

        public int? Pace { get; set; } // in seconds per kilometer

        public int? Duration { get; set; } // in seconds

        public int? Distance { get; set; } // in kilometers

        public string? Notes { get; set; }

        public string? Feedback { get; set; }

        public bool Completed { get; set; } = false;

        public string? RouteID { get; set; } // ID of the route used for the run from mongoDB
    }
}