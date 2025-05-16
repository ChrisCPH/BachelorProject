export interface RunningRoute {
    id: string;
    userID?: number;
    name: string;
    geometry: GeoJsonLineString;
    createdAt?: string;
    distanceKm?: number;
}

export interface GeoJsonLineString {
    type: 'LineString';
    coordinates: [number, number][];
}

export type Route = {
    path: { latitude: number; longitude: number }[];
    distanceKm: number;
};