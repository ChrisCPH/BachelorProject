import { useState, useEffect } from "react";
import { Workout } from "../types/Workout";
import { dayNames } from "../utils/DayNames";
import { handleAuthError } from "../utils/AuthError";
import FormatDuration from "../utils/FormatDuration";
import { ParseTimeToSeconds } from "../utils/ParseTimeToSeconds";

interface AddWorkoutFormProps {
    trainingPlanId: number;
    maxDuration: number;
    onSubmit: (workout: Workout | Workout[]) => void;
    onClose: () => void;
    initialData?: Workout;
}

const workoutTypes = ["Strength", "Weights", "Cardio", "Flexibility", "Endurance", "Yoga", "Pilates", "HIIT", "Calisthenics", "Balance", "Aerobic", "Other"];

export const AddWorkoutForm = ({ trainingPlanId, maxDuration, onSubmit, onClose, initialData }: AddWorkoutFormProps) => {
    const [type, setType] = useState("");
    const [weekNumber, setWeekNumber] = useState(1);
    const [dayOfWeek, setDayOfWeek] = useState(0);
    const [timeOfDay, setTimeOfDay] = useState("");
    const [durationInput, setDurationInput] = useState(""); // string like "30:00"
    const [duration, setDuration] = useState(0); // actual seconds
    const [notes, setNotes] = useState("");
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [repeat, setRepeat] = useState(false);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    useEffect(() => {
        if (initialData) {
            setType(initialData.type || "");
            setWeekNumber(initialData.weekNumber);
            setDayOfWeek(initialData.dayOfWeek);
            setTimeOfDay(initialData.timeOfDay || "");
            setNotes(initialData.notes || "");
            setDurationInput(initialData.duration ? FormatDuration(initialData.duration) : "");
            setDuration(initialData.duration || 0);
        }
    }, [initialData]);

    const handleSubmit = async () => {
        if (!dayOfWeek && dayOfWeek !== 0) {
            setErrorMessage("Day of week is required.");
            return;
        }
        if (!repeat && (!weekNumber || weekNumber < 1)) {
            setErrorMessage("Week number is required if not repeating.");
            return;
        }

        const newWorkout: Workout = {
            workoutID: initialData?.workoutID || 0,
            trainingPlanID: trainingPlanId,
            type,
            weekNumber,
            dayOfWeek,
            timeOfDay: timeOfDay || null,
            duration: duration > 0 ? duration : null,
            completed: false,
            notes: notes || null,
            createdAt: initialData?.createdAt || new Date().toISOString(),
        };

        try {
            const url = repeat
                ? `${API_BASE_URL}/workout/add/repeat`
                : initialData
                    ? `${API_BASE_URL}/workout/update`
                    : `${API_BASE_URL}/workout/add`;

            const method = repeat ? "POST" : initialData ? "PUT" : "POST";

            const response = await fetch(url, {
                method: method,
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(newWorkout),
            });

            if (await handleAuthError(response, setErrorMessage, `adding/updating workout`)) return;
            if (!response.ok) {
                throw new Error("Failed to save workout");
            }

            const result = await response.json();

            if (repeat && Array.isArray(result)) {
                onSubmit(result);
            } else {
                onSubmit(result);
            }
        } catch (error) {
            console.error(error);
            setErrorMessage("An unexpected error occurred.");
        }
    };

    return (
        <div className="modal-content bg-dark text-white p-4 rounded-3 shadow-lg">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <div className="modal-header border-bottom-0">
                <h5>{initialData ? "Edit Workout" : "Add Workout"}</h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
            </div>

            {!initialData && (
                <div className="form-group mb-3">
                    <label>
                        <input
                            type="checkbox"
                            checked={repeat}
                            onChange={(e) => setRepeat(e.target.checked)}
                            className="me-2"
                        />
                        Repeat every week
                    </label>
                </div>
            )}

            {!repeat && (
                <div className="form-group mb-3">
                    <label>Week Number</label>
                    <select
                        value={weekNumber}
                        onChange={(e) => setWeekNumber(Number(e.target.value))}
                        className="form-control bg-secondary text-white border-0 rounded-2"
                    >
                        {Array.from({ length: maxDuration }, (_, i) => (
                            <option key={i + 1} value={i + 1}>Week {i + 1}</option>
                        ))}
                    </select>
                </div>
            )}

            <div className="form-group mb-3">
                <label>Day of Week</label>
                <select
                    value={dayOfWeek}
                    onChange={(e) => setDayOfWeek(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                >
                    {dayNames.map((name, index) => (
                        <option key={index} value={index}>{name}</option>
                    ))}
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Workout Type</label>
                <select
                    value={type}
                    onChange={(e) => setType(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2l"
                >
                    <option value="">Select Workout Type</option>
                    {workoutTypes.map((workoutType, index) => (
                        <option key={index} value={workoutType}>{workoutType}</option>
                    ))}
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Time of Day (optional)</label>
                <input
                    type="time"
                    value={timeOfDay}
                    onChange={(e) => setTimeOfDay(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Duration (mm:ss) (optional)</label>
                <input
                    type="text"
                    placeholder="e.g., 30:00"
                    value={durationInput}
                    onChange={(e) => {
                        const value = e.target.value;
                        setDurationInput(value);
                        setDuration(ParseTimeToSeconds(value));
                    }}
                    className="form-control white-placeholder bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Notes (optional)</label>
                <textarea
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>
            <div className="d-flex justify-content-between">
                <button onClick={handleSubmit} className="btn btn-primary">
                    {initialData ? "Save Changes" : "Add Workout"}
                </button>
            </div>
        </div>

    );
};
