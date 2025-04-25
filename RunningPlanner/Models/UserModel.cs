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

        public List<UserTrainingPlan> UserTrainingPlans { get; set; } = new List<UserTrainingPlan>();
     }
}