import { useForm } from "react-hook-form";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { TrainingPlanFormData } from "../schemas/TrainingPlanSchema";

export default function CreateTrainingPlan() {
  const { register, handleSubmit, formState: { errors } } = useForm<TrainingPlanFormData>();
  const [apiError, setApiError] = useState("");
  const navigate = useNavigate();

  const onSubmit = async (data: TrainingPlanFormData) => {
    try {
      const response = await fetch("http://localhost:5015/api/runningplan/create", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Failed to create plan");
      }

      navigate("/dashboard");
    } catch (error: any) {
      setApiError(error.message);
    }
  };

  return (
    <div className="container mt-5 text-white">
      <h2>Create Running Plan</h2>
      {apiError && <div className="alert alert-danger">{apiError}</div>}
      <form onSubmit={handleSubmit(onSubmit)} className="mt-4">

        <div className="mb-3">
          <label className="form-label">Plan Name</label>
          <input {...register("name", { required: true })} className="form-control" />
          {errors.name && <div className="text-danger">Name is required</div>}
        </div>

        <div className="mb-3">
          <label className="form-label">Start Date</label>
          <input type="date" {...register("startDate", { required: true })} className="form-control" />
          {errors.startDate && <div className="text-danger">Start date is required</div>}
        </div>

        <div className="mb-3">
          <label className="form-label">Duration (weeks)</label>
          <input type="number" {...register("duration", { required: true, min: 1 })} className="form-control" />
          {errors.duration && <div className="text-danger">Please enter a valid duration</div>}
        </div>

        <div className="mb-3">
          <label className="form-label">Event</label>
          <input {...register("event", { required: false })} className="form-control" />
        </div>

        <div className="mb-3">
          <label className="form-label">Goal Time</label>
          <input {...register("goalTime", { required: false })} className="form-control" />
        </div>

        <button type="submit" className="btn btn-primary">Create Plan</button>
      </form>
    </div>
  );
}
