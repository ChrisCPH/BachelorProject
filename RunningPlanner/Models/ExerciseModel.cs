using System.ComponentModel.DataAnnotations;

namespace RunningPlanner.Models
{
    public class Exercise
    {
        [Key]
        public int ExerciseID { get; set; }

        public int WorkoutID { get; set; } // Foreign key to WorkoutModel

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Name of the exercise

        public int? Sets { get; set; } // Number of sets for the exercise

        public int? Reps { get; set; } // Number of repetitions for the exercise

        public int? Weight { get; set; } // Weight used for the exercise in kg
    }
}