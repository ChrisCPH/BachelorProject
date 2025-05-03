export interface Run {
    runID: number;
    trainingPlanID: number;
    type?: string | null; // Type of run (e.g., "easy", "long", "interval", etc.)
    weekNumber: number; // 1-16 for a 16-week plan
    dayOfWeek: number; // 0-6 for Sunday-Saturday
    timeOfDay?: string | null; // Time in HH:MM format
    pace?: number | null; // in minutes per kilometer
    duration?: number | null; // in minutes
    distance?: number | null; // in kilometers
    notes?: string | null;
    completed: boolean; // true if the run has been completed, false otherwise
    routeId?: number | null; // ID of the route associated with this run
    createdAt: string;
}