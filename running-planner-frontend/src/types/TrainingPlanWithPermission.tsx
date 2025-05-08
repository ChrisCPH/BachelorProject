import { TrainingPlan } from "./TrainingPlan";

export type TrainingPlanWithPermission = TrainingPlan & {
    permission: "owner" | "editor" | "commenter"| "viewer";
};
