import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { Link } from "react-router-dom";
import { LoginSchema, LoginFormData } from "../schemas/LoginSchema";
import { useAuth } from "../authentication/AuthContext";

export default function Login() {
    const { register, handleSubmit, formState: { errors }, } = useForm<LoginFormData>({ resolver: zodResolver(LoginSchema) });
    const navigate = useNavigate();
    const [apiError, setApiError] = useState("");
    const { login } = useAuth();
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    const onSubmit = async (data: LoginFormData) => {
        try {
            const response = await fetch(`${API_BASE_URL}/user/login`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data),
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => {
                    throw new Error("Unexpected error format");
                });
                throw new Error(errorData.message || "Login failed");
            }

            const result = await response.json();
            login(result.token);
            navigate("/overview");
        } catch (error: any) {
            setApiError(error.message);
        }
    };

    return (
        <div className="container d-flex justify-content-center align-items-start mt-5 pt-5 bg-dark text-white">
            <div className="col-lg-6 col-md-8 col-sm-10">
                <form onSubmit={handleSubmit(onSubmit)} className="form-container">
                    <h2 className="form-heading text-center">Login</h2>

                    {apiError && <div className="alert alert-danger">{apiError}</div>}

                    <div className="mb-3">
                        <label htmlFor="email" className="form-label">Email</label>
                        <input
                            {...register("email")}
                            id="email"
                            type="email"
                            placeholder="Enter your email"
                            className="form-control"
                        />
                        {errors.email && <div className="form-error">{errors.email.message}</div>}
                    </div>

                    <div className="mb-3">
                        <label htmlFor="password" className="form-label">Password</label>
                        <input
                            {...register("password")}
                            id="password"
                            type="password"
                            placeholder="Enter your password"
                            className="form-control"
                        />
                        {errors.password && <div className="form-error">{errors.password.message}</div>}
                    </div>

                    <button type="submit" className="btn btn-primary w-100">Log In</button>

                    <div className="mt-3">
                        <Link to="/" className="btn btn-secondary w-100">Back</Link>
                    </div>
                </form>
            </div>
        </div>
    );
}
