import { useEffect, useState } from "react";
import { Run } from "../types/Run";
import { TrainingPlan } from "../types/TrainingPlan";
import { Workout } from "../types/Workout";
import FormatPace from "../utils/FormatPace";
import FormatDuration from "../utils/FormatDuration";

export default function CalendarPage() {
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
    const [plans, setPlans] = useState<TrainingPlan[]>([]);
    const [selectedPlanId, setSelectedPlanId] = useState<number | null>(null);
    const [selectedPlan, setSelectedPlan] = useState<TrainingPlan | null>(null);
    const [runs, setRuns] = useState<Run[]>([]);
    const [workouts, setWorkouts] = useState<Workout[]>([]);
    const [calendar, setCalendar] = useState<Date[][]>([]);
    const [currentMonth, setCurrentMonth] = useState<Date>(new Date());

    useEffect(() => {
        fetchPlans();
    }, []);

    useEffect(() => {
        if (selectedPlanId) {
            const plan = plans.find((p) => p.trainingPlanID === selectedPlanId) || null;
            setSelectedPlan(plan);
            if (plan?.startDate) {
                fetchPlanData(plan.trainingPlanID);
            }
        }
    }, [selectedPlanId]);

    useEffect(() => {
        if (selectedPlan?.startDate) {
            generateCalendar(currentMonth);
        }
    }, [currentMonth, selectedPlan, runs, workouts]);

    const fetchPlans = async () => {
        try {
            const res = await fetch(`${API_BASE_URL}/trainingplan/user`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            const data = await res.json();
            setPlans(data);
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

    const renderCell = (date: Date) => {
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
                                <div key={index}>
                                    <div>
                                        <strong>Run {index + 1}
                                            {run.completed && (
                                                <span className="text-white">✅</span>
                                            )}
                                        </strong>
                                    </div>
                                    {run.type && <div>Type: {run.type}</div>}
                                    {typeof run.distance === "number" && <div>Distance: {run.distance} km</div>}
                                    {typeof run.pace === "number" && <div>Pace: {FormatPace(run.pace)}</div>}
                                    {typeof run.duration === "number" && <div>Duration: {FormatDuration(run.duration)} min</div>}

                                </div>
                            ))}
                        </div>
                    )}

                    {workoutsOnDate.length > 0 && (
                        <div className="text-white">
                            {workoutsOnDate.map((workout, index) => (
                                <div key={index}>
                                    <div>
                                        <strong>Workout {index + 1}
                                            {workout.completed && (
                                                <span className="text-white">✅</span>
                                            )}
                                        </strong>
                                    </div>
                                    {workout.type && <div>Type: {workout.type}</div>}
                                    {typeof workout.duration === "number" && <div>Duration: {FormatDuration(workout.duration)} min</div>}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </td>
        );
    };


    const getWeekNumber = (date: Date): number => {
        if (!selectedPlan?.startDate) return 0;
        const diff = date.getTime() - new Date(selectedPlan.startDate).getTime();
        return Math.floor(diff / (1000 * 3600 * 24 * 7)) + 1;
    };

    return (
        <div className="container text-white mt-5 p-4 rounded">
            <h2 className="text-center mb-4">Training Calendar</h2>

            <div className="mb-4">
                <label className="form-label">Select Training Plan</label>
                <select
                    className="form-select"
                    value={selectedPlanId ?? ""}
                    onChange={(e) => setSelectedPlanId(Number(e.target.value))}
                >
                    <option value="">-- Select a Plan --</option>
                    {plans.map((plan) => (
                        <option key={plan.trainingPlanID} value={plan.trainingPlanID}>
                            {plan.name}
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
        </div>
    );
}
