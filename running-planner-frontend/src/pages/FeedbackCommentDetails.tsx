import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Run } from "../types/Run";
import { Feedback } from "../types/Feedback";
import { Comment } from "../types/Comment";
import { handleAuthError } from "../utils/AuthError";
import { dayNames } from "../utils/DayNames";
import FormatPace from "../utils/FormatPace";
import FormatDuration from "../utils/FormatDuration";
import { AddFeedbackForm } from "../components/AddFeedbackForm";
import { FeedbackConfirmation } from "../components/FeedbackConfirmation";
import { AddCommentForm } from "../components/AddCommentForm";

export default function FeedbackCommentsDetails() {
    const { id } = useParams<{ id: string }>();
    const [run, setRun] = useState<Run | null>(null);
    const [feedback, setFeedback] = useState<Feedback | null>(null);
    const [comment, setComment] = useState<Comment[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [editFeedback, setEditFeedback] = useState<Feedback | null>(null);
    const [editComment, setEditComment] = useState<Comment | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [feedbackRun, setFeedbackRun] = useState<Run | null>(null);
    const [showFeedbackConfirmation, setShowFeedbackConfirmation] = useState(false);
    const [runToComplete, setRunToComplete] = useState<Run | null>(null);
    const [showAddFeedback, setShowAddFeedback] = useState(false);
    const [usernames, setUsernames] = useState<{ [userId: number]: string }>({});
    const [showAddComment, setShowAddComment] = useState(false);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
    const token = localStorage.getItem("token");

    useEffect(() => {
        const fetchData = async () => {
            if (!token) {
                setError("No authentication token found.");
                setLoading(false);
                return;
            }

            try {
                const [runRes, feedbackRes, commentRes] = await Promise.all([
                    fetch(`${API_BASE_URL}/run/${id}`, {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                    fetch(`${API_BASE_URL}/feedback/run/${id}`, {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                    fetch(`${API_BASE_URL}/comment/run/${id}`, {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                ]);

                if (!runRes.ok) {
                    throw new Error("Failed to fetch run");
                }
                const runData: Run = await runRes.json();
                setRun(runData);

                let feedbackData: Feedback | null = null;
                if (feedbackRes.ok) {
                    const response = await feedbackRes.text();
                    feedbackData = response ? JSON.parse(response) : null;
                } else if (feedbackRes.status !== 404) {
                    feedbackData = null;
                }
                setFeedback(feedbackData);

                let commentData: Comment[] = [];
                if (commentRes.status === 200) {
                    commentData = await commentRes.json();
                } else if (commentRes.status === 204 || commentRes.status === 404) {
                    commentData = [];
                }
                setComment(commentData);

            } catch (err) {
                setError((err as Error).message);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id, token]);

    useEffect(() => {
        const uniqueUserIds = [
            ...new Set(comment.map((c) => c.userID).filter((id): id is number => typeof id === "number"))
        ];

        const fetchUsernames = async () => {
            const updatedUsernames: { [userId: number]: string } = { ...usernames };

            for (const userID of uniqueUserIds) {
                if (!updatedUsernames[userID]) {
                    const name = await getCommentUser(userID);
                    if (name) {
                        updatedUsernames[userID] = name;
                    }
                }
            }

            setUsernames(updatedUsernames);
        };

        fetchUsernames();
    }, [comment]);

    const handleComplete = async (completed: boolean) => {

        try {
            if (completed) {
                setRunToComplete(run);
                setShowFeedbackConfirmation(true);
                return;
            }

            await completeItem(completed);
        } catch (err) {
            console.error(err);
        }
    };

    const completeItem = async (completed: boolean) => {
        if (!token || !run) {
            setError("Missing token or run.");
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/run/complete/${run.runID}`, {
                method: "PATCH",
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(completed),
            });

            if (await handleAuthError(response, setErrorMessage, `completing run`)) return;
            if (!response.ok) {
                throw new Error(`Failed to update run completion status. Status: ${response.status}`);
            }

            setRun({ ...run, completed });
        } catch (err) {
            console.error(err);
        }
    };

    const handleDelete = async (id: number, type: "feedback" | "comment") => {
        if (!token) {
            setError("No authentication token found.");
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/${type}/delete/${id}`, {
                method: "DELETE",
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            if (await handleAuthError(response, setErrorMessage, `deleting ${type}`)) return;
            if (!response.ok) {
                throw new Error(`Failed to delete the ${type}.`);
            }

            if (type === "feedback") {
                setFeedback(null);
            } else if (type === "comment") {
                setComment((prev) =>
                    prev.filter((item) => item.commentID !== id)
                );
            }
        } catch (err) {
            setError((err as Error).message);
        }
    };

    const getCommentUser = async (userID: number): Promise<string | undefined> => {
        if (!token) {
            setError("No authentication token found.");
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/user/${userID}`, {
                headers: { Authorization: `Bearer ${token}` },
            });

            if (await handleAuthError(response, setErrorMessage, "getting user for comment")) return;
            if (!response.ok) throw new Error("Failed to get user");

            const data = await response.json();
            return data.userName;
        } catch (err) {
            setError((err as Error).message);
        }
    };

    const handleNewComment = (newComment: Comment) => {
        setComment([...comment, newComment]);
        setShowAddComment(false);
    };

    if (loading) return <p>Loading details...</p>;
    if (error) return <p className="text-danger">{error}</p>;
    if (!run) return <p>Run not found.</p>;

    return (
        <div className="container mt-4 text-light">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <h2 className="text-center mb-4">Feedback and Comments</h2>
            <div className="row justify-content-center">
                <div className="col-md-8">
                    <div className="card bg-secondary text-white p-4">
                        <h3>Run</h3>
                        <p><strong>Run Type:</strong> {run.type ?? "Not specified"}</p>
                        <p><strong>Week:</strong> {run.weekNumber}</p>
                        <p><strong>Day:</strong> {dayNames[run.dayOfWeek]}</p>
                        <p><strong>Time:</strong> {run.timeOfDay ?? "Not specified"}</p>
                        <p><strong>Pace:</strong> {typeof run.pace === "number" ? FormatPace(run.pace!) : "Not specified"}</p>
                        <p><strong>Duration:</strong> {typeof run.duration === "number" ? FormatDuration(run.duration!) : "Not specified"} min</p>
                        <p><strong>Distance:</strong> {run.distance != null ? `${run.distance} km` : "Not specified"}</p>
                        <p><strong>Notes:</strong> {run.notes ?? "No notes"}</p>
                        <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                            <strong>Completed?</strong>
                            <input
                                type="checkbox"
                                checked={run.completed}
                                onChange={(e) => handleComplete(e.target.checked)}
                                style={{ transform: "scale(1.3)", cursor: "pointer", accentColor: "#4CAF50" }}
                            />
                        </label>
                        <button
                            className="btn btn-primary mb-1 mt-3"
                            style={{ width: '20%' }}
                            onClick={() => setShowAddComment(true)}
                        >
                            Add Comment
                        </button>
                    </div>

                    <h3 className="mt-4 text-center">Feedback</h3>
                    {feedback ? (
                        <div className="row">
                            <div className="col-md-6 mb-3">
                                <div className="card text-light bg-secondary">
                                    <div className="card-body d-flex justify-content-between">
                                        <div>
                                            <p><strong>Effort: </strong> {feedback.effortRating ?? "Not specified"}/10</p>
                                            <p><strong>Feel: </strong> {feedback.feelRating ?? "Not specified"}/10</p>
                                            <p><strong>Actual Pace:</strong> {typeof feedback.pace === "number" ? FormatPace(feedback.pace!) : "Not specified"} min/km</p>
                                            <p><strong>Actual Duration:</strong> {typeof feedback.duration === "number" ? FormatDuration(feedback.duration!) : "Not specified"} min</p>
                                            <p><strong>Actual Distance: </strong> {feedback.distance ?? "Not specified"}</p>
                                            <p><strong>Comment: </strong> {feedback.comment ?? "Not specified"}</p>
                                        </div>
                                        <div className="d-flex flex-column align-items-center">
                                            <button
                                                onClick={() => setEditFeedback(feedback)}
                                                className="btn btn-link p-0 m-0 text-secondary mb-2"
                                                title="Edit"
                                            >
                                                <i className="fas fa-pen text-light"></i>
                                            </button>
                                            <button
                                                onClick={() => {
                                                    const confirmed = window.confirm("Are you sure you want to delete this feedback?");
                                                    if (confirmed) {
                                                        handleDelete(feedback.feedbackID, "feedback");
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
                            </div>
                        </div>
                    ) : (
                        <p className="text-center text-white">No feedback added.</p>
                    )}

                    {editFeedback && (
                        <div className="modal d-block">
                            <div className="modal-dialog">
                                <AddFeedbackForm
                                    runId={editFeedback.runID}
                                    initialData={feedback!}
                                    onSubmit={(feedback) => {
                                        setEditFeedback(null);
                                        setFeedback(feedback);
                                    }}
                                    onClose={() => setEditFeedback(null)}
                                />
                            </div>
                        </div>
                    )}

                    <h3 className="mt-4 text-center">Comments</h3>
                    {comment.length > 0 ? (
                        <div className="row">
                            {comment.map((comment) => (
                                <div key={comment.commentID} className="col-md 6 mb-3">
                                    <div className="card text-light bg-secondary">
                                        <div className="card-body d-flex justify-content-between">
                                            <div>
                                                <div key={comment.commentID}>
                                                    <p><strong>User:</strong> {usernames[comment.userID!] ?? "Loading..."}</p>
                                                </div>
                                                <p><strong>Comment: </strong> {comment.text ?? "Not specified"}</p>
                                                <p><strong></strong> {new Date(comment.createdAt).toLocaleString()}</p>
                                            </div>
                                            <div className="d-flex flex-column align-items-center">
                                                <button
                                                    onClick={() => setEditComment(comment)}
                                                    className="btn btn-link p-0 m-0 text-secondary mb-2"
                                                    title="Edit"
                                                >
                                                    <i className="fas fa-pen text-light"></i>
                                                </button>

                                                <button
                                                    onClick={() => {
                                                        const confirmed = window.confirm("Are you sure you want to delete this comment?");
                                                        if (confirmed) {
                                                            handleDelete(comment.commentID, "comment");
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
                                </div>
                            ))}
                        </div>
                    ) : (
                        <p className="text-center text-white">No comments added.</p>
                    )}

                    {editComment && (
                        <div className="modal d-block">
                            <div className="modal-dialog">
                                <AddCommentForm
                                    runId={editComment.runID || null}
                                    initialData={editComment}
                                    onSubmit={(updatedComment) => {
                                        setComment(prev => prev.map(c =>
                                            c.commentID === updatedComment.commentID ? updatedComment : c
                                        ));
                                        setEditComment(null);
                                    }}
                                    onClose={() => setEditComment(null)}
                                />
                            </div>
                        </div>
                    )}
                </div>
            </div>
            {showAddFeedback && feedbackRun && (
                <div className="modal">
                    <AddFeedbackForm
                        runId={feedbackRun.runID}
                        onSubmit={() => {
                            setShowAddFeedback(false);
                            completeItem(true);
                            setFeedbackRun(null);
                        }}
                        onClose={() => {
                            setShowAddFeedback(false);
                            completeItem(true);
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
                        completeItem(true);
                    }}
                />
            )}

            {showAddComment && (
                <div className="modal d-block">
                    <div className="modal-dialog">
                        <AddCommentForm
                            runId={run.runID}
                            onSubmit={handleNewComment}
                            onClose={() => setShowAddComment(false)}
                        />
                    </div>
                </div>
            )}
        </div>
    );
}