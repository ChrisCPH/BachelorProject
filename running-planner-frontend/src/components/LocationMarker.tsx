import { useState } from "react";
import { Marker, useMapEvents } from "react-leaflet";
import { GeoPoint } from "../types/GeoPoint";

export function LocationMarker({ onLocationSelect }: { onLocationSelect: (point: GeoPoint) => void }) {
    const [position, setPosition] = useState<L.LatLng | null>(null);
    useMapEvents({
        click(e) {
            setPosition(e.latlng);
            onLocationSelect({
                latitude: e.latlng.lat,
                longitude: e.latlng.lng,
                maxDistanceMeters: 5000 // Default 5km
            });
        },
    });

    return position === null ? null : (
        <Marker position={position}></Marker>
    );
}