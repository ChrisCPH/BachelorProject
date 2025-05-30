import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { JSX } from "react";

export default function RequireAuth({ children }: { children: JSX.Element }) {
    const { isLoggedIn } = useAuth();
    const location = useLocation();

    if (!isLoggedIn) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    return children;
}
