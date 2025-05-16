import { useEffect, useState } from "react";
import { Feedback } from "../types/Feedback";
import FormatPace from "../utils/FormatPace";
import FormatDuration from "../utils/FormatDuration";
import { handleAuthError } from "../utils/AuthError";
import { ParseTimeToSeconds } from "../utils/ParseTimeToSeconds";

interface AddFeedbackFormProps {
    runId: number;
    onSubmit: (feedback: Feedback) => void;
    onClose: () => void;
    initialData?: Feedback;
}

export const AddFeedbackForm = ({ runId, onSubmit, onClose, initialData }: AddFeedbackFormProps) => {
    const [effortRating, setEffortRating] = useState(5);
    const [feelRating, setFeelRating] = useState(5);
    const [pace, setPace] = useState(0);
    const [paceInput, setPaceInput] = useState("");
    const [duration, setDuration] = useState(0);
    const [durationInput, setDurationInput] = useState("");
    const [distance, setDistance] = useState(0);
    const [comment, setComment] = useState("");
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;


    useEffect(() => {
        if (initialData) {
            setEffortRating(initialData.effortRating);
            setFeelRating(initialData.feelRating);
            setPaceInput(initialData.pace ? FormatPace(initialData.pace) : "");
            setPace(initialData.pace || 0);
            setDurationInput(initialData.duration ? FormatDuration(initialData.duration) : "");
            setDuration(initialData.duration || 0);
            setDistance(initialData.distance || 0);
            setComment(initialData.comment || "");
        }
    }, [initialData]);

    const handleSubmit = async () => {
        if (!effortRating || !feelRating) {
            setErrorMessage("Effort rating and feel rating are required.");
            return;
        }

        const newFeedback: Feedback = {
            feedbackID: initialData?.feedbackID || 0,
            runID: runId,
            effortRating,
            feelRating,
            pace: pace > 0 ? pace : null,
            duration: duration > 0 ? duration : null,
            distance: distance > 0 ? distance : null,
            createdAt: initialData?.createdAt || new Date().toISOString(),
            comment: comment || null,
        };

        try {
            const url = initialData
                ? `${API_BASE_URL}/feedback/update`
                : `${API_BASE_URL}/feedback/add`;

            const method = initialData ? "PUT" : "POST";

            const response = await fetch(url, {
                method: method,
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(newFeedback),
            });

            if (await handleAuthError(response, setErrorMessage, "adding/updating feedback")) return;
            if (!response.ok) {
                if (response.status === 409) {
                    const conflictText = await response.text();
                    setErrorMessage(conflictText || "Conflict occurred while saving feedback.");
                    return;
                } else {
                    throw new Error("Failed to save feedback");
                }
            }


            const result = await response.json();
            onSubmit(result);
        } catch (error) {
            console.error(error);
        }
    };

    return (
        <div className="modal-content bg-dark text-white p-4 rounded-3 shadow-lg">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <div className="modal-header border-bottom-0">
                <h5>{initialData ? "Edit Feedback" : "Add Feedback"}</h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
            </div>

            <div className="form-group mb-3">
                <label>Effort (1-10)</label>
                <select
                    value={effortRating}
                    onChange={(e) => setEffortRating(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                >
                    <option value={1}>1 - Minimal effort</option>
                    <option value={2}>2 - Very easy</option>
                    <option value={3}>3 - Easy run</option>
                    <option value={4}>4 - Comfortable</option>
                    <option value={5}>5 - Steady effort</option>
                    <option value={6}>6 - Somewhat hard</option>
                    <option value={7}>7 - Hard but sustainable</option>
                    <option value={8}>8 - Very hard</option>
                    <option value={9}>9 - Max effort</option>
                    <option value={10}>10 - All-out race effort</option>
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Feel (1-10)</label>
                <select
                    value={feelRating}
                    onChange={(e) => setFeelRating(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                >
                    <option value={1}>1 - Completely drained</option>
                    <option value={2}>2 - Very weak</option>
                    <option value={3}>3 - Weak</option>
                    <option value={4}>4 - Below average</option>
                    <option value={5}>5 - Neutral</option>
                    <option value={6}>6 - Slightly strong</option>
                    <option value={7}>7 - Strong</option>
                    <option value={8}>8 - Very strong</option>
                    <option value={9}>9 - Excellent</option>
                    <option value={10}>10 - Peak performance</option>
                </select>
            </div>

            <div className="form-group mb-3">
                <label>Actual Pace (optional)</label>
                <input
                    type="text"
                    placeholder="e.g., 5:20"
                    value={paceInput}
                    onChange={(e) => {
                        const value = e.target.value;
                        setPaceInput(value);
                        setPace(ParseTimeToSeconds(value));
                    }}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Actual Duration (optional)</label>
                <input
                    type="text"
                    placeholder="e.g., 30:00"
                    value={durationInput}
                    onChange={(e) => {
                        const value = e.target.value;
                        setDurationInput(value);
                        setDuration(ParseTimeToSeconds(value));
                    }}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Actual Distance (optional)</label>
                <input
                    type="number"
                    value={distance}
                    onChange={(e) => setDistance(Number(e.target.value))}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="form-group mb-3">
                <label>Comments (optional)</label>
                <textarea
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                />
            </div>

            <div className="d-flex justify-content-between">
                <button onClick={handleSubmit} className="btn btn-primary w-100 rounded-2 py-2">
                    {initialData ? "Save Changes" : "Add Feedback"}
                </button>
            </div>
        </div>
    );
};