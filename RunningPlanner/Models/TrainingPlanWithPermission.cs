namespace RunningPlanner.Models
{
    public class TrainingPlanWithPermission
    {
        public int TrainingPlanID { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public int Duration { get; set; }
        public string? Event { get; set; }
        public string? GoalTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Permission { get; set; } = "viewer";
    }
}