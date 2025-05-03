export interface AddWorkoutSchema {
    type: string;
    weekNumber: number;
    dayOfWeek: number;
    timeOfDay: string;
    duration: number;
    notes: string;
}