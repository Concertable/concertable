import googleGeocodingApi from "@concertable/shared/lib/googleGeocodingApi";
import Config from "./config";

googleGeocodingApi.configure(Config.googleMapsApiKey);
