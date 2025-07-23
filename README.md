# How to Run the Code

### Logins:
#### Venue Manager:
- Emails: `venuemanager1@test.com`, `venuemanager2@test.com`
- Password (for both): `Password11!`

#### ArtistManager:
- Emails: `artistmanager1@test.com`, `artistmanager2@test.com`
- Password (for both): `Password11!`

#### Customer:
- Emails: `customer1@test.com`, `customer2@test.com`
- Password (for both): `Password11!`

### Important Note:
> **These are the only accounts that can pass financial transactions between each other** as they are the only ones with a **Stripe account** with **KYC (Know Your Customer)** associated with them. 
> 
> All other accounts have a **Stripe ID** but no associated Stripe account. Therefore, any attempted transactions made to these will result in the server informing you that a stripe account needs to be created for the recipiant in order for them to recieve the funds.
---

## On the Cloud

Type in the following URL: [https://concertable-app.azurewebsites.net/](https://concertable-app.azurewebsites.net/)

# How to Use the App

## Creating an Event

### 1. Create Listing

- **Login** as `venuemanager1@test.com`
- Navigate to the **Venue Profile** section: `/venue/my`
- Scroll to the bottom and create and save a listing

### 2. Apply to Listing

- **Login** as `artistmanager1@test.com`
- Navigate to **Find a Venue**: `/artist/find`
- Press **Search**
- Click on **The Grand Venue**
- Scroll down to **Listings**
- Click the **Apply** button on the appropriate listing

### 3. Accept Listing Application

- **Login** as `venuemanager1@test.com`
- Go to **Messages** and click the **"View"** button next to the notification
  - Alternatively, on your dashboard (`/venue`), click the **"View Applications"** button next to your active listing
- Click **"Accept"** next to the artist who just applied
- Enter the following **card details**:
  - **Card Number**: `4242 4242 4242 4242`
  - **Expiry Date**: `12/34`
  - **CCV**: `123`
- Confirm the payment
- Once the payment has gone through, the app will navigate you to the **automatically created event**

### 4. Posting an Event

- On the same account and page, enter the **number of tickets available** and the **ticket price** at the bottom of the page
- Press **"Save"**

---

## Buying a Ticket

### 1. Login and Search for the Event

- **Login** as `customer1@test.com`
- Navigate to **Find Events/Artists/Venues**: `/find`
- Search for the event you just created
- Click on that event

---
**Alternatively**:
- Whenever a new event is posted that matches the customer preferenmces, it will instantaneously notify the customer with a toast message, so the new event can also immediately be navigated to in that way

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

