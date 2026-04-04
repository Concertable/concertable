import { Map, Marker } from "@vis.gl/react-google-maps";

interface Props {
  lat?: number;
  lng?: number;
  className?: string;
}

export function GoogleMap({ lat, lng, className }: Readonly<Props>) {
  if (!lat || !lng) return null;

  return (
    <div className={className}>
      <Map
        style={{ width: "100%", height: "400px" }}
        defaultCenter={{ lat, lng }}
        defaultZoom={14}
        gestureHandling="none"
        disableDefaultUI
      >
        <Marker position={{ lat, lng }} />
      </Map>
    </div>
  );
}
