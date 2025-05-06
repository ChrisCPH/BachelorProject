export interface Comment {
    commentID: number;
    userID: number;
    runID?: number | null;
    workoutID?: number | null;
    Text: string;
    createdAt: string;
}