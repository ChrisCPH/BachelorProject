import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Workout } from "../types/Workout";
import { Exercise } from "../types/Exercise";
import { dayNames } from "../utils/DayNames";
import { handleAuthError } from "../utils/AuthError";
import { AddCommentForm } from "../components/AddCommentForm";
import { Comment } from "../types/Comment";
import FormatDuration from "../utils/FormatDuration";
import { AddExerciseForm } from "../components/AddExerciseForm";

export default function WorkoutDetails() {
    const { id } = useParams<{ id: string }>();
    const [workout, setWorkout] = useState<Workout | null>(null);
    const [exercises, setExercises] = useState<Exercise[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showAddExercise, setShowAddExercise] = useState(false);
    const [editExercise, setEditExercise] = useState<Exercise | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [comment, setComment] = useState<Comment[]>([]);
    const [showAddComment, setShowAddComment] = useState(false);
    const [usernames, setUsernames] = useState<{ [userId: number]: string }>({});
    const [editComment, setEditComment] = useState<Comment | null>(null);
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
                const [workoutRes, exerciseRes] = await Promise.all([
                    fetch(`${API_BASE_URL}/workout/${id}`, {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                    fetch(`${API_BASE_URL}/exercise/workout/${id}`, {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                ]);

                if (!workoutRes.ok) {
                    throw new Error("Failed to fetch workout");
                }
                const workoutData: Workout = await workoutRes.json();
                setWorkout(workoutData);

                let exerciseData: Exercise[] = [];

                if (exerciseRes.status === 200) {
                    exerciseData = await exerciseRes.json();
                } else if (exerciseRes.status === 204 || exerciseRes.status === 404) {
                    exerciseData = [];
                } else if (!exerciseRes.ok) {
                    throw new Error("Failed to fetch exercises");
                }
                setExercises(exerciseData);

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
        if (!token || !workout) {
            setError("Missing token or workout.");
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/workout/complete/${workout.workoutID}`, {
                method: "PATCH",
                headers: {
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(completed),
            });

            if (await handleAuthError(response, setErrorMessage, "completing workout")) return;
            if (!response.ok) {
                throw new Error(`Failed to update workout completion status. Status: ${response.status}`);
            }

            setWorkout({ ...workout, completed });
        } catch (err) {
            console.error(err);
            setError((err as Error).message);
        }
    };

    const handleDelete = async (id: number, type: "exercise" | "comment") => {
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

            if (await handleAuthError(response, setErrorMessage, `deleting exercise`)) return;
            if (!response.ok) {
                throw new Error("Failed to delete the exercise.");
            }

            if (type === "exercise") {
                setExercises((prevExercises) =>
                    prevExercises.filter((exercise) => exercise.exerciseID !== id)
                );
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

    if (loading) return <p>Loading workout details...</p>;
    if (error) return <p className="text-danger">{error}</p>;
    if (!workout) return <p>Workout not found.</p>;

    return (
        <div className="container mt-4 text-light">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <h2 className="text-center mb-4">Workout Details</h2>
            <div className="row justify-content-center">
                <div className="col-md-8">
                    <div className="card bg-secondary text-white p-4">
                        <p><strong>Type:</strong> {workout.type}</p>
                        <p><strong>Week:</strong> {workout.weekNumber}</p>
                        <p><strong>Day:</strong> {dayNames[workout.dayOfWeek]}</p>
                        <p><strong>Time:</strong> {workout.timeOfDay ?? "Not specified"}</p>
                        <p><strong>Duration:</strong> {typeof workout.duration === "number" ? FormatDuration(workout.duration!) : "Not specified"} min</p>
                        <p><strong>Notes:</strong> {workout.notes ?? "No notes"}</p>
                        <p><strong>Created At:</strong> {new Date(workout.createdAt).toLocaleString()}</p>
                        <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                            <strong>Completed?</strong>
                            <input
                                type="checkbox"
                                checked={workout.completed}
                                onChange={(e) => handleComplete(e.target.checked)}
                                style={{ transform: "scale(1.3)", cursor: "pointer", accentColor: "#4CAF50" }}
                            />
                        </label>
                        <button
                            className="btn btn-primary mb-1 mt-3"
                            style={{ width: '20%' }}
                            onClick={() => {
                                setShowAddExercise(true);
                            }}
                        >
                            Add Exercise
                        </button>
                        <button
                            className="btn btn-primary mb-1 mt-3"
                            style={{ width: '20%' }}
                            onClick={() => setShowAddComment(true)}
                        >
                            Add Comment
                        </button>
                    </div>

                    <h3 className="mt-4 text-center">Exercises</h3>
                    {exercises.length > 0 ? (
                        <div className="row">
                            {exercises.map((exercise) => (
                                <div key={exercise.exerciseID} className="col-md-6 mb-3">
                                    <div className="card text-light bg-secondary">
                                        <div className="card-body d-flex justify-content-between">
                                            <div>
                                                <h5 className="card-title">{exercise.name}</h5>
                                                <p><strong>Sets:</strong> {exercise.sets ?? "Not specified"}</p>
                                                <p><strong>Reps:</strong> {exercise.reps ?? "Not specified"}</p>
                                                <p><strong>Weight:</strong> {exercise.weight != null ? `${exercise.weight} kg` : "Not specified"}</p>
                                            </div>
                                            <div className="d-flex flex-column align-items-center">
                                                <button
                                                    onClick={() => setEditExercise(exercise)}
                                                    className="btn btn-link p-0 m-0 text-secondary mb-2"
                                                    title="Edit"
                                                >
                                                    <i className="fas fa-pen text-light"></i>
                                                </button>

                                                <button
                                                    onClick={() => {
                                                        const confirmed = window.confirm("Are you sure you want to delete this exercise?");
                                                        if (confirmed) {
                                                            handleDelete(exercise.exerciseID, "exercise");
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
                        <p className="text-center text-white">No exercises added yet. Click "Add Exercise" to get started!</p>
                    )}

                    {editExercise && (
                        <div className="modal d-block">
                            <div className="modal-dialog">
                                <AddExerciseForm
                                    workoutId={Number(id)}
                                    initialData={editExercise}
                                    onSubmit={(updatedExercise) => {
                                        setExercises(prev => prev.map(ex =>
                                            ex.exerciseID === updatedExercise.exerciseID ? updatedExercise : ex
                                        ));
                                    }}
                                    onClose={() => setEditExercise(null)}
                                />
                            </div>
                        </div>
                    )}

                    {showAddExercise && (
                        <div className="modal d-block">
                            <div className="modal-dialog">
                                <AddExerciseForm
                                    workoutId={Number(id)}
                                    onSubmit={(newExercise) => {
                                        setExercises(prev => [...prev, newExercise]);
                                        setShowAddExercise(false);
                                    }}
                                    onClose={() => setShowAddExercise(false)}
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
                                    workoutId={editComment.workoutID || null}
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

            {showAddComment && (
                <div className="modal d-block">
                    <div className="modal-dialog">
                        <AddCommentForm
                            workoutId={workout.workoutID}
                            onSubmit={handleNewComment}
                            onClose={() => setShowAddComment(false)}
                        />
                    </div>
                </div>
            )}
        </div>
    );
}
