import { Outlet } from "react-router-dom";
import AppNavbar from "./Navbar";

export default function ProtectedLayout() {
  return (
    <>
      <AppNavbar />
      <div className="container mt-4">
        <Outlet />
      </div>
    </>
  );
}
