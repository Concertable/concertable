# How to Run the Code

### Prerequisites
- Docker Desktop (running)

### Start
From the repo root:
```
dotnet run --project api/Concertable.AppHost
```
The Aspire dashboard will open and list all service URLs.

---

### Logins

#### Venue Manager:
- Emails: `venuemanager1@test.com`, `venuemanager2@test.com`
- Password (for both): `Password11!`

#### Artist Manager:
- Emails: `artistmanager1@test.com`, `artistmanager2@test.com`
- Password (for both): `Password11!`

#### Customer:
- Emails: `customer1@test.com`, `customer2@test.com`
- Password (for both): `Password11!`

> **These are the only accounts that can pass financial transactions between each other** as they are the only ones with a **Stripe account** with **KYC (Know Your Customer)** associated with them.
>
> All other accounts have a **Stripe ID** but no associated Stripe account. Therefore, any attempted transactions made to these will result in the server informing you that a Stripe account needs to be created for the recipient in order for them to receive the funds.

---

# How to Use the App

## Creating a Concert

### 1. Create Opportunity

- **Login** as `venuemanager1@test.com`
- Navigate to the **Venue Profile** section: `/venue/my`
- Scroll to the bottom, pick a contract, and create and save an opportunity

### 2. Apply to Opportunity

- **Login** as `artistmanager1@test.com`
- Navigate to **Find a Venue**: `/artist/find`
- Press **Search**
- Click on **The Grand Venue**
- Scroll down to **Opportunities**
- Click the **Apply** button on the appropriate opportunity

### 3. Accept Application

- **Login** as `venuemanager1@test.com`
- Go to **Messages** and click the **"View"** button next to the notification
  - Alternatively, on your dashboard (`/venue`), click the **"View Applications"** button next to your active opportunity
- Click **"Accept"** next to the artist who just applied
- Enter the following **card details**:
  - **Card Number**: `4242 4242 4242 4242`
  - **Expiry Date**: `12/34`
  - **CCV**: `123`
- Confirm the payment
- Once the payment has gone through, the app will navigate you to the **automatically created concert**

### 4. Post a Concert

- On the same account and page, enter the **number of tickets available** and the **ticket price** at the bottom of the page
- Press **"Save"**

---

## Buying a Ticket

### 1. Login and Search for the Concert

- **Login** as `customer1@test.com`
- Navigate to **Find Events/Artists/Venues**: `/find`
- Search for the concert you just created
- Click on it

---
**Alternatively**:
- Whenever a new concert is posted that matches the customer's preferences, it will instantaneously notify the customer with a toast message, which can be clicked to navigate directly to it

### 2. Buy Ticket

- Click on **"Buy Tickets"**
- Enter the following **card details**:
  - **Card Number**: `4242 4242 4242 4242`
  - **Expiry Date**: `12/34`
  - **CCV**: `123`
- Enter a **quantity of tickets**
- Confirm the payment
- Once the payment has gone through, a **toast message** will pop up
  - Clicking on this will navigate you to your **purchased ticket(s)**
