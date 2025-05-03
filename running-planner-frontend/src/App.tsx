import { Routes, Route } from "react-router-dom";
import Login from "./pages/Login.tsx";
import Register from "./pages/Register.tsx";
import Landing from "./pages/Welcome.tsx";
import TrainingPlanOverview from "./pages/TrainingPlanOverview.tsx";
import RequireAuth from "./authentication/RequireAuth.tsx";
import ProtectedLayout from "./components/ProtectedLayout.tsx";
import 'bootstrap/dist/css/bootstrap.min.css';
import '@fortawesome/fontawesome-free/css/all.min.css';
import TrainingPlanDetails from "./pages/TrainingPlanDetails.tsx";
import './App.css';

function App() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      <Route element={<RequireAuth><ProtectedLayout /></RequireAuth>}>
        <Route path="/overview" element={<TrainingPlanOverview />} />
        <Route path="/training-plan/:id" element={<TrainingPlanDetails />} />
      </Route>
    </Routes>
  );
}

export default App;
