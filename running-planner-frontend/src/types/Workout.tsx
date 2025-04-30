export interface Workout {
    workoutID: number;
    trainingPlanID: number;
    type: string; // Type of workout (e.g., "legs", "strength", etc.)
    weekNumber: number; // 1-16 for a 16-week plan
    dayOfTheWeek: number; // 0-6 for Sunday-Saturday
    timeOfDay?: string | null; // Time in HH:MM format
    duration?: number | null; // in minutes
    notes?: string | null;
    completed: boolean; // true if the workout has been completed, false otherwise
    createdAt: string;
}
