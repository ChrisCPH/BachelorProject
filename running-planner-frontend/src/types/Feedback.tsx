export interface Feedback {
    feedbackID: number;
    runID: number;
    effortRating: number; // 1 to 10 scale
    feelRating: number; // 1 to 10 scale
    pace?: number | null; // in minutes per kilometer
    duration?: number | null; // in minutes
    distance?: number | null; // in kilometers
    createdAt: string;
    comment?: string | null;
}