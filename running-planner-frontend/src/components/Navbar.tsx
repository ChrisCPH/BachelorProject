import { useNavigate, NavLink } from "react-router-dom";
import { useAuth } from "../authentication/AuthContext";

export default function Navbar() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container-fluid d-flex justify-content-between align-items-center">

        <NavLink className="navbar-brand" to="/overview">Running Planner</NavLink>

        <div className="position-absolute start-50 translate-middle-x d-flex gap-4">
          <NavLink
            to="/overview"
            className={({ isActive }) => `nav-link text-white ${isActive ? "border-bottom border-primary" : ""}`}
          >
            Overview
          </NavLink>
          <NavLink
            to="/createPlan"
            className={({ isActive }) => `nav-link text-white ${isActive ? "border-bottom border-primary" : ""}`}
          >
            Create Plan
          </NavLink>
        </div>

        <div>
          <button onClick={handleLogout} className="btn btn-primary">Log Out</button>
        </div>

      </div>
    </nav>
  );
}
