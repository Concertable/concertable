Feature: Versus workflow happy path
  A venue manager posts a versus opportunity (guarantee + door percentage), an artist
  applies, the venue manager accepts and registers a card for future settlement. A draft
  concert is created.

  @VenueManager
  Scenario: Venue manager books artist on a versus deal
    When the venue manager posts a versus opportunity for £100 guarantee and 70% door
    And the artist applies to the opportunity
    And the venue manager accepts the application with a valid card
    Then a draft concert is created
