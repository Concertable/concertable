# Concertable Mobile App — Plan & Progress

## Stack

| Concern | Choice |
|---|---|
| Framework | Expo SDK 54 + React Native 0.81 |
| Navigation | React Navigation v7 (native-stack + bottom-tabs) |
| Styling | NativeWind v4 (Tailwind v3 syntax) |
| Data fetching | TanStack React Query v5 |
| Auth | expo-auth-session (OIDC PKCE → Duende IS) |
| Token storage | expo-secure-store |
| Payments | @stripe/stripe-react-native |
| QR codes | react-native-qrcode-svg |
| Toasts | burnt |
| Image picker | expo-image-picker |
| Shared code | @concertable/shared (API clients, React Query hooks, Zustand stores, types) |

---

## Phases

### ✅ Phase 1 — Scaffold + Auth (DONE)
- [x] Expo project, babel/metro config, workspace setup
- [x] OIDC PKCE login via `expo-auth-session` against Duende IS
- [x] Token storage in `expo-secure-store`
- [x] Role-based tab navigation (Customer / Artist / Venue / Auth)
- [x] `useAuthInit` — cold-start token hydration
- [x] `useLogout` — IdP end-session + local token clear
- [x] Axios request interceptor reading SecureStore on each request
- [x] Token refresh on 401 (refresh → retry → sign-out fallback)
- [x] NativeWind v4 + `tailwind.config.js` + `metro.config.js`
- [x] `burnt` toast lib wired via `src/lib/toast.ts`
- [x] UI primitives: `Button`, `Card`, `Input`, `Avatar`, `Badge`, `Skeleton`, `Screen`

### ✅ Phase 2 — Customer MVP (DONE)
- [x] `HomeScreen` — concert grid via `useHeaderAmountQuery("concert")`
- [x] `SearchScreen` — live search via `useHeaderQuery` + `useSearchFiltersStore`
- [x] `ConcertDetailScreen` — full concert info + buy button
- [x] `TicketsScreen` — upcoming / history toggle
- [x] `TicketDetailScreen` — QR code display
- [x] `TicketCheckoutScreen` — Stripe PaymentSheet integration
- [x] `CheckoutSuccessScreen` — confirmation + navigate to Tickets
- [x] `ProfileScreen` — user info + sign out
- [x] Nested stacks per tab (HomeStack, SearchStack, TicketsStack)
- [x] `StripeProvider` in `AppProviders`
- [x] `EXPO_PUBLIC_STRIPE_KEY` in `.env` (fill in `pk_test_...` before testing payments)

### 🔲 Phase 3 — Artist features (`Feature/MobileArtist`)
- [ ] `ArtistDashboardScreen` — upcoming gigs, active applications
- [ ] `ArtistProfileScreen` — edit profile, banner/avatar upload (expo-image-picker)
- [ ] `OpportunitiesScreen` — browse open opportunities
- [ ] `ApplicationsScreen` — my applications + status
- [ ] `ApplyCheckoutScreen` — prepaid opportunity checkout

### 🔲 Phase 4 — Venue features (`Feature/MobileVenue`)
- [ ] `VenueDashboardScreen` — concerts, stats
- [ ] `VenueProfileScreen` — edit profile
- [ ] `MyConcertScreen` — manage opportunities, view applications
- [ ] `AcceptApplicationScreen` — accept/reject artist applications

### 🔲 Phase 5 — Messaging + SignalR (`Feature/MobileMessaging`)
- [ ] `MailboxScreen` / `ThreadScreen`
- [ ] SignalR connection (with RN polyfill)
- [ ] Real-time notification handlers

### 🔲 Phase 6 — Push notifications
- [ ] `expo-notifications` setup
- [ ] Notification permission flow
- [ ] Deep-link from push → relevant screen

### 🔲 Phase 7 — Polish + E2E
- [ ] Pull-to-refresh across list screens
- [ ] Offline / error boundary handling
- [ ] Accessibility pass
- [ ] Maps (react-native-maps) on Search
- [ ] Maestro / Detox E2E

---

## Pre-flight checklist (before first real device run)

1. Add `pk_test_...` to `EXPO_PUBLIC_STRIPE_KEY` in `.env`
2. Set `EXPO_PUBLIC_API_URL` + `EXPO_PUBLIC_AUTH_AUTHORITY` to your LAN IP for physical device (or `10.0.2.2` for Android emulator)
3. Ensure Duende IS has `concertable-mobile` client registered with scheme `concertable://`
4. Run `npx expo prebuild` if targeting a bare workflow build

---

## Key file map

| File | Purpose |
|---|---|
| `App.tsx` | Entry — imports `global.css`, mounts `AppProviders` + `RootNavigator` |
| `src/lib/axios.ts` | Configures shared axios: base URL, bearer interceptor, refresh-on-401 |
| `src/lib/config.ts` | `EXPO_PUBLIC_*` env vars |
| `src/lib/toast.ts` | `notify(msg, type)` wrapper over `burnt` |
| `src/auth/tokenStorage.ts` | SecureStore get/set/clear |
| `src/auth/useLogin.ts` | PKCE code exchange |
| `src/auth/useAuthInit.ts` | Cold-start hydration |
| `src/auth/useLogout.ts` | Token clear + IdP end-session |
| `src/navigation/RootNavigator.tsx` | Auth → role-based tab switch |
| `src/navigation/CustomerTabs.tsx` | Bottom tabs: Home / Search / Tickets / Messages / Profile |
| `src/navigation/HomeStack.tsx` | Home → ConcertDetail → TicketCheckout → CheckoutSuccess |
| `src/navigation/SearchStack.tsx` | Search → ConcertDetail → TicketCheckout → CheckoutSuccess |
| `src/navigation/TicketsStack.tsx` | Tickets → TicketDetail |
| `src/providers/AppProviders.tsx` | GestureHandler + SafeArea + Stripe + QueryClient |
| `src/components/ui/` | Button, Card, Input, Avatar, Badge, Skeleton, Screen |
| `src/screens/customer/` | All customer-facing screens |
| `tailwind.config.js` | Tailwind v3 config for NativeWind |
| `metro.config.js` | `withNativeWind` wrapper |
| `global.css` | `@tailwind base/components/utilities` |
