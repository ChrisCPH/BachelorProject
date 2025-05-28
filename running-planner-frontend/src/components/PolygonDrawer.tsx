import { useEffect, useRef } from "react";
import { useMap } from "react-leaflet";
import { Polygon } from "../types/Polygon";
import L from "leaflet";

export function PolygonDrawer({ onPolygonComplete, mode }: { onPolygonComplete: (polygon: Polygon) => void, mode: 'within' | 'intersect' }) {
    const map = useMap();
    const drawnItemsRef = useRef<L.FeatureGroup | null>(null);

    useEffect(() => {
        if (!map) return;

        const drawnItems = new L.FeatureGroup();
        drawnItemsRef.current = drawnItems;
        map.addLayer(drawnItems);

        const drawControl = new L.Control.Draw({
            draw: {
                polygon: {
                    shapeOptions: {
                        color: '#3388ff',
                        weight: 4
                    },
                    allowIntersection: false,
                    showArea: true
                },
                marker: false,
                circle: false,
                rectangle: false,
                circlemarker: false,
                polyline: false
            },
            edit: {
                featureGroup: drawnItems,
                edit: false,
                remove: false
            }
        });

        map.addControl(drawControl);

        const handleCreated = (e: any) => {
            const layer = e.layer;
            if (layer instanceof L.Polygon) {
                const latlngs = layer.getLatLngs()[0] as L.LatLng[];
                const coordinates = latlngs.map(latlng => [latlng.lng, latlng.lat]);

                // For a valid GeoJSON polygon, the first and last points must be the same
                if (coordinates.length > 0 &&
                    (coordinates[0][0] !== coordinates[coordinates.length - 1][0] ||
                        coordinates[0][1] !== coordinates[coordinates.length - 1][1])) {
                    coordinates.push([coordinates[0][0], coordinates[0][1]]);
                }

                onPolygonComplete({
                    coordinates
                });
            }
            drawnItems.clearLayers();
            drawnItems.addLayer(layer);
            map.removeControl(drawControl);
        };

        map.on(L.Draw.Event.CREATED, handleCreated);

        return () => {
            map.off(L.Draw.Event.CREATED, handleCreated);
            if (drawnItemsRef.current) {
                map.removeLayer(drawnItemsRef.current);
            }
            map.removeControl(drawControl);
        };
    }, [map, mode]);

    return null;
}