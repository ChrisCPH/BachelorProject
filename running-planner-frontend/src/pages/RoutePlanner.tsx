import { MapContainer, TileLayer, Polyline, useMap } from 'react-leaflet';
import { useEffect, useState } from 'react';
import { RouteMap } from '../components/RouteMap';
import 'leaflet/dist/leaflet.css';
import 'leaflet-draw/dist/leaflet.draw.css';
import { RunningRoute, GeoJsonLineString, Route } from '../types/RunningRoute';
import { useSearchParams } from 'react-router-dom';

function FlyToRoute({ coordinates }: { coordinates: [number, number][] }) {
    const map = useMap();

    useEffect(() => {
        if (coordinates.length > 0) {
            const bounds = coordinates.map(([lng, lat]) => [lat, lng]);
            map.fitBounds(bounds as [number, number][]);
        }
    }, [coordinates]);

    return null;
}

export default function RoutePlanner() {
    const [route, setRoute] = useState<Route | null>(null);
    const [routeName, setRouteName] = useState('');
    const [saving, setSaving] = useState(false);
    const [message, setMessage] = useState<string | null>(null);
    const [savedRoutes, setSavedRoutes] = useState<RunningRoute[]>([]);
    const [selectedRouteCoords, setSelectedRouteCoords] = useState<[number, number][]>([]);
    const [editingRoute, setEditingRoute] = useState<RunningRoute | null>(null);
    const [searchParams] = useSearchParams();
    const selectedRouteId = searchParams.get('routeId');
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    const fetchAndSelectRoute = async () => {
        try {
            const res = await fetch(`${API_BASE_URL}/runningroute/getAll`, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
            });

            if (!res.ok) throw new Error(await res.text());

            const data = await res.json();
            setSavedRoutes(data);

            if (selectedRouteId) {
                const match = data.find((r: RunningRoute) => r.id === selectedRouteId);
                if (match) {
                    setEditingRoute(match);
                    setRouteName(match.name);
                    setSelectedRouteCoords([]);
                    setRoute(null);
                }
            }
        } catch (err: any) {
            setMessage(`Failed to fetch routes: ${err.message}`);
        }
    };

    useEffect(() => {
        fetchAndSelectRoute();
    }, [selectedRouteId]);

    const saveRunningRoute = async (route: Route) => {
        const geometry: GeoJsonLineString = {
            type: 'LineString',
            coordinates: route.path.map((p) => [p.longitude, p.latitude]),
        };

        const newRoute: RunningRoute = {
            id: '',
            name: routeName || `Route - ${route.distanceKm.toFixed(2)} km`,
            geometry,
            distanceKm: route.distanceKm,
        };

        try {
            setSaving(true);
            setMessage(null);

            const response = await fetch(`${API_BASE_URL}/runningroute/add`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
                body: JSON.stringify(newRoute),
            });

            if (!response.ok) throw new Error(await response.text());

            setMessage('Route saved successfully!');
            setTimeout(() => setMessage(null), 4000);
            setRoute(null);
            setRouteName('');
            fetchAndSelectRoute();
        } catch (err: any) {
            setMessage(`Failed to save: ${err.message}`);
        } finally {
            setSaving(false);
        }
    }

    const deleteRunningRoute = async (routeId: string) => {
        const confirmed = window.confirm('Are you sure you want to delete this route?');
        if (!confirmed) return;

        try {
            const res = await fetch(`${API_BASE_URL}/runningroute/delete/${routeId}`, {
                method: 'DELETE',
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
            });

            if (!res.ok) throw new Error(await res.text());

            setMessage('Route deleted successfully!');
            setTimeout(() => setMessage(null), 4000);
            resetMapState();
            fetchAndSelectRoute();
        } catch (err: any) {
            setMessage(`Failed to delete route: ${err.message}`);
        }
    }

    const updateRunningRoute = async (route: RunningRoute) => {
        try {
            setSaving(true);
            setMessage(null);

            const response = await fetch(`${API_BASE_URL}/runningroute/update`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
                body: JSON.stringify(route),
            });

            if (!response.ok) throw new Error(await response.text());

            setMessage('Route updated successfully!');
            setTimeout(() => setMessage(null), 4000);

            setSavedRoutes(prev => prev.map(r =>
                r.id === route.id ? route : r
            ));

            resetMapState();
        } catch (err: any) {
            setMessage(`Failed to update: ${err.message}`);
        } finally {
            setSaving(false);
        }
    }

    const handleRouteCreated = (newRoute: Route) => {
        setRoute(newRoute);
        setSelectedRouteCoords(newRoute.path.map(p => [p.longitude, p.latitude]));
    };

    const resetMapState = () => {
        setSelectedRouteCoords([]);
        setEditingRoute(null);
        setRoute(null);
        setRouteName('');
    };

    return (
        <div style={{ height: '100vh', display: 'flex' }}>
            <div style={{ width: '300px', padding: '10px', backgroundColor: '#f7f7f7', overflowY: 'auto' }}>
                <h3>Routes</h3>
                <button
                    onClick={() => {
                        setSelectedRouteCoords([]);
                        setEditingRoute(null);
                        setRoute(null);
                        setRouteName('');
                    }}
                    style={{
                        marginBottom: '5px',
                        backgroundColor: '#f0f0f0',
                        border: '1px solid #ccc',
                        borderRadius: '4px',
                        cursor: 'pointer'
                    }}
                >
                    Clear Map
                </button>
                {savedRoutes.map((r) => (
                    <div
                        key={r.id}
                        style={{
                            marginBottom: '10px',
                            cursor: 'pointer',
                            display: 'flex',
                            justifyContent: 'space-between',
                            backgroundColor: editingRoute?.id === r.id ? '#e6f7ff' : 'transparent'
                        }}
                        onClick={() => {
                            setEditingRoute(r);
                            setRouteName(r.name);
                            setSelectedRouteCoords([]);
                            setRoute(null);
                        }}
                    >
                        <div>
                            <strong>{r.name}</strong><br />
                            <small>{r.distanceKm?.toFixed(2)} km</small>
                        </div>
                        <div>
                            <button
                                onClick={(e) => {
                                    e.stopPropagation();
                                    deleteRunningRoute(r.id);
                                }}
                                className="btn btn-link p-0 m-0 text-secondary"
                            >
                                <i className="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                ))}

            </div>

            <div style={{ flex: 1, position: 'relative' }}>
                <MapContainer
                    center={[55.6761, 12.5683]} // CPH
                    zoom={13}
                    style={{ height: '100%', width: '100%' }}>
                    <TileLayer
                        attribution="&copy; OpenStreetMap contributors"
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />

                    <RouteMap
                        onRouteCreated={handleRouteCreated}
                        existingRoute={editingRoute ? {
                            path: editingRoute.geometry.coordinates.map(([lng, lat]) => ({ latitude: lat, longitude: lng })),
                            distanceKm: editingRoute.distanceKm || 0
                        } : null}
                    />

                    {selectedRouteCoords.length > 0 && (
                        <>
                            <Polyline positions={selectedRouteCoords.map(([lng, lat]) => [lat, lng])} color="green" />
                            <FlyToRoute coordinates={selectedRouteCoords} />
                        </>
                    )}
                </MapContainer>

                {route && (
                    <div
                        style={{
                            position: 'absolute',
                            top: 10,
                            left: 320,
                            backgroundColor: 'white',
                            padding: '10px',
                            borderRadius: '8px',
                            boxShadow: '0 0 10px rgba(0,0,0,0.1)',
                            zIndex: 1000,
                        }}
                    >
                        <label>
                            <strong>Route Name:</strong>
                            <input
                                type="text"
                                value={routeName}
                                onChange={(e) => setRouteName(e.target.value)}
                                placeholder="Enter route name"
                                style={{ width: '100%', marginBottom: '10px' }}
                            />
                        </label>
                        <p><strong>Distance:</strong> {route.distanceKm.toFixed(2)} km</p>

                        <button
                            onClick={() => {
                                if (editingRoute) {
                                    const updatedRoute: RunningRoute = {
                                        ...editingRoute,
                                        name: routeName || `Route - ${route.distanceKm.toFixed(2)} km`,
                                        geometry: {
                                            type: 'LineString',
                                            coordinates: route.path.map((p) => [p.longitude, p.latitude]),
                                        },
                                        distanceKm: route.distanceKm,
                                    };
                                    updateRunningRoute(updatedRoute);
                                } else {
                                    saveRunningRoute(route);
                                }
                            }}
                            disabled={saving}
                        >
                            {saving ? 'Saving...' : editingRoute ? 'Update Route' : 'Save Route'}
                        </button>

                        {editingRoute && (
                            <button
                                onClick={resetMapState}
                                style={{ marginLeft: '10px' }}
                            >
                                Cancel
                            </button>
                        )}

                        {message && <p style={{ marginTop: '8px' }}>{message}</p>}
                    </div>
                )}
            </div>
        </div>
    );
}
