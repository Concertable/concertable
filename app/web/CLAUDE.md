# Web Apps

After making any changes to a web app or shared code, run a build to verify before reporting done:

```
npm -w @concertable/web-customer run build
npm -w @concertable/web-venue run build
npm -w @concertable/web-artist run build
npm -w @concertable/web-business run build
```

The business app uses `vite build` only (no `tsc -b`) — it's a minimal app that only uses a slice of shared and does not implement the full feature set that shared references.
