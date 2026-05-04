Feature: FlatFee workflow happy path
  A venue manager posts a flat fee opportunity, an artist applies, the venue
  manager accepts and pays the flat fee. The booking lands in escrow and a
  draft concert is created.

  Scenario: Venue manager books artist on a flat fee contract
    When a venue manager posts a flat fee opportunity for £500
    And an artist applies to the opportunity
    And the venue manager accepts the application with a valid card
    Then the application is accepted
    And a draft concert is created
    And £500 is held in escrow for the artist
