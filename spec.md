# GloberBooks Product Specification
## 1. Overview
**Problem Statement:** Readers need a simple way to track books they want to read, are reading, and have completed.
**Target Users:** Individual readers managing personal reading lists
**Success Metrics:** - User can add a book in <10 seconds
- User can view their library instantly
- Zero data loss on app restart
## 2. User Stories
### US-001: Add Book
**As a** user
**I want** to add a book with title, author, and status
**So that** I can track my reading list
**Acceptance Criteria:**
- Title is required (1-500 chars)
- Author is required (1-200 chars)
- Status is optional, defaults to "unread"
- Valid statuses: unread, reading, completed
- System assigns unique ID and timestamp
### US-002: List Books
**As a** user
**I want** to see all my books in a list
**So that** I can review my reading progress
**Acceptance Criteria:**
- Shows all books sorted by date added (newest first)
- Displays title, author, status, rating (if completed)
- Option to filter by status
- Empty state message if no books
### US-003: Rate Completed Books
**As a** user
**I want** to rate books I've completed
**So that** I can remember which ones I enjoyed
**Acceptance Criteria:**
- Rating only allowed for "completed" books
- Rating is 1-5 (integers only)
- Rating is optional (can be null)
- Can edit rating after initial assignment
### US-004: Prevent Duplicate Books
**As a** user
**I want** to be warned about duplicate books
**So that** I don't accidentally add the same book twice
**Acceptance Criteria:**
- Compare title + author (case-insensitive, trimmed)
- Show confirmation dialog if match found
- User can override and add anyway (for different editions)
## 3. Business Rules
### BR-001: Status Validation
**Given** a user sets book status
**When** the status value is provided
**Then** it must be one of: unread, reading, completed
**And** invalid values return error code `INVALID_STATUS`
### BR-002: Rating Validation
**Given** a book with status "completed"
**When** a user assigns a rating
**Then** system accepts integers 1-5 (inclusive)
**And** rejects other values with error code `INVALID_RATING`
**Given** a book with status "unread" or "reading"
**When** a user attempts to assign a rating
**Then** system rejects with error code `RATING_NOT_ALLOWED`
### BR-003: Duplicate Detection
**Given** a book with title "Clean Code" and author "Robert Martin" exists
**When** user adds book with same title and author (case-insensitive)
**Then** system prompts: "This book may already exist. Add anyway?"
**And** user can confirm or cancel
## 4. Data Model (Conceptual)
### Book Entity
- `id`: string (UUID, system-generated)
- `title`: string (required, 1-500 chars)
- `author`: string (required, 1-200 chars)
- `status`: enum (unread | reading | completed, default: unread)
- `rating`: integer (1-5, nullable, only for completed books)
- `date_added`: timestamp (system-generated)
- `date_completed`: timestamp (nullable, set when status changes to completed)
## 5. API Contract
### GET /api/v1/books
**Purpose:** Retrieve all books
**Query Parameters:**
- `status` (optional): filter by unread|reading|completed
  **Success Response (200):**
  {
  "books": [
  {
  "id": "uuid-string",
  "title": "Clean Code",
  "author": "Robert Martin",
  "status": "completed",
  "rating": 5,
  "date_added": "2024-02-19T10:00:00Z",
  "date_completed": "2024-02-20T15:30:00Z"
  }
  ],
  "total": 1
  }
### POST /api/v1/books
**Purpose:** Add a new book
**Request Body:**
{
"title": "Clean Code",
"author": "Robert Martin",
"status": "unread"
}
**Success Response (201):**
{
"id": "uuid-string",
"message": "Book added successfully"
}
**Error Responses:**
- 400: Validation errors (missing fields, invalid status)
- 409: Duplicate book detected (returns existing book details)
### PUT /api/v1/books/:id/rating
**Purpose:** Set or update rating for a completed book
**Request Body:**
{
"rating": 5
}
**Success Response (200):**
{
"message": "Rating updated successfully"
}
**Error Responses:**
- 400: Invalid rating (not 1-5) or book not completed
- 404: Book not found
## 6. Out of Scope (Phase 1)
- ❌ Multi-user support / authentication
- ❌ Book cover image uploads
- ❌ Integration with Goodreads/Amazon APIs
- ❌ Social features (sharing lists, reviews)
- ❌ Mobile app (web-only)
- ❌ Advanced search (full-text, tags)
## 7. Decision Log
### DEC-001: Duplicate Handling
**Date:** [MM/DD/YYYY]
**Decision:** Case-insensitive comparison of title + author
**Rationale:** Users may capitalize differently; strict match would be frustrating
### DEC-002: Rating Editing
**Date:** [MM/DD/YYYY]
**Decision:** Ratings can be changed after initial assignment
**Rationale:** Opinions change; flexibility improves UX