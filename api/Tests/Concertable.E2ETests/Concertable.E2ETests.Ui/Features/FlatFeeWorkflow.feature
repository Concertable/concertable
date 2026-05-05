Feature: FlatFee workflow happy path
  A venue manager posts a flat fee opportunity, an artist applies, the venue
  manager accepts and pays the flat fee. The booking lands in escrow and a
  draft concert is created.

  @VenueManager
  Scenario: Venue manager books artist on a flat fee
    When the venue manager posts a flat fee opportunity for £500
    And the artist applies to the opportunity
    And the venue manager accepts the application with a valid card
    Then a draft concert is created
