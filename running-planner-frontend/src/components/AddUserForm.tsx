import { useState } from "react";
import { handleAuthError } from "../utils/AuthError";

interface AddUserFormProps {
    trainingPlanId: number;
    onSubmit: () => void;
    onClose: () => void;
}

interface UserSearchResult {
    userId: number;
    username: string;
}

const permissionTypes = ["editor", "commenter", "viewer"];

export const AddUserForm = ({ trainingPlanId, onSubmit, onClose }: AddUserFormProps) => {
    const [username, setUsername] = useState("");
    const [permission, setPermission] = useState<"editor" | "commenter" | "viewer">("viewer");
    const [error, setError] = useState("");
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [isSearching, setIsSearching] = useState(false);
    const [userResult, setUserResult] = useState<UserSearchResult | null>(null);
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    const handleSearch = async () => {
        if (!username.trim()) {
            setError("Please enter a username");
            return;
        }

        setIsSearching(true);
        setError("");
        setUserResult(null);

        try {
            const response = await fetch(`${API_BASE_URL}/user/get/${encodeURIComponent(username.trim())}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    setError("User not found");
                } else {
                    throw new Error("Failed to search for user");
                }
                return;
            }

            const userData = await response.json();
            setUserResult({
                userId: userData.userID,
                username: userData.userName,
            });
        } catch (error) {
            console.error(error);
            setError("Failed to search for user");
        } finally {
            setIsSearching(false);
        }
    };

    const handleSubmit = async () => {
        if (!userResult) {
            setError("Please search for a user first");
            return;
        }

        if (!permission) {
            setError("Permission level is required.");
            return;
        }

        setError("");
        setErrorMessage(null);

        try {
            const url = `${API_BASE_URL}/user/addUserToTrainingPlan?userId=${userResult.userId}&trainingPlanId=${trainingPlanId}&permission=${permission}`;

            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                },
            });

            if (await handleAuthError(response, setErrorMessage, `adding user to training plan`)) return;
            if (!response.ok) {
                if (response.status === 400) {
                    try {
                        const errorData = await response.json();
                        setError(errorData.message || "User already has this permission");
                    } catch {
                        setError("User already has this permission");
                    }
                } else {
                    throw new Error(`Failed to add user. Status: ${response.status}`);
                }
                return;
            }

            setError("");
            setErrorMessage(null);
            setSuccessMessage(`${userResult.username} added successfully as ${permission}!`);
            setTimeout(() => {
                setSuccessMessage(null);
                onSubmit();
            }, 4000);
        } catch (error) {
            console.error(error);
            setError("Failed to add user to training plan");
        }
    };

    return (
        <div className="modal-content bg-dark text-white p-4 rounded-3 shadow-lg">
            <div>
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            <div className="modal-header border-bottom-0">
                <h5>Add User to Training Plan</h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
            </div>

            {error && <div className="alert alert-danger">{error}</div>}
            {successMessage && <div className="alert alert-success">{successMessage}</div>}

            <div className="form-group mb-3">
                <label>Username</label>
                <div className="input-group">
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        className="form-control bg-secondary text-white border-0 rounded-2"
                        placeholder="Enter username to search"
                        onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                    />
                    <button
                        className="btn btn-primary"
                        onClick={handleSearch}
                        disabled={isSearching}
                    >
                        {isSearching ? "Searching..." : "Search"}
                    </button>
                </div>
            </div>

            {userResult && (
                <div className="mb-1 p-3 bg-secondary rounded-2">
                    <p>User found: {userResult.username}</p>
                </div>
            )}

            <div className="form-group mb-3">
                <label>Permission Level</label>
                <select
                    value={permission}
                    onChange={(e) => setPermission(e.target.value as "editor" | "commenter" | "viewer")}
                    className="form-control bg-secondary text-white border-0 rounded-2"
                    disabled={!userResult}
                >
                    {permissionTypes.map((perm, index) => (
                        <option key={index} value={perm}>
                            {perm.charAt(0).toUpperCase() + perm.slice(1)}
                        </option>
                    ))}
                </select>
            </div>

            <div className="d-flex justify-content-between">
                <button
                    onClick={handleSubmit}
                    className="btn btn-primary w-100 rounded-2 py-2"
                    disabled={!userResult || isSearching}
                >
                    {isSearching ? "Processing..." : "Add User"}
                </button>
            </div>
        </div>
    );
};