import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useForm } from "react-hook-form";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { TrainingPlan } from "../types/TrainingPlan";
import { TrainingPlanFormData } from "../schemas/TrainingPlanSchema";
import { formatLocalDate, isSunday } from "../utils/FormatLocalDate";
import { TrainingPlanWithPermission } from "../types/TrainingPlanWithPermission";

export default function TrainingPlanOverview() {
	const [ownerPlans, setOwnerPlans] = useState<TrainingPlanWithPermission[]>([]);
	const [otherPlans, setOtherPlans] = useState<TrainingPlanWithPermission[]>([]);
	const [editMode, setEditMode] = useState<number | null>(null);
	const [editedPlan, setEditedPlan] = useState<TrainingPlan | null>(null);
	const [apiError, setApiError] = useState("");
	const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		setValue,
		watch
	} = useForm<TrainingPlanFormData>();

	const startDate = watch("startDate");

	useEffect(() => {
		fetchTrainingPlans();
	}, []);

	const fetchTrainingPlans = async () => {
		try {
			const response = await fetch(`${API_BASE_URL}/trainingplan/planswithpermission`, {
				method: "GET",
				headers: {
					"Content-Type": "application/json",
					Authorization: `Bearer ${localStorage.getItem("token")}`,
				},
			});
			if (!response.ok) throw new Error("Failed to fetch training plans");

			const data: TrainingPlanWithPermission[] = await response.json();
			setOwnerPlans(data.filter(plan => plan.permission === "owner"));
			setOtherPlans(data.filter(plan => plan.permission !== "owner"));
		} catch (error) {
			console.error(error);
		}
	};

	const onSubmit = async (data: TrainingPlanFormData) => {
		try {
			const planData: TrainingPlanFormData = {
				...data,
				createdAt: new Date().toISOString(),
				startDate: data.startDate ? formatLocalDate(data.startDate) : null,
				event: data.event || null,
				goalTime: data.goalTime || null,
			};

			const response = await fetch(`${API_BASE_URL}/trainingplan/add`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					Authorization: `Bearer ${localStorage.getItem("token")}`,
				},
				body: JSON.stringify(planData),
			});

			if (!response.ok) {
				const errorData = await response.json();
				throw new Error(errorData.message || "Failed to create plan");
			}

			reset();
			fetchTrainingPlans();
		} catch (error: any) {
			setApiError(error.message);
		}
	};

	const handleEditClick = (plan: TrainingPlanWithPermission) => {
		setEditMode(plan.trainingPlanID);
		setEditedPlan({
			...plan,
			startDate: plan.startDate || null
		});
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
					StartDate: editedPlan.startDate ? formatLocalDate(editedPlan.startDate) : null,
					Duration: editedPlan.duration,
					Event: editedPlan.event || "",
					GoalTime: editedPlan.goalTime || "",
				}),
			});

			if (response.ok) {
				fetchTrainingPlans();
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
				fetchTrainingPlans();
			} else {
				throw new Error("Failed to delete the training plan.");
			}
		} catch (error) {
			console.error(error);
		}
	};

	const renderPlanCard = (plan: TrainingPlanWithPermission) => {
		const canEdit = plan.permission === "owner" || plan.permission === "editor";
		const canDelete = plan.permission === "owner";

		return (
			<div key={plan.trainingPlanID} className="col-md-4 mb-4">
				<div className="card shadow-sm position-relative bg-secondary text-white">
					{(canEdit || canDelete) && (
						<div className="position-absolute top-0 end-0 p-2 d-flex gap-2">
							{canEdit && (
								<button
									onClick={() => handleEditClick(plan)}
									className="btn btn-link p-0 m-0 text-secondary"
								>
									<i className="fas fa-pen text-light"></i>
								</button>
							)}
							{canDelete && (
								<button
									onClick={() => handleDelete(plan.trainingPlanID)}
									className="btn btn-link p-0 m-0 text-secondary"
								>
									<i className="fas fa-trash text-light"></i>
								</button>
							)}
						</div>
					)}

					<div className="card-body">
						{editMode === plan.trainingPlanID ? (
							<>
								<div className="mb-1">
									<label className="form-label">Plan Name</label>
									<input
										type="text"
										className="form-control mb-2"
										value={editedPlan?.name}
										onChange={(e) => setEditedPlan({ ...editedPlan!, name: e.target.value })}
									/>
								</div>
								<div className="mb-1">
									<label className="form-label">Start Date</label>
									< br />
									<DatePicker
										selected={editedPlan?.startDate ? new Date(editedPlan.startDate) : null}
										onChange={(date: Date | null) => setEditedPlan({ ...editedPlan!, startDate: date ? formatLocalDate(date.toISOString()) : null })}
										filterDate={isSunday}
										className="form-control mb-2"
										dateFormat="yyyy-MM-dd"
										placeholderText="Select a Sunday"
									/>
								</div>
								<div className="mb-1">
									<label className="form-label">Duration (weeks)</label>
									<input
										type="number"
										className="form-control mb-2"
										value={editedPlan?.duration}
										max={52}
										onChange={(e) => setEditedPlan({ ...editedPlan!, duration: Number(e.target.value) })}
									/>
								</div>
								<div className="mb-1">
									<label className="form-label">Event (optional)</label>
									<input
										type="text"
										className="form-control mb-2"
										value={editedPlan?.event || ""}
										onChange={(e) => setEditedPlan({ ...editedPlan!, event: e.target.value })}
									/>
								</div>
								<div className="mb-1">
									<label className="form-label">Goal Time (optional)</label>
									<input
										type="text"
										className="form-control mb-2"
										value={editedPlan?.goalTime || ""}
										onChange={(e) => setEditedPlan({ ...editedPlan!, goalTime: e.target.value })}
									/>
									<button onClick={handleSave} className="btn btn-primary w-100 mt-2">
										Save Changes
									</button>
									<button onClick={() => setEditMode(null)} className="btn btn-info w-100 mt-2">
										Cancel
									</button>
								</div>
							</>
						) : (
							<>
								<h5 className="card-title">{plan.name}</h5>
								<p className="card-text"><strong>Start Date:</strong> {plan.startDate ? new Date(plan.startDate).toLocaleDateString() : "Not set"}</p>
								<p className="card-text"><strong>Duration:</strong> {plan.duration} weeks</p>
								<p className="card-text"><strong>Event:</strong> {plan.event}</p>
								<p className="card-text"><strong>Goal Time:</strong> {plan.goalTime}</p>
								<Link to={`/training-plan/${plan.trainingPlanID}`} className="btn btn-primary w-100">
									View Details
								</Link>
							</>
						)}
					</div>
				</div>
			</div>
		);
	};

	return (
		<div className="container mt-5 text-white">
			<h2 className="text-center mb-4">Training Plans Overview</h2>

			{/* Owner plans section */}
			{ownerPlans.length > 0 && (
				<div className="mb-5">
					<h4>My Plans</h4>
					<div className="row">{ownerPlans.map(renderPlanCard)}</div>
				</div>
			)}

			{/* Other plans section */}
			{otherPlans.length > 0 && (
				<div className="mb-5">
					<h4>Shared Plans</h4>
					<div className="row">{otherPlans.map(renderPlanCard)}</div>
				</div>
			)}

			{/* Create new plan form */}
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
						<label className="form-label">Start Date (optional)</label>
						<br />
						<DatePicker
							selected={startDate ? new Date(startDate) : null}
							onChange={(date: Date | null) => {
								setValue("startDate", date ? formatLocalDate(date.toISOString()) : "");
							}}
							filterDate={isSunday}
							className="form-control"
							dateFormat="yyyy-MM-dd"
							placeholderText="Select a Sunday"
						/>
						{errors.startDate && <div className="text-danger">Start date is required</div>}
					</div>
					<div className="mb-3">
						<label className="form-label">Duration (weeks)</label>
						<input type="number" {...register("duration", { required: true })} className="form-control" max={52} />
						{errors.duration && <div className="text-danger">Duration is required</div>}
					</div>
					<div className="mb-3">
						<label className="form-label">Event (optional)</label>
						<input type="text" {...register("event")} className="form-control" />
					</div>
					<div className="mb-3">
						<label className="form-label">Goal Time (optional)</label>
						<input type="text" {...register("goalTime")} className="form-control" />
					</div>
					<button type="submit" className="btn btn-primary w-100">
						Create Plan
					</button>
				</form>
			</div>
		</div>
	);
}
