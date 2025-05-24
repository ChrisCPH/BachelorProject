## Introduction

This project was developed as part of my bachelor project in Software Development at Copenhagen Business Academy. It is a running planner application that enables runners to design personalized and detailed training plans.

## Technology Stack

| Layer | Tools |
| -------- | ------- |
| Version Control | Git, GitHub |
| IDE | Visual Studio Code |
| Programming Language | C#, TypeScript |
| Database | mssql, mongodb |
| CI/CD | GitHub Actions |
| Api | REST |
| Project management | GitHub Projects |
| Frontend Framework | React |
| Testing | XUnit |
| Api Testing | Postman |

## User Stories and Acceptance Criteria

#### Runner

1. As a runner I want to create an account so that I can save my training plans and routes.

    **Acceptance Criteria:**
    1. To create an account the runner must provide a valid email, password and a unique username.
    2. The password must be at least 8 characters long and include at least one number.

2. As a runner I want to be able to log in to my account so that I can access my training plans.

    **Acceptance Criteria:**
    1. The runner must enter a valid email and password to log in.

3. As a runner I want to be able to create a training plan so that I can plan my runs and workouts.

    **Acceptance Criteria:**
    1. To create a training plan the runner has to enter a name and a duration/length in weeks.
    2. The runner should be able to create multiple training plans.

4. As a runner I want to add runs to my training plan so that I can train effectively.

    **Acceptance Criteria:**
    1. To create a run the runner must specify a week and a day of the week.
    2. The runner should be able to make the run repeat every week for a training plan with a checkbox.
    3. When adding a run the runner should be able to input distance, duration and pace.

5. As a runner I want to add workouts to my training plan so that I can train effectively.

    **Acceptance Criteria:**
    1. To create a workout the runner must specify the type of workout, week and day of the week.
    2. The runner should be able to make the workout repeat every week for a training plan with a checkbox.
    3. When adding a workout the runner should be able to input duration.

6. As a runner I want to be able to add my training plan to a calendar so that I know exactly when my runs and workouts are.

    **Acceptance Criteria:**
    1. Runs and workouts should be visible in a calendar view.
    2. Clicking on a run or workout should show details of the session.

7. As a runner I want to be able to create my running route on a map so that I can plan where to run ahead of time.

    **Acceptance Criteria:**
    1. The runner should be able to draw on the map.
    2. The routes should be able to be saved and changed later.

8. As a runner I want to give feedback on a run so that I can reflect on the session.

    **Acceptance Criteria:**
    1. When a run is completed the runner should be prompted to give feedback.
    2. A runner should give a rating 1-10 on their effort and how the run felt.

9. As a runner I want to give access to my training plan so that I can share it with other users.

    **Acceptance Criteria:**
    1. The runner should be able to decide between three levels of access viewer, commenter or editor.

#### Coach

10. As a coach I want to leave comments on runs or workouts so that the runner can receive feedback or notes.

    **Acceptance Criteria:**
    1. A coach should be able to comment on workouts and runs in a runners training plan with permission.
    2. Comments should be timestamped and able to be viewed by the runner.

11. As a coach I want to be able to edit the training plan so that it fits better for the runner.

    **Acceptance Criteria:**
    1. The coach should be able to edit the runners training plan with permission.