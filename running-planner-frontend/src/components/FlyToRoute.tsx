import { useEffect } from "react";
import { useMap } from "react-leaflet";

export function FlyToRoute({ coordinates }: { coordinates: [number, number][] }) {
    const map = useMap();

    useEffect(() => {
        if (coordinates.length > 0) {
            const bounds = coordinates.map(([lng, lat]) => [lat, lng]);
            map.fitBounds(bounds as [number, number][]);
        }
    }, [coordinates]);

    return null;
}