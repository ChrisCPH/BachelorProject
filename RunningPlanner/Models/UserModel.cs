using System.ComponentModel.DataAnnotations;

namespace RunningPlanner.Models
{
    public class User
     {
        [Key]
        public int UserID { get; set; }

        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(int.MaxValue)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [MaxLength(10)]
        public string PreferredUnit { get; set; } = "km"; // kilometers or miles

        public List<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
     }
}