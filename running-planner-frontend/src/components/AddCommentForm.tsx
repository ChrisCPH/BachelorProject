import { useEffect, useState } from "react";
import { Comment } from "../types/Comment";
import { handleAuthError } from "../utils/AuthError";

interface AddCommentFormProps {
    runId?: number | null;
    workoutId?: number | null;
    onSubmit: (comment: Comment) => void;
    onClose: () => void;
    initialData?: Comment;
}

export const AddCommentForm = ({ runId, workoutId, onSubmit, onClose, initialData }: AddCommentFormProps) => {
    const [text, setText] = useState("");
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    useEffect(() => {
        if (initialData) {
            setText(initialData.text);
        }
    }, [initialData]);

    const handleSubmit = async () => {
        if (!text.trim()) {
            setErrorMessage("Comment text is required.");
            return;
        }

        const newComment: Comment = {
            commentID: initialData?.commentID || 0,
            runID: runId || null,
            workoutID: workoutId || null,
            text: text.trim(),
            createdAt: initialData?.createdAt || new Date().toISOString(),
        };

        try {
            const url = initialData
                ? `${API_BASE_URL}/comment/update`
                : `${API_BASE_URL}/comment/add`;

            const method = initialData ? "PUT" : "POST";

            const response = await fetch(url, {
                method: method,
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                },
                body: JSON.stringify(newComment),
            });

            if (await handleAuthError(response, setErrorMessage, "adding/updating comment")) return;
            if (!response.ok) {
                throw new Error("Failed to save comment");
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
                <h5>{initialData ? "Edit Comment" : "Add Comment"}</h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
            </div>

            <div className="form-group mb-3">
                <label>Comment</label>
                <textarea
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                    rows={5}
                />
            </div>

            <div className="d-flex justify-content-between">
                <button onClick={handleSubmit} className="btn btn-primary w-100 rounded-2 py-2">
                    {initialData ? "Save Changes" : "Add Comment"}
                </button>
            </div>
        </div>
    );
};