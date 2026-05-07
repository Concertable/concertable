Feature: VenueHire workflow happy path
  A venue manager posts a venue hire opportunity, an artist pays the hire fee
  upfront to apply, the venue manager accepts (no further payment). A draft concert is created.

  @VenueManager
  Scenario: Artist pays hire fee upfront to book venue
    When the venue manager posts a venue hire opportunity for £300
    And the artist applies to the venue hire opportunity with a valid card
    And the venue manager accepts the application
    Then a draft concert is created
    And a payment hold of £300 is captured from the artist

  @VenueManager
  Scenario: Artist pays hire fee upfront with a new card
    Given a venue hire opportunity is open for application
    When the artist pays the venue hire fee with a new card
    And the venue manager accepts the application
    Then a draft concert is created
    And a payment hold of £250 is captured from the artist
