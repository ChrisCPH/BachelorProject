import { useForm } from "react-hook-form";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { TrainingPlanFormData } from "../schemas/TrainingPlanSchema";

export default function CreateTrainingPlan() {
  const { register, handleSubmit, formState: { errors } } = useForm<TrainingPlanFormData>();
  const [apiError, setApiError] = useState("");
  const navigate = useNavigate();
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

  const onSubmit = async (data: TrainingPlanFormData) => {
    try {
      const dataWithTimestamp: TrainingPlanFormData = {
        ...data,
        createdAt: new Date().toISOString(),
        startDate: data.startDate === "" ? null : data.startDate,
        event: data.event === "" ? null : data.event,
        goalTime: data.goalTime === "" ? null : data.goalTime,
      };

      const response = await fetch(`${API_BASE_URL}/trainingplan/add`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify(dataWithTimestamp),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Failed to create plan");
      }

      navigate("/overview");
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
          <input type="date" {...register("startDate")} className="form-control" />
        </div>

        <div className="mb-3">
          <label className="form-label">Duration (weeks)</label>
          <input type="number" {...register("duration", { required: true, min: 1 })} className="form-control" />
          {errors.duration && <div className="text-danger">Please enter a valid duration</div>}
        </div>

        <div className="mb-3">
          <label className="form-label">Event</label>
          <input {...register("event")} className="form-control" />
        </div>

        <div className="mb-3">
          <label className="form-label">Goal Time</label>
          <input {...register("goalTime")} className="form-control" />
        </div>

        <button type="submit" className="btn btn-primary">Create Plan</button>
      </form>
    </div>
  );
}
