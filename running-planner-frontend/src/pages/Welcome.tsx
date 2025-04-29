import { Link } from "react-router-dom";

export default function Welcome() {
  return (
    <div className="d-flex flex-column align-items-center justify-content-center min-vh-100 bg-dark">
      <h1 className="display-4 text-primary mb-4">Welcome to the Running Planner</h1>
      <p className="lead mb-5">Choose an option to get started</p>
      <div className="d-flex gap-3">
        <Link to="/login">
          <button className="btn btn-primary btn-lg px-4 py-2 shadow-sm">
            Login
          </button>
        </Link>
        <Link to="/register">
          <button className="btn btn-success btn-lg px-4 py-2 shadow-sm">
            Register
          </button>
        </Link>
      </div>
    </div>
  );
}
