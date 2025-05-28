import { MapContainer, TileLayer, Polyline } from 'react-leaflet';
import { useEffect, useState } from 'react';
import { RouteMap } from '../components/RouteMap';
import 'leaflet/dist/leaflet.css';
import 'leaflet-draw/dist/leaflet.draw.css';
import { RunningRoute, GeoJsonLineString, Route } from '../types/RunningRoute';
import { GeoPoint } from '../types/GeoPoint';
import { Polygon } from '../types/Polygon';
import { useSearchParams } from 'react-router-dom';
import { PolygonDrawer } from '../components/PolygonDrawer';
import { FlyToRoute } from '../components/FlyToRoute';
import { LocationMarker } from '../components/LocationMarker';

type SearchMode = 'point' | 'within' | 'intersect' | null;

export default function RoutePlanner() {
    const [route, setRoute] = useState<Route | null>(null);
    const [routeName, setRouteName] = useState('');
    const [saving, setSaving] = useState(false);
    const [message, setMessage] = useState<string | null>(null);
    const [savedRoutes, setSavedRoutes] = useState<RunningRoute[]>([]);
    const [selectedRouteCoords, setSelectedRouteCoords] = useState<[number, number][]>([]);
    const [editingRoute, setEditingRoute] = useState<RunningRoute | null>(null);
    const [searchParams] = useSearchParams();
    const [selectedPoint, setSelectedPoint] = useState<GeoPoint | null>(null);
    const [selectedPolygon, setSelectedPolygon] = useState<Polygon | null>(null);
    const [maxDistance, setMaxDistance] = useState<number>(50); // in km
    const [searchMode, setSearchMode] = useState<SearchMode>(null);
    const selectedRouteId = searchParams.get('routeId');
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

    const fetchNearbyRoutes = async (point: GeoPoint) => {
        try {
            const res = await fetch(`${API_BASE_URL}/runningroute/nearby`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
                body: JSON.stringify({
                    ...point,
                    maxDistanceMeters: maxDistance * 1000 // convert km to meters
                }),
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

    const fetchRoutesWithinPolygon = async (polygon: Polygon) => {
        try {
            const res = await fetch(`${API_BASE_URL}/runningroute/within`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
                body: JSON.stringify(polygon),
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

    const fetchRoutesIntersectingPolygon = async (polygon: Polygon) => {
        try {
            const res = await fetch(`${API_BASE_URL}/runningroute/intersect`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${localStorage.getItem('token')}`,
                },
                body: JSON.stringify(polygon),
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
        if (selectedPoint && searchMode === 'point') {
            fetchNearbyRoutes(selectedPoint);
        } else if (selectedPolygon && searchMode === 'within') {
            fetchRoutesWithinPolygon(selectedPolygon);
        } else if (selectedPolygon && searchMode === 'intersect') {
            fetchRoutesIntersectingPolygon(selectedPolygon);
        }
    }, [selectedPoint, selectedPolygon, maxDistance, selectedRouteId, searchMode]);

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
            if (searchMode === 'point' && selectedPoint) {
                fetchNearbyRoutes(selectedPoint);
            } else if ((searchMode === 'within' || searchMode === 'intersect') && selectedPolygon) {
                if (searchMode === 'within') {
                    fetchRoutesWithinPolygon(selectedPolygon);
                } else {
                    fetchRoutesIntersectingPolygon(selectedPolygon);
                }
            }
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
            if (searchMode === 'point' && selectedPoint) {
                fetchNearbyRoutes(selectedPoint);
            } else if ((searchMode === 'within' || searchMode === 'intersect') && selectedPolygon) {
                if (searchMode === 'within') {
                    fetchRoutesWithinPolygon(selectedPolygon);
                } else {
                    fetchRoutesIntersectingPolygon(selectedPolygon);
                }
            }
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

    const handleLocationSelect = (point: GeoPoint) => {
        setSelectedPoint(point);
        setSelectedPolygon(null);
    };

    const handlePolygonComplete = (polygon: Polygon) => {
        setSelectedPolygon(polygon);
        setSelectedPoint(null);
    };

    const handleResetSearch = () => {
        setSelectedPoint(null);
        setSelectedPolygon(null);
        setSearchMode(null);
        setSavedRoutes([]);
        resetMapState();
    };

    const renderSidebarContent = () => {
        if (!searchMode) {
            return (
                <div>
                    <h3>Search Options</h3>
                    <button
                        onClick={() => setSearchMode('point')}
                        style={{
                            display: 'block',
                            width: '100%',
                            marginBottom: '10px',
                            padding: '10px',
                            backgroundColor: '#f0f0f0',
                            border: '1px solid #ccc',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Get routes near a point
                    </button>
                    <button
                        onClick={() => setSearchMode('within')}
                        style={{
                            display: 'block',
                            width: '100%',
                            marginBottom: '10px',
                            padding: '10px',
                            backgroundColor: '#f0f0f0',
                            border: '1px solid #ccc',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Get routes within triangle
                    </button>
                    <button
                        onClick={() => setSearchMode('intersect')}
                        style={{
                            display: 'block',
                            width: '100%',
                            marginBottom: '10px',
                            padding: '10px',
                            backgroundColor: '#f0f0f0',
                            border: '1px solid #ccc',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Get routes intersecting triangle
                    </button>
                </div>
            );
        }

        if (searchMode === 'point' && !selectedPoint) {
            return (
                <div>
                    <h3>Select a location</h3>
                    <p>Click on the map to select a point to search for nearby routes.</p>
                    <div style={{ marginBottom: '10px' }}>
                        <label>
                            Max distance (km):
                            <input
                                type="range"
                                min="1"
                                max="50"
                                value={maxDistance}
                                onChange={(e) => setMaxDistance(Number(e.target.value))}
                                style={{ width: '100%' }}
                            />
                            {maxDistance} km
                        </label>
                    </div>
                    <button
                        onClick={handleResetSearch}
                        style={{
                            marginBottom: '5px',
                            backgroundColor: '#f0f0f0',
                            border: '1px solid #ccc',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Back to options
                    </button>
                </div>
            );
        }

        if ((searchMode === 'within' || searchMode === 'intersect') && !selectedPolygon) {
            return (
                <div>
                    <h3>{searchMode === 'within' ? 'Draw area to find routes within' : 'Draw area to find routes intersecting'}</h3>
                    <p>Draw a triangle on the map to search for routes.</p>
                    <button
                        onClick={handleResetSearch}
                        style={{
                            marginBottom: '5px',
                            backgroundColor: '#f0f0f0',
                            border: '1px solid #ccc',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Back to options
                    </button>
                </div>
            );
        }

        return (
            <>
                <h3>
                    {searchMode === 'point' ? `Routes within ${maxDistance} km` :
                        searchMode === 'within' ? 'Routes within area' : 'Routes intersecting area'}
                </h3>
                <button
                    onClick={handleResetSearch}
                    style={{
                        marginBottom: '5px',
                        backgroundColor: '#f0f0f0',
                        border: '1px solid #ccc',
                        borderRadius: '4px',
                        cursor: 'pointer'
                    }}
                >
                    Change Search
                </button>
                <button
                    onClick={resetMapState}
                    style={{
                        marginBottom: '5px',
                        backgroundColor: '#f0f0f0',
                        border: '1px solid #ccc',
                        borderRadius: '4px',
                        cursor: 'pointer',
                        marginLeft: '5px'
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
            </>
        );
    };

    const renderMapContent = () => {
        if (!searchMode) {
            return null;
        }

        if (searchMode === 'point' && !selectedPoint) {
            return <LocationMarker onLocationSelect={handleLocationSelect} />;
        }

        if ((searchMode === 'within' || searchMode === 'intersect') && !selectedPolygon) {
            return <PolygonDrawer
                onPolygonComplete={handlePolygonComplete}
                mode={searchMode}
            />;
        }

        return (
            <>
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
            </>
        );
    };

    return (
        <div style={{ height: '100vh', display: 'flex' }}>
            <div style={{ width: '300px', padding: '10px', backgroundColor: '#f7f7f7', overflowY: 'auto' }}>
                {renderSidebarContent()}
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

                    {renderMapContent()}
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