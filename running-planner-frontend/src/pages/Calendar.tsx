import { useEffect, useState } from "react";
import { Run } from "../types/Run";
import { TrainingPlan } from "../types/TrainingPlan";
import { Workout } from "../types/Workout";
import FormatPace from "../utils/FormatPace";
import FormatDuration from "../utils/FormatDuration";
import DatePicker from "react-datepicker";
import { formatLocalDate, isSunday } from "../utils/FormatLocalDate";
import { Link } from "react-router-dom";
import { TrainingPlanWithPermission } from "../types/TrainingPlanWithPermission";
import { handleAuthError } from "../utils/AuthError";
import { AddFeedbackForm } from "../components/AddFeedbackForm";
import { FeedbackConfirmation } from "../components/FeedbackConfirmation";

export default function Calendar() {
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
    const [plans, setPlans] = useState<TrainingPlan[]>([]);
    const [selectedPlanId, setSelectedPlanId] = useState<number | null>(null);
    const [selectedPlan, setSelectedPlan] = useState<TrainingPlan | null>(null);
    const [runs, setRuns] = useState<Run[]>([]);
    const [workouts, setWorkouts] = useState<Workout[]>([]);
    const [calendar, setCalendar] = useState<Date[][]>([]);
    const [currentMonth, setCurrentMonth] = useState<Date>(new Date());
    const [showStartDateModal, setShowStartDateModal] = useState(false);
    const [newStartDate, setNewStartDate] = useState<string>("");
    const [editAccess, setEditAccessPlans] = useState<TrainingPlanWithPermission[]>([]);
    const [noEditAccess, setNoEditAccessPlans] = useState<TrainingPlanWithPermission[]>([]);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [feedbackRun, setFeedbackRun] = useState<Run | null>(null);
    const [showFeedbackConfirmation, setShowFeedbackConfirmation] = useState(false);
    const [runToComplete, setRunToComplete] = useState<Run | null>(null);
    const [showAddFeedback, setShowAddFeedback] = useState(false);

    useEffect(() => {
        fetchPlans();
    }, []);

    useEffect(() => {
        if (selectedPlanId) {
            const allPlans = [...editAccess, ...noEditAccess];
            const plan = allPlans.find((p) => p.trainingPlanID === selectedPlanId) || null;
            setSelectedPlan(plan);

            if (plan) {
                if (plan.startDate) {
                    fetchPlanData(plan.trainingPlanID);
                } else {
                    const hasEditAccess = editAccess.some(p => p.trainingPlanID === plan.trainingPlanID);
                    if (hasEditAccess) {
                        setShowStartDateModal(true);
                    } else {
                        const nearestSunday = getNearestSunday(new Date());
                        const planWithStartDate = {
                            ...plan,
                            startDate: formatLocalDate(nearestSunday.toISOString())
                        };
                        setSelectedPlan(planWithStartDate);
                        fetchPlanData(plan.trainingPlanID);
                    }
                }
            }
        }
    }, [selectedPlanId, editAccess, noEditAccess]);

    const getNearestSunday = (date: Date): Date => {
        const day = date.getDay();
        const diff = date.getDate() - day + (day === 0 ? 0 : 7);
        return new Date(date.setDate(diff));
    };

    useEffect(() => {
        if (selectedPlan?.startDate) {
            generateCalendar(currentMonth);
        }
    }, [currentMonth, selectedPlan, runs, workouts]);

    const fetchPlans = async () => {
        try {
            const response = await fetch(`${API_BASE_URL}/trainingplan/planswithpermission`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                },
            });
            if (!response.ok) throw new Error("Failed to fetch training plans");

            const data: TrainingPlanWithPermission[] = await response.json();
            setEditAccessPlans(data.filter(plan => plan.permission === "owner" || plan.permission === "editor"));
            setNoEditAccessPlans(data.filter(plan => plan.permission === "viewer" || plan.permission === "commenter"));
            setPlans([...data]);
        } catch (error) {
            console.error("Error fetching plans:", error);
        }
    };

    const fetchPlanData = async (id: number) => {
        try {
            const [runsRes, workoutsRes] = await Promise.all([
                fetch(`${API_BASE_URL}/run/trainingPlan/${id}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }),
                fetch(`${API_BASE_URL}/workout/trainingPlan/${id}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }),
            ]);

            const runsData = await runsRes.json();
            const workoutsData = await workoutsRes.json();
            setRuns(runsData);
            setWorkouts(workoutsData);
        } catch (error) {
            console.error("Error fetching runs or workouts:", error);
        }
    };

    const updatePlanStartDate = async () => {
        if (!selectedPlan || !newStartDate) return;

        try {
            const updatedPlan = {
                ...selectedPlan,
                startDate: newStartDate
            };

            const response = await fetch(`${API_BASE_URL}/trainingplan/update`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(updatedPlan),
            });

            if (response.ok) {
                const updatedPlanData = await response.json();
                setSelectedPlan(updatedPlanData);
                setPlans(plans.map(plan =>
                    plan.trainingPlanID === updatedPlanData.trainingPlanID ? updatedPlanData : plan
                ));
                setShowStartDateModal(false);
                fetchPlanData(updatedPlanData.trainingPlanID);
            } else {
                console.error("Failed to update plan");
            }
        } catch (error) {
            console.error("Error updating plan:", error);
        }
    };

    const generateCalendar = (currentMonth: Date) => {
        const firstDayOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1);
        const lastDayOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1, 0);

        const weeks: Date[][] = [];
        let currentDate = new Date(firstDayOfMonth);

        if (currentDate.getDay() !== 0) {
            currentDate.setDate(currentDate.getDate() - currentDate.getDay());
        }

        while (currentDate <= lastDayOfMonth || currentDate.getDay() !== 0) {
            const weekDates: Date[] = [];
            for (let day = 0; day < 7; day++) {
                weekDates.push(new Date(currentDate));
                currentDate.setDate(currentDate.getDate() + 1);
            }
            weeks.push(weekDates);
        }

        setCalendar(weeks);
    };

    const handlePreviousMonth = () => {
        setCurrentMonth((prevDate) => {
            const newDate = new Date(prevDate);
            newDate.setMonth(newDate.getMonth() - 1);
            return newDate;
        });
    };

    const handleNextMonth = () => {
        setCurrentMonth((prevDate) => {
            const newDate = new Date(prevDate);
            newDate.setMonth(newDate.getMonth() + 1);
            return newDate;
        });
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

        try {
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

            if (type === "run") {
                setRuns(prevRuns =>
                    prevRuns.map(run =>
                        run.runID === itemId ? { ...run, completed } : run
                    )
                );
            } else {
                setWorkouts(prevWorkouts =>
                    prevWorkouts.map(workout =>
                        workout.workoutID === itemId ? { ...workout, completed } : workout
                    )
                );
            }
        } catch (err) {
            console.error(err);
        }
    };

    const getWeekNumber = (date: Date): number => {
        if (!selectedPlan?.startDate) return 0;
        const diff = date.getTime() - new Date(selectedPlan.startDate).getTime();
        return Math.floor(diff / (1000 * 3600 * 24 * 7)) + 1;
    };

    const renderCell = (date: Date) => {
        if (!selectedPlan?.startDate) return null;

        const runsOnDate = runs.filter(
            (r) =>
                r.weekNumber === getWeekNumber(date) && r.dayOfWeek === date.getDay()
        );
        const workoutsOnDate = workouts.filter(
            (w) =>
                w.weekNumber === getWeekNumber(date) && w.dayOfWeek === date.getDay()
        );

        const isStartDate =
            selectedPlan?.startDate &&
            new Date(selectedPlan.startDate).toDateString() === date.toDateString();

        const weeksFromStart = selectedPlan?.duration || 0;
        const endDate = selectedPlan?.startDate
            ? new Date(selectedPlan.startDate)
            : null;

        if (endDate) {
            endDate.setDate(endDate.getDate() + weeksFromStart * 7);
        }

        const isEndDate = endDate && endDate.toDateString() === date.toDateString();

        return (
            <td
                key={date.toISOString()}
                className="border p-2 align-top bg-secondary"
                style={{
                    minHeight: "10%",
                    width: "12%",
                    wordWrap: "break-word",
                    overflow: "hidden",
                    whiteSpace: "normal",
                    position: "relative",
                }}
            >
                <strong className="text-white">{date.getDate()}</strong>

                <div
                    className="position-absolute"
                    style={{
                        right: "5px",
                        top: "5px",
                        fontSize: "0.7rem",
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "flex-end",
                    }}
                >
                    {isStartDate && (
                        <span className="badge bg-success" style={{ fontSize: "0.7rem" }}>
                            Start
                        </span>
                    )}
                    {isEndDate && (
                        <span className="badge bg-danger" style={{ fontSize: "0.7rem" }}>
                            End
                        </span>
                    )}
                </div>

                <div>
                    {runsOnDate.length > 0 && (
                        <div className="text-white">
                            {runsOnDate.map((run, index) => (
                                <div key={index} className="mb-2">
                                    <div className="d-flex justify-content-between align-items-center">
                                        <div>
                                            <strong>Run {index + 1}</strong>
                                            <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                                <strong>Completed?</strong>
                                                <input
                                                    type="checkbox"
                                                    checked={run.completed}
                                                    onChange={(e) => handleComplete(run, "run", e.target.checked)}
                                                    style={{ transform: "scale(1.3)", cursor: "pointer", accentColor: "#4CAF50" }}
                                                />
                                            </label>
                                            {typeof run.timeOfDay === "string" && <div>{run.timeOfDay}</div>}
                                            {run.type && <div>Type: {run.type}</div>}
                                            {typeof run.distance === "number" && <div>Distance: {run.distance} km</div>}
                                            {typeof run.pace === "number" && <div>Pace: {FormatPace(run.pace)}</div>}
                                            {typeof run.duration === "number" && <div>Duration: {FormatDuration(run.duration)} min</div>}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                    {workoutsOnDate.length > 0 && (
                        <div className="text-white">
                            {workoutsOnDate.map((workout, index) => (
                                <div key={index} className="mb-2">
                                    <div className="d-flex justify-content-between align-items-center">
                                        <div>
                                            <strong>Workout {index + 1}</strong>
                                            <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                                <strong>Completed?</strong>
                                                <input
                                                    type="checkbox"
                                                    checked={workout.completed}
                                                    onChange={(e) => handleComplete(workout, "workout", e.target.checked)}
                                                    style={{ transform: "scale(1.3)", cursor: "pointer", accentColor: "#4CAF50" }}
                                                />
                                            </label>
                                            {typeof workout.timeOfDay === "string" && <div>{workout.timeOfDay}</div>}
                                            {workout.type && <div>Type: {workout.type}</div>}
                                            {typeof workout.duration === "number" && <div>Duration: {FormatDuration(workout.duration)} min</div>}
                                            <Link to={`/exercises/${workout.workoutID}`}>
                                                <button className="btn btn-sm btn-outline-light mt-1">View Exercises</button>
                                            </Link>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </td>
        );
    };

    return (
        <div className="container text-white mt-5 p-4 rounded">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <h2 className="text-center mb-4">Training Calendar</h2>

            <div className="mb-4">
                <label className="form-label">Select Training Plan</label>
                <select
                    className="form-select"
                    value={selectedPlanId ?? ""}
                    onChange={(e) => setSelectedPlanId(Number(e.target.value))}
                >
                    <option value="">-- Select a Plan --</option>
                    {editAccess.map((plan) => (
                        <option key={plan.trainingPlanID} value={plan.trainingPlanID}>
                            {plan.name}
                        </option>
                    ))}
                    {noEditAccess.map((plan) => (
                        <option key={plan.trainingPlanID} value={plan.trainingPlanID}>
                            {plan.name} (View only)
                        </option>
                    ))}
                </select>
            </div>

            {calendar.length > 0 && (
                <div>
                    <div className="d-flex justify-content-between mb-4">
                        <button
                            className="btn btn-secondary"
                            onClick={handlePreviousMonth}
                        >
                            Previous
                        </button>
                        <span className="h5">
                            {currentMonth.toLocaleString('en-US', { month: 'long', year: 'numeric' })}
                        </span>
                        <button
                            className="btn btn-secondary"
                            onClick={handleNextMonth}
                        >
                            Next
                        </button>
                    </div>

                    <table className="table table-bordered mt-4">
                        <thead>
                            <tr>
                                <th className="text-center text-white bg-dark">Sun</th>
                                <th className="text-center text-white bg-dark">Mon</th>
                                <th className="text-center text-white bg-dark">Tue</th>
                                <th className="text-center text-white bg-dark">Wed</th>
                                <th className="text-center text-white bg-dark">Thu</th>
                                <th className="text-center text-white bg-dark">Fri</th>
                                <th className="text-center text-white bg-dark">Sat</th>
                            </tr>
                        </thead>
                        <tbody>
                            {calendar.map((week, i) => (
                                <tr key={i}>
                                    {week.map((date) => renderCell(date))}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {showStartDateModal && (
                <div className="modal show" style={{ display: 'block' }}>
                    <div className="modal-dialog">
                        <div className="modal-content bg-dark text-white">
                            <div className="modal-header">
                                <h5 className="modal-title">Set Start Date</h5>
                                <button
                                    type="button"
                                    className="btn-close btn-close-white"
                                    onClick={() => setShowStartDateModal(false)}
                                ></button>
                            </div>
                            <div className="modal-body">
                                <p>Please select a start date for your training plan:</p>
                                <DatePicker
                                    selected={newStartDate ? new Date(newStartDate) : null}
                                    onChange={(date: Date | null) => {
                                        setNewStartDate(date ? formatLocalDate(date.toISOString()) : "");
                                    }}
                                    filterDate={isSunday}
                                    className="form-control"
                                    dateFormat="yyyy-MM-dd"
                                    placeholderText="Select a Sunday"
                                />
                            </div>
                            <div className="modal-footer">
                                <button
                                    type="button"
                                    className="btn btn-secondary"
                                    onClick={() => setShowStartDateModal(false)}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="button"
                                    className="btn btn-primary"
                                    onClick={updatePlanStartDate}
                                    disabled={!newStartDate}
                                >
                                    Save
                                </button>
                            </div>
                        </div>
                    </div>
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
        </div>
    );
}