# RaceDay – API Endpoint Plan

This document defines every API endpoint the RaceDay system exposes.
It matches the ERD in `docs/ERD.png` and covers all Part 2 functional requirements.

---

## 1. Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Registers a new user (Organiser or Participant) with hashed password. | None | { fullName, email, password, role, phoneNumber } | 201 Created – new user record (no password)<br>400 Bad Request – invalid input<br>409 Conflict – email already exists |
| POST | /api/auth/login | Authenticates a user and returns a JWT token. | None | { email, password } | 200 OK – JWT + user info<br>401 Unauthorized – invalid credentials |

---

## 2. User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Returns the profile of the currently logged-in user. | Any | None | 200 OK – user profile<br>401 Unauthorized |
| PUT | /api/users/me | Updates the logged-in user's profile details. | Any | { fullName, phoneNumber } | 200 OK – updated profile<br>400 Bad Request – invalid input |
| GET | /api/users | Returns a list of all users (admin/organiser view). | Organiser | None | 200 OK – user list<br>403 Forbidden – not an organiser |

---

## 3. Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Returns a list of all upcoming events (public browsing). | None | None | 200 OK – event list |
| GET | /api/events/{id} | Returns details of a single event, including its categories. | None | None | 200 OK – event details<br>404 Not Found |
| POST | /api/events | Creates a new event owned by the logged-in organiser. | Organiser | { name, description, eventDate, location, province } | 201 Created – new event<br>400 Bad Request<br>403 Forbidden |
| PUT | /api/events/{id} | Updates an existing event owned by the organiser. | Organiser | { name, description, eventDate, location, province } | 200 OK – updated event<br>403 Forbidden<br>404 Not Found |
| DELETE | /api/events/{id} | Deletes an event owned by the organiser. | Organiser | None | 204 No Content<br>403 Forbidden<br>404 Not Found |
| GET | /api/events/mine | Returns all events created by the logged-in organiser. | Organiser | None | 200 OK – organiser's event list<br>403 Forbidden |

---

## 4. Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Returns all categories for a given event. | None | None | 200 OK – category list<br>404 Not Found |
| GET | /api/categories/{id} | Returns a single category by id. | None | None | 200 OK – category details<br>404 Not Found |
| POST | /api/events/{eventId}/categories | Adds a new category to an event owned by the organiser. | Organiser | { name, distanceKm, entryFee, maxParticipants } | 201 Created – new category<br>400 Bad Request<br>403 Forbidden |
| PUT | /api/categories/{id} | Updates an existing category owned by the organiser. | Organiser | { name, distanceKm, entryFee, maxParticipants } | 200 OK – updated category<br>403 Forbidden<br>404 Not Found |
| DELETE | /api/categories/{id} | Deletes a category owned by the organiser. | Organiser | None | 204 No Content<br>403 Forbidden<br>404 Not Found |

---

## 5. Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enrols the logged-in participant into a specific category. | Participant | { categoryId } | 201 Created – enrolment record<br>400 Bad Request – category full<br>404 Not Found<br>409 Conflict – already enrolled |
| GET | /api/enrolments/mine | Returns all enrolments for the logged-in participant. | Participant | None | 200 OK – enrolment list |
| GET | /api/enrolments/{id} | Returns details of a single enrolment (participant owns it or organiser owns the event). | Any | None | 200 OK – enrolment details<br>403 Forbidden<br>404 Not Found |
| GET | /api/events/{eventId}/enrolments | Returns all enrolments for an event owned by the organiser. | Organiser | None | 200 OK – enrolment list<br>403 Forbidden<br>404 Not Found |
| DELETE | /api/enrolments/{id} | Cancels the logged-in participant's own enrolment. | Participant | None | 204 No Content<br>403 Forbidden<br>404 Not Found |

---

## 6. Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/results | Captures a race result for a specific enrolment. | Organiser | { enrolmentId, finishTime, position, didNotFinish } | 201 Created – new result<br>400 Bad Request<br>403 Forbidden<br>404 Not Found<br>409 Conflict – result already captured |
| GET | /api/results/mine | Returns all personal race results for the logged-in participant. | Participant | None | 200 OK – result list |
| GET | /api/results/{id} | Returns a single result by id. | Any | None | 200 OK – result details<br>403 Forbidden<br>404 Not Found |
| PUT | /api/results/{id} | Updates a race result captured by the organiser. | Organiser | { finishTime, position, didNotFinish } | 200 OK – updated result<br>403 Forbidden<br>404 Not Found |
| DELETE | /api/results/{id} | Deletes a race result owned by the organiser. | Organiser | None | 204 No Content<br>403 Forbidden<br>404 Not Found |
| GET | /api/events/{eventId}/results | Returns all results for an event owned by the organiser. | Organiser | None | 200 OK – result list<br>403 Forbidden<br>404 Not Found |

---

## Summary

- **Total endpoints:** 26
- **Authentication:** 2
- **User Profile:** 3
- **Events:** 6
- **Categories:** 5
- **Enrolments:** 5
- **Results:** 6

All endpoints enforce role-based access at the API level as required by Part 2 of the POE.