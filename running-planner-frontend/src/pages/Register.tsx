import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { Link } from "react-router-dom";
import { RegisterSchema, RegisterFormData } from "../schemas/RegisterSchema";

export default function Register() {
    const { register, handleSubmit, formState: { errors } } = useForm<RegisterFormData>({ resolver: zodResolver(RegisterSchema) });
    const navigate = useNavigate();
    const [apiError, setApiError] = useState("");
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    const onSubmit = async (data: RegisterFormData) => {
        try {
            const response = await fetch(`${API_BASE_URL}/api/user/register`, {
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

            navigate("/login");
        } catch (error: any) {
            setApiError(error.message);
        }
    };

    return (
        <div className="container d-flex justify-content-center align-items-start mt-5 pt-5 bg-dark text-white">
            <div className="col-lg-6 col-md-8 col-sm-10">
                <form onSubmit={handleSubmit(onSubmit)} className="form-container">
                    <h2 className="text-center mb-4">Register</h2>

                    {apiError && <div className="alert alert-danger">{apiError}</div>}

                    <div className="mb-3">
                        <label htmlFor="userName" className="form-label">User Name</label>
                        <input
                            {...register("userName")}
                            id="userName"
                            type="text"
                            placeholder="Enter your user name"
                            className="form-control"
                        />
                        {errors.userName && <div className="text-danger mt-1">{errors.userName.message}</div>}
                    </div>

                    <div className="mb-3">
                        <label htmlFor="email" className="form-label">Email</label>
                        <input
                            {...register("email")}
                            id="email"
                            type="email"
                            placeholder="Enter your email"
                            className="form-control"
                        />
                        {errors.email && <div className="text-danger mt-1">{errors.email.message}</div>}
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
                        {errors.password && <div className="text-danger mt-1">{errors.password.message}</div>}
                    </div>

                    <div className="mb-3">
                        <label htmlFor="confirmPassword" className="form-label">Confirm Password</label>
                        <input
                            {...register("confirmPassword")}
                            id="confirmPassword"
                            type="password"
                            placeholder="Confirm your password"
                            className="form-control"
                        />
                        {errors.confirmPassword && <div className="text-danger mt-1">{errors.confirmPassword.message}</div>}
                    </div>

                    <div className="mb-3">
                        <label htmlFor="preferredDistance" className="form-label">Preferred Distance Unit</label>
                        <select
                            {...register("preferredDistance")}
                            id="preferredDistance"
                            className="form-control"
                        >
                            <option value="">Select a unit</option>
                            <option value="km">Kilometers</option>
                            <option value="mi">Miles</option>
                        </select>
                        {errors.preferredDistance && <div className="text-danger mt-1">{errors.preferredDistance.message}</div>}
                    </div>

                    <div className="mb-3">
                        <label htmlFor="preferredWeight" className="form-label">Preferred Weight Unit</label>
                        <select
                            {...register("preferredWeight")}
                            id="preferredWeight"
                            className="form-control"
                        >
                            <option value="">Select a unit</option>
                            <option value="kg">Kilograms</option>
                            <option value="lbs">Pounds</option>
                        </select>
                        {errors.preferredWeight && <div className="text-danger mt-1">{errors.preferredWeight.message}</div>}
                    </div>

                    <button type="submit" className="btn btn-primary w-100">Register</button>

                    <div className="mt-3">
                        <Link to="/" className="btn btn-secondary w-100">Back</Link>
                    </div>
                </form>
            </div>
        </div>
    );
}
