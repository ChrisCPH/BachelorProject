import { useState, useEffect } from "react";
import { Exercise } from "../types/Exercise";
import { handleAuthError } from "../utils/AuthError";

interface ExerciseFormProps {
    workoutId?: number;
    onSubmit: (exercise: Exercise) => void;
    onClose: () => void;
    initialData?: Exercise;
}

export const AddExerciseForm = ({ workoutId, onSubmit, onClose, initialData }: ExerciseFormProps) => {
    const [exercise, setExercise] = useState<Partial<Exercise>>(initialData || {});
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
    const token = localStorage.getItem("token");

    useEffect(() => {
        if (initialData) {
            setExercise(initialData);
        }
    }, [initialData]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!exercise.name) {
            setErrorMessage("Please enter a name for the exercise");
            return;
        }

        try {
            const url = initialData
                ? `${API_BASE_URL}/exercise/update`
                : `${API_BASE_URL}/exercise/add`;
                
            const method = initialData ? "PUT" : "POST";

            const payload = initialData 
                ? exercise 
                : {
                    ...exercise,
                    workoutID: workoutId
                };

            const response = await fetch(url, {
                method,
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify(payload),
            });

            if (await handleAuthError(response, setErrorMessage, initialData ? "updating exercise" : "adding exercise")) return;
            if (!response.ok) {
                throw new Error(initialData ? "Failed to update exercise" : "Failed to add exercise");
            }

            const result = await response.json();
            onSubmit(result);
            onClose();
        } catch (err) {
            console.error(err);
            setErrorMessage((err as Error).message);
        }
    };

    return (
        <div className="modal-content bg-dark text-white p-4 rounded-3 shadow-lg">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <div className="modal-header border-bottom-0">
                <h5 className="modal-title">{initialData ? "Edit Exercise" : "Add Exercise"}</h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
            </div>
            <div className="modal-body">
                <form onSubmit={handleSubmit}>
                    <div className="mb-3">
                        <label className="form-label">Name</label>
                        <input
                            className="form-control bg-secondary text-white border-0 rounded-2"
                            required
                            value={exercise.name ?? ""}
                            onChange={(e) => setExercise({ ...exercise, name: e.target.value })}
                        />
                    </div>
                    <div className="mb-3">
                        <label className="form-label">Sets</label>
                        <input
                            type="number"
                            className="form-control bg-secondary text-white border-0 rounded-2"
                            value={exercise.sets ?? ""}
                            onChange={(e) => setExercise({ ...exercise, sets: Number(e.target.value) })}
                        />
                    </div>
                    <div className="mb-3">
                        <label className="form-label">Reps</label>
                        <input
                            type="number"
                            className="form-control bg-secondary text-white border-0 rounded-2"
                            value={exercise.reps ?? ""}
                            onChange={(e) => setExercise({ ...exercise, reps: Number(e.target.value) })}
                        />
                    </div>
                    <div className="mb-3">
                        <label className="form-label">Weight (kg)</label>
                        <input
                            type="number"
                            className="form-control bg-secondary text-white border-0 rounded-2"
                            value={exercise.weight ?? ""}
                            onChange={(e) => setExercise({ ...exercise, weight: parseFloat(e.target.value) })}
                        />
                    </div>
                    <button type="submit" className="btn btn-primary w-100 rounded-2 py-2">
                        {initialData ? "Update" : "Add"}
                    </button>
                </form>
            </div>
        </div>
    );
};