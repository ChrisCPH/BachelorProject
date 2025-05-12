import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { Run } from "../types/Run";
import { Workout } from "../types/Workout";
import { AddRunForm } from "../components/AddRunForm";
import { AddWorkoutForm } from "../components/AddWorkoutForm";
import { dayNames } from "../utils/DayNames";
import FormatPace from "../utils/FormatPace";
import FormatDuration from "../utils/FormatDuration";
import { handleAuthError } from "../utils/AuthError";
import { AddUserForm } from "../components/AddUserForm";
import { AddFeedbackForm } from "../components/AddFeedbackForm";
import { FeedbackConfirmation } from "../components/FeedbackConfirmation";

interface DayItem {
    type: "run" | "workout";
    data: Run | Workout;
}

type WeekSchedule = {
    [weekNumber: number]: {
        [dayOfWeek: number]: DayItem[];
    };
};

export default function TrainingPlanDetails() {
    const { id } = useParams<{ id: string }>();
    const [schedule, setSchedule] = useState<WeekSchedule>({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showAddRun, setShowAddRun] = useState(false);
    const [showAddUser, setShowAddUser] = useState(false);
    const [showAddWorkout, setShowAddWorkout] = useState(false);
    const [showAddFeedback, setShowAddFeedback] = useState(false);
    const [planDuration, setPlanDuration] = useState<number | null>(null);
    const [editRun, setEditRun] = useState<Run | null>(null);
    const [editWorkout, setEditWorkout] = useState<Workout | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [feedbackRun, setFeedbackRun] = useState<Run | null>(null);
    const [showFeedbackConfirmation, setShowFeedbackConfirmation] = useState(false);
    const [runToComplete, setRunToComplete] = useState<Run | null>(null);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [planResponse, runsResponse, workoutsResponse] = await Promise.all([
                    fetch(`${API_BASE_URL}/trainingplan/${id}`, {
                        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                    }),
                    fetch(`${API_BASE_URL}/run/trainingPlan/${id}`, {
                        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                    }),
                    fetch(`${API_BASE_URL}/workout/trainingPlan/${id}`, {
                        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                    }),
                ]);

                if (!planResponse.ok) {
                    throw new Error(`Failed to fetch plan details. Status: ${planResponse.status}`);
                }
                const planData = await planResponse.json();
                setPlanDuration(planData.duration);

                const combined: WeekSchedule = {};

                // Handle runs response, even if no runs are found
                if (runsResponse.ok) {
                    const runs: Run[] = await runsResponse.json();
                    runs.forEach(run => {
                        if (!combined[run.weekNumber]) combined[run.weekNumber] = {};
                        if (!combined[run.weekNumber][run.dayOfWeek]) combined[run.weekNumber][run.dayOfWeek] = [];
                        combined[run.weekNumber][run.dayOfWeek].push({ type: "run", data: run });
                    });
                }

                // Handle workouts response, even if no workouts are found
                if (workoutsResponse.ok) {
                    const workouts: Workout[] = await workoutsResponse.json();
                    workouts.forEach(workout => {
                        if (!combined[workout.weekNumber]) combined[workout.weekNumber] = {};
                        if (!combined[workout.weekNumber][workout.dayOfWeek]) combined[workout.weekNumber][workout.dayOfWeek] = [];
                        combined[workout.weekNumber][workout.dayOfWeek].push({ type: "workout", data: workout });
                    });
                }

                setSchedule(combined);

            } catch (err: any) {
                console.error("Error fetching data:", err);
                setError(err.message || "An unknown error occurred.");
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id]);

    if (loading) return <div className="text-light container mt-5">Loading...</div>;
    if (error) return <div className="text-danger container mt-5">{error}</div>;

    const handleEditRunSubmit = async (updatedRun: Run) => {
        try {
            const response = await fetch(`${API_BASE_URL}/run/update`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(updatedRun),
            });

            if (await handleAuthError(response, setErrorMessage, `editing run`)) return;
            if (!response.ok) throw new Error("Failed to update run");

            setSchedule(prev => {
                const updated = { ...prev };
                const week = updatedRun.weekNumber;
                const day = updatedRun.dayOfWeek;
                if (!updated[week]) updated[week] = {};
                if (!updated[week][day]) updated[week][day] = [];

                updated[week][day] = updated[week][day].map(item =>
                    item.type === "run" && (item.data as Run).runID === updatedRun.runID
                        ? { type: "run", data: updatedRun }
                        : item
                );
                return updated;
            });

            setEditRun(null);
        } catch (err) {
            console.error("Error updating run:", err);
        }
    };

    const handleEditWorkoutSubmit = async (updatedWorkout: Workout) => {
        try {
            const response = await fetch(`${API_BASE_URL}/workout/update`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(updatedWorkout),
            });

            if (await handleAuthError(response, setErrorMessage, `editing workout`)) return;
            if (!response.ok) throw new Error("Failed to update workout");

            setSchedule(prev => {
                const updated = { ...prev };
                const week = updatedWorkout.weekNumber;
                const day = updatedWorkout.dayOfWeek;
                if (!updated[week]) updated[week] = {};
                if (!updated[week][day]) updated[week][day] = [];

                updated[week][day] = updated[week][day].map(item =>
                    item.type === "workout" && (item.data as Workout).workoutID === updatedWorkout.workoutID
                        ? { type: "workout", data: updatedWorkout }
                        : item
                );
                return updated;
            });

            setEditWorkout(null);
        } catch (err) {
            console.error("Error updating workout:", err);
        }
    };

    const handleDelete = async (item: Run | Workout, type: "run" | "workout") => {
        const itemId = type === "run" ? (item as Run).runID : (item as Workout).workoutID;

        try {
            const response = await fetch(`${API_BASE_URL}/${type}/delete/${itemId}`, {
                method: "DELETE",
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                },
            });

            if (await handleAuthError(response, setErrorMessage, `deleting ${type}`)) return;
            if (!response.ok) {
                throw new Error(`Failed to delete ${type}. Status: ${response.status}`);
            }

            setSchedule((prev) => {
                const updated = { ...prev };
                const week = type === "run" ? (item as Run).weekNumber : (item as Workout).weekNumber;
                const day = type === "run" ? (item as Run).dayOfWeek : (item as Workout).dayOfWeek;

                updated[week][day] = updated[week][day].filter((d) =>
                    type === "run" ? (d.data as Run).runID !== itemId : (d.data as Workout).workoutID !== itemId
                );

                return updated;
            });
        } catch (err) {
            console.error(err);
            alert(`Error deleting ${type}.`);
        }
    };

    const handleComplete = async (
        item: Run | Workout,
        type: "run" | "workout",
        completed: boolean
    ) => {

        try {
            if (type === "run" && completed) {
                setRunToComplete(item as Run);
                setShowFeedbackConfirmation(true);
                return;
            }

            await completeItem(item, type, completed);
        } catch (err) {
            console.error(err);
        }
    };

    const completeItem = async (item: Run | Workout, type: "run" | "workout", completed: boolean) => {
        const itemId = type === "run" ? (item as Run).runID : (item as Workout).workoutID;

        const response = await fetch(`${API_BASE_URL}/${type}/complete/${itemId}`, {
            method: "PATCH",
            headers: {
                Authorization: `Bearer ${localStorage.getItem("token")}`,
                "Content-Type": "application/json",
            },
            body: JSON.stringify(completed),
        });

        if (await handleAuthError(response, setErrorMessage, `completing ${type}`)) return;
        if (!response.ok) {
            throw new Error(`Failed to update ${type} completion status. Status: ${response.status}`);
        }

        setSchedule(prev => {
            const updated = { ...prev };
            const week = type === "run" ? (item as Run).weekNumber : (item as Workout).weekNumber;
            const day = type === "run" ? (item as Run).dayOfWeek : (item as Workout).dayOfWeek;

            updated[week][day] = updated[week][day].map(d => {
                if (type === "run" && (d.data as Run).runID === itemId) {
                    return { ...d, data: { ...d.data, completed } };
                }
                if (type === "workout" && (d.data as Workout).workoutID === itemId) {
                    return { ...d, data: { ...d.data, completed } };
                }
                return d;
            });

            return updated;
        });
    };

    return (
        <div className="container mt-5 text-light">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <h2>Training Plan Details</h2>
            {planDuration && <p><strong>Plan Duration:</strong> {planDuration} weeks</p>}
            <div className="mb-4">
                {/* Add Run Button */}
                <button className="btn btn-primary" onClick={() => setShowAddRun(true)}>
                    Add Run
                </button>

                {/* Add Workout Button */}
                <button className="btn btn-primary" onClick={() => setShowAddWorkout(true)}>
                    Add Workout
                </button>

                {/* Add User Button */}
                <button className="btn btn-primary" onClick={() => setShowAddUser(true)}>
                    Add User
                </button>
            </div>

            {/* Add Run Form Modal */}
            {showAddRun && (
                <div className="modal">
                    <AddRunForm
                        trainingPlanId={parseInt(id!)}
                        maxDuration={planDuration ?? 0}
                        onClose={() => { setShowAddRun(false) }}
                        onSubmit={(newRun) => {
                            setShowAddRun(false);
                            setSchedule((prev) => {
                                const updated = { ...prev };
                                if (!updated[newRun.weekNumber]) updated[newRun.weekNumber] = {};
                                if (!updated[newRun.weekNumber][newRun.dayOfWeek]) updated[newRun.weekNumber][newRun.dayOfWeek] = [];

                                const exists = updated[newRun.weekNumber][newRun.dayOfWeek].some(
                                    (item) => item.type === "run" && (item.data as Run).runID === newRun.runID
                                );

                                if (!exists) {
                                    updated[newRun.weekNumber][newRun.dayOfWeek].push({ type: "run", data: newRun });
                                }

                                return updated;
                            });
                        }}

                    />
                </div>
            )}

            {/* Add Workout Form Modal */}
            {showAddWorkout && (
                <div className="modal">
                    <AddWorkoutForm
                        trainingPlanId={parseInt(id!)}
                        maxDuration={planDuration ?? 0}
                        onClose={() => setShowAddWorkout(false)}
                        onSubmit={(newWorkout) => {
                            setShowAddWorkout(false);
                            setSchedule((prev) => {
                                const updated = { ...prev };
                                if (!updated[newWorkout.weekNumber]) updated[newWorkout.weekNumber] = {};
                                if (!updated[newWorkout.weekNumber][newWorkout.dayOfWeek]) updated[newWorkout.weekNumber][newWorkout.dayOfWeek] = [];
                                const exists = updated[newWorkout.weekNumber][newWorkout.dayOfWeek].some(
                                    (item) => item.type === "workout" && (item.data as Workout).workoutID === newWorkout.workoutID
                                );

                                if (!exists) {
                                    updated[newWorkout.weekNumber][newWorkout.dayOfWeek].push({ type: "workout", data: newWorkout });
                                }
                                return updated;
                            });
                        }}
                    />
                </div>
            )}

            {showAddUser && (
                <div className="modal">
                    <AddUserForm
                        trainingPlanId={parseInt(id!)}
                        onSubmit={() => { }}
                        onClose={() => setShowAddUser(false)}
                    />
                </div>
            )}

            {showAddFeedback && feedbackRun && (
                <div className="modal">
                    <AddFeedbackForm
                        runId={feedbackRun.runID}
                        onSubmit={() => {
                            setShowAddFeedback(false);
                            completeItem(feedbackRun, "run", true);
                            setFeedbackRun(null);
                        }}
                        onClose={() => {
                            setShowAddFeedback(false);
                            completeItem(feedbackRun, "run", true);
                            setFeedbackRun(null);
                        }}
                    />
                </div>
            )}

            {/* Edit Run Form Modal */}
            {editRun && (
                <div className="modal">
                    <AddRunForm
                        trainingPlanId={parseInt(id!)}
                        maxDuration={planDuration ?? 0}
                        initialData={editRun}
                        onClose={() => setEditRun(null)}
                        onSubmit={handleEditRunSubmit}
                    />
                </div>
            )}

            {/* Edit Workout Form Modal */}
            {editWorkout && (
                <div className="modal">
                    <AddWorkoutForm
                        trainingPlanId={parseInt(id!)}
                        maxDuration={planDuration ?? 0}
                        initialData={editWorkout}
                        onClose={() => setEditWorkout(null)}
                        onSubmit={handleEditWorkoutSubmit}
                    />
                </div>
            )}

            {showFeedbackConfirmation && runToComplete && (
                <FeedbackConfirmation
                    onConfirm={() => {
                        setShowFeedbackConfirmation(false);
                        setFeedbackRun(runToComplete);
                        setShowAddFeedback(true);
                    }}
                    onCancel={() => {
                        setShowFeedbackConfirmation(false);
                        completeItem(runToComplete, "run", true);
                    }}
                />
            )}

            <div style={{ width: '100%' }}>
                {Array.from({ length: planDuration || 0 }, (_, weekIndex) => {
                    const weekNumber = weekIndex + 1;
                    const days = schedule[weekNumber] || {};
                    return (
                        <div key={weekNumber} className="mb-4">
                            <h4 className="mt-4">Week {weekNumber}</h4>
                            <div className="d-flex flex-wrap mb-3">
                                {dayNames.map((dayName, index) => {
                                    const dayItems = days[index] || [];
                                    return (
                                        <div
                                            key={index}
                                            className="card text-light bg-secondary me-3 mb-4"
                                            style={{ width: "22rem", flex: "1 0 300px" }}
                                        >
                                            <div className="card-body">
                                                <h5 className="card-title">{dayName}</h5>
                                                {dayItems.length > 0 ? (
                                                    dayItems.map((item, itemIndex) => {
                                                        let itemCount = 1;
                                                        if (item.type === "run") {
                                                            const runsToday = dayItems.filter((d) => d.type === "run");
                                                            itemCount = runsToday.indexOf(item) + 1;
                                                        } else if (item.type === "workout") {
                                                            const workoutsToday = dayItems.filter((d) => d.type === "workout");
                                                            itemCount = workoutsToday.indexOf(item) + 1;
                                                        }

                                                        return (
                                                            <div
                                                                key={itemIndex}
                                                                className="bg-dark text-light mb-3 p-3 rounded"
                                                                style={{ border: "1px solid #444" }}
                                                            >
                                                                <div className="d-flex justify-content-between align-items-start">
                                                                    <div>
                                                                        <p className="mb-2"><strong>{item.type === "run" ? `Run ${itemCount}` : `Workout ${itemCount}`}</strong></p>
                                                                        {item.type === "run" ? (
                                                                            <>
                                                                                <p className="mb-1"><strong>Run Type:</strong> {(item.data as Run).type ?? "Not specified"}</p>
                                                                                <p className="mb-1"><strong>Time:</strong> {(item.data as Run).timeOfDay ?? "Not specified"}</p>
                                                                                <p className="mb-1"><strong>Distance:</strong> {(item.data as Run).distance ?? "Not specified"} km</p>
                                                                                <p className="mb-1"><strong>Pace:</strong> {typeof (item.data as Run).pace === "number" ? FormatPace((item.data as Run).pace!) : "Not specified"}</p>
                                                                                <p className="mb-1"><strong>Duration:</strong> {typeof (item.data as Run).duration === "number" ? FormatDuration((item.data as Run).duration!) : "Not specified"} min</p>
                                                                                <p className="mb-1"><strong>Notes:</strong> {(item.data as Run).notes ?? "Not specified"}</p>
                                                                                <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                                                                    <strong>Completed?</strong>
                                                                                    <input
                                                                                        type="checkbox"
                                                                                        checked={item.data.completed}
                                                                                        onChange={(e) => handleComplete(item.data, item.type, e.target.checked)}
                                                                                        style={{ transform: "scale(1.3)", cursor: "pointer", accentColor: "#4CAF50" }}
                                                                                    />
                                                                                </label>
                                                                                <Link to={`/feedbackcomment/${(item.data as Run).runID}`}>
                                                                                    <button className="btn btn-sm btn-outline-light mt-2">View Feedback and Comments</button>
                                                                                </Link>
                                                                            </>
                                                                        ) : (
                                                                            <>
                                                                                <p className="mb-1"><strong>Workout Type:</strong> {(item.data as Workout).type ?? "Not specified"}</p>
                                                                                <p className="mb-1"><strong>Time:</strong> {(item.data as Workout).timeOfDay ?? "Not specified"}</p>
                                                                                <p className="mb-1"><strong>Duration:</strong> {typeof (item.data as Workout).duration === "number" ? FormatDuration((item.data as Workout).duration!) : "Not specified"} min</p>
                                                                                <p className="mb-1"><strong>Notes:</strong> {(item.data as Workout).notes ?? "Not specified"}</p>
                                                                                <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                                                                    <strong>Completed?</strong>
                                                                                    <input
                                                                                        type="checkbox"
                                                                                        checked={item.data.completed}
                                                                                        onChange={(e) => handleComplete(item.data, item.type, e.target.checked)}
                                                                                        style={{ transform: "scale(1.3)", cursor: "pointer", accentColor: "#4CAF50" }}
                                                                                    />
                                                                                </label>
                                                                                <Link to={`/exercises/${(item.data as Workout).workoutID}`}>
                                                                                    <button className="btn btn-sm btn-outline-light mt-2">View Exercises</button>
                                                                                </Link>
                                                                            </>
                                                                        )}
                                                                    </div>
                                                                    <div className="d-flex flex-column align-items-end ms-3">
                                                                        <button
                                                                            onClick={() =>
                                                                                item.type === "run"
                                                                                    ? setEditRun(item.data as Run)
                                                                                    : setEditWorkout(item.data as Workout)
                                                                            }
                                                                            className="btn btn-link p-0 m-0 text-secondary"
                                                                            title="Edit"
                                                                        >
                                                                            <i className="fas fa-pen text-light"></i>
                                                                        </button>
                                                                        <button
                                                                            onClick={() => {
                                                                                const confirmed = window.confirm("Are you sure you want to delete this workout/run?");
                                                                                if (confirmed) {
                                                                                    handleDelete(item.data, item.type);
                                                                                }
                                                                            }}
                                                                            className="btn btn-link p-0 m-0 text-secondary mt-2"
                                                                            title="Delete"
                                                                        >
                                                                            <i className="fas fa-trash text-light"></i>
                                                                        </button>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        );
                                                    })
                                                ) : (
                                                    <p className="text-light">No runs or workouts</p>
                                                )}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div >
    );
}
