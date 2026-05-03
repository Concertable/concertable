Feature: Login
  Smoke test that the OIDC redirect dance completes end-to-end via SPA + Auth host.

  Scenario: Customer signs in via OIDC
    Given a fresh browser context
    When the customer signs in with seeded credentials
    Then the SPA lands on the home page
