import { useState, useEffect } from "react";
import { Run } from "../types/Run";
import { dayNames } from "../utils/DayNames";
import { handleAuthError } from "../utils/AuthError";

interface AddRunFormProps {
    trainingPlanId: number;
    maxDuration: number;
    onSubmit: (run: Run) => void;
    onClose: () => void;
    initialData?: Run;
}

const runTypes = ["Easy", "Moderate", "Hard"];

export const AddRunForm = ({ trainingPlanId, maxDuration, onSubmit, onClose, initialData }: AddRunFormProps) => {
    const [type, setType] = useState("");
    const [weekNumber, setWeekNumber] = useState(1);
    const [dayOfWeek, setDayOfWeek] = useState(0);
    const [timeOfDay, setTimeOfDay] = useState("");
    const [distance, setDistance] = useState(0);
    const [notes, setNotes] = useState("");
    const [error, setError] = useState("");
    const [paceInput, setPaceInput] = useState(""); // string like "5:20"
    const [pace, setPace] = useState(0); // actual seconds
    const [durationInput, setDurationInput] = useState(""); // string like "30:00"
    const [duration, setDuration] = useState(0); // actual seconds
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    useEffect(() => {
        if (initialData) {
            setType(initialData.type || "");
            setWeekNumber(initialData.weekNumber);
            setDayOfWeek(initialData.dayOfWeek);
            setTimeOfDay(initialData.timeOfDay || "");
            setDistance(initialData.distance || 0);
            setNotes(initialData.notes || "");
            setPaceInput(initialData.pace ? formatPace(initialData.pace) : "");
            setPace(initialData.pace || 0);
            setDurationInput(initialData.duration ? formatDuration(initialData.duration) : "");
            setDuration(initialData.duration || 0);
        }
    }, [initialData]);

    const handleSubmit = async () => {
        if (!weekNumber || dayOfWeek === null) {
            setError("Week number and day of week are required.");
            return;
        }

        const newRun: Run = {
            runID: initialData?.runID || 0,
            trainingPlanID: trainingPlanId,
            type: type || null,
            weekNumber,
            dayOfWeek,
            timeOfDay: timeOfDay || null,
            distance: distance > 0 ? distance : null,
            pace: pace > 0 ? pace : null,
            duration: duration > 0 ? duration : null,
            notes: notes || null,
            completed: initialData?.completed || false,
            createdAt: initialData?.createdAt || new Date().toISOString(),
        };

        try {
            const url = initialData
                ? `${API_BASE_URL}/run/update`
                : `${API_BASE_URL}/run/add`;

            const method = initialData ? "PUT" : "POST";

            const response = await fetch(url, {
                method: method,
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(newRun),
            });

            if (await handleAuthError(response, setErrorMessage, `adding/updating run`)) return;
            if (!response.ok) {
                throw new Error("Failed to save run");
            }

            const result = await response.json();
            onSubmit(result);
        } catch (error) {
            console.error(error);
        }
    };

    const parseTimeToSeconds = (input: string): number => {
        if (!input) return 0;

        if (!input.includes(":")) {
            return Number(input) * 60;
        }

        const [minutes, seconds] = input.split(":").map(Number);
        if (isNaN(minutes) || isNaN(seconds)) return 0;
        return minutes * 60 + seconds;
    };

    const formatPace = (seconds: number): string => {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        return `${minutes}:${remainingSeconds.toString().padStart(2, "0")}`;
    };

    const formatDuration = (seconds: number): string => {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        return `${minutes}:${remainingSeconds.toString().padStart(2, "0")}`;
    };

    return (
        <div className="modal-content bg-dark text-white p-4 rounded-3 shadow-lg">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <div className="modal-header border-bottom-0">
                <h5>{initialData ? "Edit Run" : "Add Run"}</h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
            </div>

            {error && <div className="alert alert-danger">{error}</div>}

            <div className="form-group mb-3">
                <label>Week Number</label>
                <select
                    value={weekNumber}
                    onChange={(e) => setWeekNumber(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                >
                    {Array.from({ length: maxDuration }, (_, i) => (
                        <option key={i + 1} value={i + 1}>
                            Week {i + 1}
                        </option>
                    ))}
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Day of Week</label>
                <select
                    value={dayOfWeek}
                    onChange={(e) => setDayOfWeek(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                >
                    {dayNames.map((name, index) => (
                        <option key={index} value={index}>
                            {name}
                        </option>
                    ))}
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Type</label>
                <select
                    value={type}
                    onChange={(e) => setType(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                >
                    <option value="">Select Run Type</option>
                    {runTypes.map((runType, index) => (
                        <option key={index} value={runType}>
                            {runType}
                        </option>
                    ))}
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Time of Day</label>
                <input
                    type="time"
                    value={timeOfDay}
                    onChange={(e) => setTimeOfDay(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Distance (km)</label>
                <input
                    type="number"
                    value={distance}
                    onChange={(e) => setDistance(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Pace (min/km)</label>
                <input
                    type="text"
                    placeholder="e.g., 5:20"
                    value={paceInput}
                    onChange={(e) => {
                        const value = e.target.value;
                        setPaceInput(value);
                        setPace(parseTimeToSeconds(value));
                    }}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Duration (mm:ss)</label>
                <input
                    type="text"
                    placeholder="e.g., 30:00"
                    value={durationInput}
                    onChange={(e) => {
                        const value = e.target.value;
                        setDurationInput(value);
                        setDuration(parseTimeToSeconds(value));
                    }}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Notes</label>
                <textarea
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="d-flex justify-content-between">
                <button onClick={handleSubmit} className="btn btn-primary w-100 rounded-2 py-2">
                    {initialData ? "Save Changes" : "Add Run"}
                </button>
            </div>
        </div>
    );
};
