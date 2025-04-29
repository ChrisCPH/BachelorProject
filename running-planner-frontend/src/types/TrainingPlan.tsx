export interface TrainingPlan {
    trainingPlanId: number;
    name: string;
    startDate: string;
    duration: number; // in weeks
    event: string;
    goalTime: string;
}