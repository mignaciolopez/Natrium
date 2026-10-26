# Project Constitution: ProjectName
## Tech Stack
- **Backend:** [Your language/framework: Node.js + Express / Python + FastAPI / Java + Spring Boot]
- **Database:** SQLite with simple ORM (keep it lightweight)
- **Frontend:** [Your framework: React / Angular / Vue / Plain HTML+JS]
## Architecture Principles
- Layered architecture: Controllers → Services → Data Access
- Dependency injection where applicable
- Single Responsibility Principle for all classes/modules
## Boundaries
### ✅ ALWAYS DO:
- Write unit tests for all business logic before finalizing a task
- Follow RESTful API naming conventions (use `/api/v1/` prefix)
- Use explicit error handling and return standard HTTP status codes
- Add comments for all public interfaces
- Use async/await (not callbacks) for asynchronous operations
- Validate all user inputs
### ⚠️ ASK FIRST:
- Before adding any new third-party dependencies/libraries
- Before modifying the database schema
- Before adding new entities/tables
- Before changing architectural patterns
### 🚫 NEVER DO:
- Never hardcode secrets, API keys, or sensitive data in code
- Never skip writing tests for business logic
- Never use SELECT * in production code
- Never store passwords in plain text
- Never add features not in spec.md
## Code Style
- Naming: camelCase for variables/functions, PascalCase for classes
- Max function length: 50 lines
- Max file length: 300 lines
- Descriptive variable names (minimum 3 characters)
## Error Handling Standards
All errors must return:
{
"error": "Human-readable message",
"code": "ERROR_CODE",
"timestamp": "ISO-8601"
}
Status codes:
- 200: Success with data
  None
- 201: Resource created
- 400: Validation errors (return field-specific messages)
- 404: Resource not found
- 500: Unexpected server errors
## Testing Requirements
- Minimum 80% code coverage
- Test file naming: `[filename].test.[ext]` or `[filename].spec.[ext]`
- Use AAA pattern: Arrange, Act, Assert
- Mock external dependencies (DB, APIs)
## API Standards
- All endpoints prefixed with `/api/v1/`
- RESTful conventions (GET/POST/PUT/DELETE)
- Consistent response format