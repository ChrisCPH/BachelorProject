import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useForm } from "react-hook-form";
import { TrainingPlan } from "../types/TrainingPlan";
import { TrainingPlanFormData } from "../schemas/TrainingPlanSchema";

export default function TrainingPlanOverview() {
  const [trainingPlans, setTrainingPlans] = useState<TrainingPlan[]>([]);
  const [editMode, setEditMode] = useState<number | null>(null);
  const [editedPlan, setEditedPlan] = useState<TrainingPlan | null>(null);
  const [apiError, setApiError] = useState("");
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

  const { register, handleSubmit, formState: { errors }, reset } = useForm<TrainingPlanFormData>();

  useEffect(() => {
    fetchTrainingPlans();
  }, []);

  const fetchTrainingPlans = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/trainingplan/user`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      });
      if (response.ok) {
        const data = await response.json();
        setTrainingPlans(data);
      } else {
        throw new Error("Failed to fetch training plans");
      }
    } catch (error) {
      console.error(error);
    }
  };

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

      reset(); // clear form
      fetchTrainingPlans(); // refresh list
    } catch (error: any) {
      setApiError(error.message);
    }
  };

  const handleEditClick = (plan: TrainingPlan) => {
    setEditMode(plan.trainingPlanID);
    setEditedPlan({ ...plan });
  };

  const handleSave = async () => {
    if (!editedPlan) return;

    try {
      const response = await fetch(`${API_BASE_URL}/trainingplan/update`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          TrainingPlanID: editedPlan.trainingPlanID,
          Name: editedPlan.name,
          StartDate: editedPlan.startDate || null,
          Duration: editedPlan.duration,
          Event: editedPlan.event || "",
          GoalTime: editedPlan.goalTime || "",
          CreatedAt: new Date().toISOString(),
        }),
      });

      if (response.ok) {
        setTrainingPlans(prevPlans =>
          prevPlans.map(plan =>
            plan.trainingPlanID === editedPlan.trainingPlanID ? editedPlan : plan
          )
        );
        setEditMode(null);
        setEditedPlan(null);
      } else {
        throw new Error("Failed to update the training plan");
      }
    } catch (error) {
      console.error("Error updating training plan:", error);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("Are you sure you want to delete this training plan?")) return;

    try {
      const response = await fetch(`${API_BASE_URL}/trainingplan/delete/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      });

      if (response.ok) {
        setTrainingPlans(prev => prev.filter(plan => plan.trainingPlanID !== id));
      } else {
        throw new Error("Failed to delete the training plan.");
      }
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <div className="container mt-5 text-white">
      <h2 className="text-center mb-4">Training Plans Overview</h2>

      <div className="row">
        {trainingPlans.length > 0 ? (
          trainingPlans.map((plan) => (
            <div key={plan.trainingPlanID} className="col-md-4 mb-4">
              <div className="card shadow-sm position-relative bg-secondary text-white">
                <div className="position-absolute top-0 end-0 p-2 d-flex gap-2">
                  <button
                    onClick={() => handleEditClick(plan)}
                    className="btn btn-link p-0 m-0 text-secondary"
                  >
                    <i className="fas fa-pen text-light"></i>
                  </button>
                  <button
                    onClick={() => handleDelete(plan.trainingPlanID)}
                    className="btn btn-link p-0 m-0 text-secondary"
                  >
                    <i className="fas fa-trash text-light"></i>
                  </button>
                </div>

                <div className="card-body">
                  {editMode === plan.trainingPlanID ? (
                    <div>
                      <div className="form-group">
                        <label htmlFor="name">Plan Name</label>
                        <input
                          type="text"
                          id="name"
                          className="form-control"
                          value={editedPlan?.name}
                          onChange={(e) => setEditedPlan({ ...editedPlan!, name: e.target.value })}
                        />
                      </div>
                      <div className="form-group">
                        <label htmlFor="startDate">Start Date</label>
                        <input
                          type="date"
                          id="startDate"
                          className="form-control"
                          value={editedPlan?.startDate || ''}
                          onChange={(e) => setEditedPlan({ ...editedPlan!, startDate: e.target.value })}
                        />
                      </div>
                      <div className="form-group">
                        <label htmlFor="duration">Duration (weeks)</label>
                        <input
                          type="number"
                          id="duration"
                          className="form-control"
                          value={editedPlan?.duration}
                          onChange={(e) => setEditedPlan({ ...editedPlan!, duration: Number(e.target.value) })}
                        />
                      </div>
                      <div className="form-group">
                        <label htmlFor="event">Event</label>
                        <input
                          type="text"
                          id="event"
                          className="form-control"
                          value={editedPlan?.event || ''}
                          onChange={(e) => setEditedPlan({ ...editedPlan!, event: e.target.value })}
                        />
                      </div>
                      <div className="form-group">
                        <label htmlFor="goalTime">Goal Time</label>
                        <input
                          type="text"
                          id="goalTime"
                          className="form-control"
                          value={editedPlan?.goalTime || ''}
                          onChange={(e) => setEditedPlan({ ...editedPlan!, goalTime: e.target.value })}
                        />
                      </div>

                      <button onClick={handleSave} className="btn btn-success w-100 mt-3">
                        Save Changes
                      </button>
                    </div>
                  ) : (
                    <>
                      <h5 className="card-title">{plan.name}</h5>
                      <p className="card-text">
                        <strong>Start Date:</strong>{" "}
                        {plan.startDate ? new Date(plan.startDate).toLocaleDateString() : "Not set"}
                      </p>
                      <p className="card-text">
                        <strong>Duration:</strong> {plan.duration} weeks
                      </p>
                      <p className="card-text">
                        <strong>Event:</strong> {plan.event}
                      </p>
                      <p className="card-text">
                        <strong>Goal Time:</strong> {plan.goalTime}
                      </p>
                      <Link to={`/training-plan/${plan.trainingPlanID}`} className="btn btn-primary w-100">
                        View Details
                      </Link>
                    </>
                  )}
                </div>
              </div>
            </div>
          ))
        ) : (
          <p>No training plans available.</p>
        )}
      </div>
      <div className="card mb-5 bg-dark text-white p-4 shadow">
        <h4>Create New Running Plan</h4>
        {apiError && <div className="alert alert-danger">{apiError}</div>}
        <form onSubmit={handleSubmit(onSubmit)}>
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
    </div>
  );
}
