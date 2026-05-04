Feature: FlatFee workflow happy path
  A venue manager posts a flat fee opportunity, an artist applies, the venue
  manager accepts and pays the flat fee. The booking lands in escrow and a
  draft concert is created.

  @VenueManager
  Scenario: Venue manager posts a flat fee opportunity
    When the venue manager posts a flat fee opportunity for £500
    Then the opportunity is saved
