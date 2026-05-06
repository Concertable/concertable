Feature: Ticket purchase happy path
  A customer uses the search filter to find a concert, purchases a ticket, waits
  for confirmation, and then opens their QR code from the upcoming tickets page.

  @Customer
  Scenario: Customer searches for concerts, purchases a ticket, and views the QR code
    When the customer opens the filter panel on the find page
    And the customer selects the header type "Concert"
    And the customer applies the filters
    Then concert results should be visible
    When the customer clicks the first concert result
    Then the customer should be on the concert detail page
    When the customer clicks buy tickets
    Then the customer should be on the checkout page
    When the customer pays with a test card and confirms
    Then the checkout awaiting screen should be visible
    And the checkout success screen should be visible
    And a ticket purchased toast should appear
    When the customer clicks view tickets
    Then the customer should be on the upcoming tickets page
    And a ticket card should be listed
    When the customer opens the QR code
    Then the QR dialog should be visible
    And the QR image should be present
