using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

      public string PreferredDistance { get; set; } = "km"; // km or miles

      public string PreferredWeight { get; set; } = "kg"; // kg or lbs

      [JsonIgnore]
      public List<UserTrainingPlan> UserTrainingPlans { get; set; } = new List<UserTrainingPlan>();
   }
}