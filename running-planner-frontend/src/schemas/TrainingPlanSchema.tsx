export type TrainingPlanFormData = {
    name: string;
    startDate?: string | null;
    duration: number; // in weeks
    event?: string | null;
    goalTime?: string | null;
    createdAt: string;
};