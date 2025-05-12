export interface Comment {
    commentID: number;
    userID?: number;
    runID?: number | null;
    workoutID?: number | null;
    text: string;
    createdAt: string;
}