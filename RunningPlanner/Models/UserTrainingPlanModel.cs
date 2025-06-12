using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RunningPlanner.Models
{
    public class UserTrainingPlan
    {
        [Key]
        public int UserTrainingPlanID { get; set; }

        public int UserID { get; set; } // Foreign key to UsersModel

        public int TrainingPlanID { get; set; } // Foreign key to TrainingPlansModel

        [MaxLength(20)]
        public string Permission { get; set; } = "viewer"; // Owner, editor, commenter, viewer

        [JsonIgnore]
        public TrainingPlan TrainingPlan { get; set; } = null!;

        [JsonIgnore]
        public User User { get; set; } = null!;
    }
}