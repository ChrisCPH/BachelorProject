export interface Comment {
    commentID: number;
    userId: number;
    runId?: number | null;
    workoutId?: number | null;
    Text: string;
    createdAt: string;
}