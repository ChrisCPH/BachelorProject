import { useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import length from '@turf/length';
import { lineString } from '@turf/helpers';
import 'leaflet-draw';

export const RouteMap = ({
    onRouteCreated,
    existingRoute
}: {
    onRouteCreated: (route: any) => void,
    existingRoute: { path: { latitude: number; longitude: number }[], distanceKm: number } | null
}) => {
    const map = useMap();
    const drawnItemsRef = useRef<L.FeatureGroup | null>(null);
    const drawControlRef = useRef<L.Control.Draw | null>(null);

    useEffect(() => {
        if (drawnItemsRef.current) {
            map.removeLayer(drawnItemsRef.current);
        }

        const drawnItems = new L.FeatureGroup();
        drawnItemsRef.current = drawnItems;
        map.addLayer(drawnItems);

        const initialDrawConfig: L.Control.DrawOptions = {
            polygon: false,
            marker: false,
            circle: false,
            rectangle: false,
            circlemarker: false,
            polyline: existingRoute ? false : {
                metric: true,
                showLength: true,
                shapeOptions: {
                    color: '#3388ff',
                    weight: 4
                }
            }
        };

        if (existingRoute) {
            const latlngs = existingRoute.path.map(p => L.latLng(p.latitude, p.longitude));
            const polyline = L.polyline(latlngs, { color: 'blue' });
            drawnItems.addLayer(polyline);
            map.fitBounds(polyline.getBounds());
        }

        const drawControl = new L.Control.Draw({
            draw: initialDrawConfig,
            edit: {
                featureGroup: drawnItems,
                remove: false,
            },
        });

        drawControlRef.current = drawControl;
        map.addControl(drawControl);

        const handleCreated = (e: any) => {
            drawnItems.clearLayers();
            const layer = e.layer;
            drawnItems.addLayer(layer);

            if (drawControlRef.current) {
                map.removeControl(drawControlRef.current);
                drawControlRef.current = new L.Control.Draw({
                    draw: {
                        polygon: false,
                        marker: false,
                        circle: false,
                        rectangle: false,
                        circlemarker: false,
                        polyline: false,
                    },
                    edit: {
                        featureGroup: drawnItems,
                        remove: false,
                    },
                });
                map.addControl(drawControlRef.current);
            }

            const latlngs = layer.getLatLngs();
            const coordinates = latlngs.map((point: L.LatLng) => [point.lng, point.lat]);

            const line = lineString(coordinates);
            const distance = length(line, { units: 'kilometers' });

            onRouteCreated({
                path: coordinates.map(([lng, lat]: [number, number]) => ({
                    latitude: lat,
                    longitude: lng,
                })),
                distanceKm: distance,
            });
        };

        const handleEdited = (e: any) => {
            const layers = e.layers;
            layers.eachLayer((layer: any) => {
                if (layer instanceof L.Polyline) {
                    const latlngs = layer.getLatLngs();
                    const coordinates = (latlngs as L.LatLng[]).map((point: L.LatLng) => [point.lng, point.lat]);

                    const line = lineString(coordinates);
                    const distance = length(line, { units: 'kilometers' });

                    onRouteCreated({
                        path: (coordinates as [number, number][]).map(([lng, lat]) => ({
                            latitude: lat,
                            longitude: lng,
                        })),
                        distanceKm: distance,
                    });
                }
            });
        };

        map.on(L.Draw.Event.CREATED, handleCreated);
        map.on(L.Draw.Event.EDITED, handleEdited);

        return () => {
            map.off(L.Draw.Event.CREATED, handleCreated);
            map.off(L.Draw.Event.EDITED, handleEdited);
            if (drawControlRef.current) {
                map.removeControl(drawControlRef.current);
            }
            if (drawnItemsRef.current) {
                map.removeLayer(drawnItemsRef.current);
            }
        };
    }, [map, onRouteCreated, existingRoute]);

    return null;
};