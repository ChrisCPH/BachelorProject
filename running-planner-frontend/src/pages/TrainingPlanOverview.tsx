import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { TrainingPlan } from "../types/TrainingPlan";

export default function TrainingPlanOverview() {
  const [trainingPlans, setTrainingPlans] = useState<TrainingPlan[]>([]);

  useEffect(() => {
    const fetchTrainingPlans = async () => {
      try {
        const response = await fetch("http://localhost:5015/api/trainingPlans");
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

    fetchTrainingPlans();
  }, []);

  return (
    <div className="container mt-5 text-light">
      <h2 className="text-center mb-4">Training Plans Overview</h2>
      <div className="row">
        {trainingPlans.map((plan) => (
          <div key={plan.trainingPlanId} className="col-md-4 mb-4">
            <div className="card shadow-sm">
              <div className="card-body">
                <h5 className="card-title">{plan.name}</h5>
                <p className="card-text">
                  <strong>Event:</strong> {plan.event}
                </p>
                <p className="card-text">
                  <strong>Start Date:</strong> {new Date(plan.startDate).toLocaleDateString()}
                </p>
                <p className="card-text">
                  <strong>Duration:</strong> {plan.duration} weeks
                </p>
                <p className="card-text">
                  <strong>Goal Time:</strong> {plan.goalTime}
                </p>
                <Link to={`/trainingPlan/${plan.trainingPlanId}`} className="btn btn-primary w-100">
                  View Details
                </Link>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
