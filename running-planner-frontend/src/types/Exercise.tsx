export interface Exercise {
    exerciseID: number;
    workoutID: number;
    name: string;
    sets?: number | null;
    reps?: number | null;
    weight?: number | null; // in kg
}