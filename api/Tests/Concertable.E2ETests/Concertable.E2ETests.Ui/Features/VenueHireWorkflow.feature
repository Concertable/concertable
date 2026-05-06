Feature: VenueHire workflow happy path
  A venue manager posts a venue hire opportunity, an artist pays the hire fee
  upfront to apply, the venue manager accepts (no further payment). A draft concert is created.

  @VenueManager
  Scenario: Artist pays hire fee upfront to book venue
    When the venue manager posts a venue hire opportunity for £300
    And the artist applies to the venue hire opportunity with a valid card
    And the venue manager accepts the application
    Then a draft concert is created
